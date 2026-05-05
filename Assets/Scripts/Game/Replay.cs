using PlayFab;
using PlayFab.ClientModels;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// 오목 게임의 리플레이 기능을 담당하는 클래스
/// </summary>
public class Replay : MonoBehaviour
{
    /// <summary>
    /// 오목판의 한 칸의 상태 변화
    /// </summary>
    [Serializable]
    public struct CellChange
    {
        public int row;                     // 변경된 행
        public int col;                     // 변경된 열
        public StoneType before;            // 변경 전 상태(돌)
        public StoneType after;             // 변경 후 상태(돌)
        public CellChangeType changeType;   // 변경 종류 (착수, 제거, 시각 효과)

        // row와 col는 필요없을때 -1
        public CellChange(int row, int col, StoneType before, StoneType after, CellChangeType changeType)
        {   
            this.row = row;
            this.col = col;
            this.before = before;
            this.after = after;
            this.changeType = changeType;
        }
    }

    /// <summary>
    /// 한 턴 내에서 발생하는 행동 데이터
    /// </summary>
    [Serializable]
    public struct ActionData
    {
        public ActionType actionType;         // 행동 종류 (마법 사용, 돌 착수)
        public int magicID;                   // 사용한 마법 ID (마법 사용 시에만 유효)
        public int targetRow;                 // 대상 행 (마법은 skillContext 안에)
        public int targetCol;                 // 대상 열 (마법은 skillContext 안에)
        public SkillContext skillContext;     // 마법 사용 시의 스킬 컨텍스트 (타겟 행열, 시전자, 스킨 ID)
        public List<CellChange> boardChanges; // 행동으로 인한 보드 변화

        /// <summary>
        /// 마법 사용 행동 생성
        /// </summary>
        public static ActionData CreateMagicAction(int magicID, SkillContext skillContext, List<CellChange> changes, float timestamp) =>
            new ActionData
            {
                actionType = ActionType.UseMagic,
                magicID = magicID,
                skillContext = skillContext,
                boardChanges = changes ?? new List<CellChange>() // 만약 보드판에 변화가 없는 마법일 수 있으니 null 일경우 빈 리스트로 초기화
            };

        /// <summary>
        /// 착수 행동 생성
        /// </summary>
        public static ActionData CreatePlaceStoneAction(int row, int col, StoneType stoneType, float timestamp) =>
            new ActionData
            {
                actionType = ActionType.PlaceStone,
                magicID = -1, // 착수는 마법이 아니므로 -1로 설정
                targetRow = row,
                targetCol = col,
                boardChanges = new List<CellChange> {
                    new CellChange(row, col, StoneType.Empty, stoneType, CellChangeType.PlaceStone) // 빈 공간 -> 착수된 돌
                }
            };
    }

    /// <summary>
    /// 한 턴의 전체 데이터
    /// </summary>
    [Serializable]
    public struct TurnData
    {
        public PlayerType playerType;    // 행동한 플레이어(흑, 백)
        public List<ActionData> actions; // 한 턴 내에서 발생한 행동들
        public float turnStartTime;      // 턴 시작 시간(초)
        public float turnEndTime;        // 턴 종료 시간(초)

        public TurnData(PlayerType playerType, float startTime)
        {
            this.playerType = playerType;
            this.actions = new List<ActionData>();
            this.turnStartTime = startTime;
            this.turnEndTime = 0f;
        }
    }

    /// <summary>
    /// 한 게임의 전체 리플레이 데이터
    /// </summary>
    [Serializable]
    public class ReplayData
    {
        public List<TurnData> turns = new List<TurnData>(); // 게임에서 발생한 모든 턴 데이터
        public string blackPlayerName;                      // 흑 플레이어 이름
        public string whitePlayerName;                      // 백 플레이어 이름
        public PlayerType winner;                           // 게임 승자(흑, 백)
        public float totalGameTime;                         // 게임 전체 시간(초)
        public string gameDate;                             // 게임이 진행된 날짜 및 시간

        public ReplayData()
        {
            gameDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        }
    }

    private ReplayData _currentReplay;  // 현재 리플레이
    private TurnData _currentTurn;      // 현재 리플레이의 현재 턴 데이터
    private int _currentTurnIndex;      // 현재 리플레이의 턴 인덱스
    private int _currentActionIndex;    // 현재 리플레이의 특정 턴의 현재 행동 인덱스
    private bool _isRecording;          // 리플레이 기록 여부
    private bool _isReplaying;          // 리플레이 재생 여부
    private float _replayInterval = 1f; // 리플레이 재생 간격(턴 당 초)
    private float _gameStartTime;       // 게임 시작 시간

    public int CurrentTurnIndex => _currentTurnIndex;
    public int TotalTurns => _currentReplay?.turns.Count ?? 0;
    public float ReplayInterval => _replayInterval;
    public bool IsReplaying => _isReplaying;

    #region Recording Methods

    /// <summary>
    /// 게임 시작 시 리플레이 기록을 시작하는 메서드
    /// </summary>
    public void StartRecording(string blackPlayerName, string whitePlayerName)
    {
        _currentReplay = new ReplayData
        {
            blackPlayerName = blackPlayerName,
            whitePlayerName = whitePlayerName
        };

        _isRecording = true;
        _gameStartTime = Time.time;
    }

    /// <summary>
    /// 특정 플레이어의 턴 시작 기록
    /// </summary>
    public void StartTurn(PlayerType playerType)
    {
        // 기록 중이 아니면 무시
        if (!_isRecording) return;

        _currentTurn = new TurnData(playerType, Time.time - _gameStartTime);
    }

    /// <summary>
    /// 사용한 마법을 기록
    /// </summary>
    /// <param name="magicID">사용된 마법의 ID</param>
    /// <param name="skillContext">마법 컨텍스트</param>
    /// <param name="boardBefore">마법 사용 전 보드 상태</param>
    /// <param name="boardAfter">마법 사용 후 보드 상태</param>
    public void RecordUseMagic(int magicID, SkillContext skillContext, StoneType[,] boardBefore, StoneType[,] boardAfter)
    {
        // 기록 중이 아니면 무시
        if (!_isRecording) return;

        // 마법 정보 가져오기
        IMagic magic = MagicRegistry.Instance.GetMagicByID(magicID);

        // 마법 정보가 없으면 시각 효과로 간주
        CellChangeType changeType = magic != null ? magic.ChangeType : CellChangeType.VisualEffect;

        // 보드 변화 감지
        var changes = DetectBoardChanges(skillContext.TargetY, skillContext.TargetX, boardBefore, boardAfter, changeType);

        // 마법 사용 데이터 기록 후 행동 리스트에 추가
        ActionData action = ActionData.CreateMagicAction(
            magicID,
            skillContext,
            changes,
            Time.time - _gameStartTime
        );

        _currentTurn.actions.Add(action);
    }

    /// <summary>
    /// 착수 기록
    /// </summary>
    public void RecordPlaceStone(int row, int col, StoneType stoneType)
    {
        // 기록 중이 아니면 무시
        if (!_isRecording) return;

        // 착수 데이터 기록 후 행동 리스트에 추가
        ActionData action = ActionData.CreatePlaceStoneAction(
            row,
            col,
            stoneType,
            Time.time - _gameStartTime
        );

        _currentTurn.actions.Add(action);
    }

    /// <summary>
    /// 특정 플레이어의 턴 종료 기록
    /// </summary>
    public void EndTurn()
    {
        // 기록 중이 아니면 무시
        if (!_isRecording) return;

        // 턴 종료 시간 기록
        _currentTurn.turnEndTime = Time.time - _gameStartTime;

        // 현재 턴 데이터를 리플레이의 턴 리스트에 추가
        _currentReplay.turns.Add(_currentTurn);
    }

    /// <summary>
    /// 게임 종료 시 리플레이 기록을 마무리
    /// </summary>
    public void EndRecording(PlayerType winner)
    {
        // 기록 중이 아니면 무시
        if (!_isRecording) return;

        _currentReplay.winner = winner;
        _currentReplay.totalGameTime = Time.time - _gameStartTime;
        _isRecording = false;
    }

    /// <summary>
    /// 보드 상태를 비교하여 변화된 칸 감지
    /// </summary>
    /// <param name="before">행동 전 보드 상태</param>
    /// <param name="after">행동 후 보드 상태</param>
    /// <returns>변화된 칸 정보 반환</returns>
    private List<CellChange> DetectBoardChanges(int targetRow, int targetCol, StoneType[,] before, StoneType[,] after, CellChangeType changeType)
    {
        List<CellChange> changes = new();

        int rows = before.GetLength(0);
        int cols = before.GetLength(1);

        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                if (before[r, c] != after[r, c])
                {
                    changes.Add(new CellChange(r, c, before[r, c], after[r, c], changeType));
                }
            }
        }

        // 보드 변화 없이 시각 효과만 있는 마법
        if(changes.Count == 0 && changeType == CellChangeType.VisualEffect)
        {
            changes.Add(new CellChange(targetRow, targetCol, StoneType.Empty, StoneType.Empty, changeType));
        }

        return changes;
    }

    #endregion

    #region Playback Methods

    /// <summary>
    /// 리플레이 시작
    /// </summary>
    public void StartReplay(ReplayData replayData)
    {
        // 리플레이 데이터가 유효한지 확인
        if (replayData == null)
        {
            Debug.LogError("리플레이 데이터가 없습니다.");
            return;
        }

        _currentReplay = replayData;
        _currentTurnIndex = 0;
        _currentActionIndex = 0;
        _isReplaying = true;
    }

    /// <summary>
    /// 다음 행동 재생
    /// </summary>
    /// <returns>다음 행동 데이터 반환, 없으면 null</returns>
    public ActionData? PlayNextAction(out PlayerType currentPlayer)
    {
        // 기본 값으로 흑 설정
        currentPlayer = PlayerType.Black;

        // 리플레이 중이 아니거나 현재 리플레이 데이터가 없거나 모든 턴을 재생한 경우 null 반환
        if (!_isReplaying || _currentReplay == null || _currentTurnIndex >= _currentReplay.turns.Count)
        {
            _isReplaying = false;
            return null;
        }

        // 현재 턴 정보 가져오기
        TurnData turn = _currentReplay.turns[_currentTurnIndex];
        currentPlayer = turn.playerType;

        // 현재 턴에 남은 행동이 있으면 꺼내서 반환
        if (_currentActionIndex < turn.actions.Count)
        {
            // 행동 가져온 후 인덱스 증가 및 행동 반환
            ActionData action = turn.actions[_currentActionIndex];
            _currentActionIndex++;
            return action;
        }

        // 현재 턴의 모든 행동을 재생했으면 다음 턴으로 넘어가기
        _currentTurnIndex++;
        _currentActionIndex = 0;

        // 다음 턴의 첫 행동 반환
        return PlayNextAction(out currentPlayer);
    }

    /// <summary>
    /// 다음 턴 재생
    /// </summary>
    /// <returns>다음 턴 데이터 반환, 없으면 null</returns>
    public TurnData? PlayNextTurn()
    {
        // 리플레이 중이 아니거나 현재 리플레이 데이터가 없거나 모든 턴을 재생한 경우 null 반환
        if (!_isReplaying || _currentReplay == null || _currentTurnIndex >= _currentReplay.turns.Count)
        {
            _isReplaying = false;
            return null;
        }

        // 현재 턴 정보 가져오고 턴 인덱스 증가 및 행동 인덱스는 초기화
        TurnData turn = _currentReplay.turns[_currentTurnIndex];
        _currentTurnIndex++;
        _currentActionIndex = 0;

        return turn;
    }

    /// <summary>
    /// 이전 턴으로 이동
    /// </summary>
    public void PlayPreviousTurn()
    {
        // 리플레이가 없으면 무시
        if (_currentReplay == null) return;

        // 이전 턴이 존재 할때만
        if (_currentTurnIndex > 0)
        {
            _currentTurnIndex--;
            _currentActionIndex = 0;
        }
    }

    /// <summary>
    /// 특정 턴으로 이동
    /// </summary>
    /// <param name="turnIndex"></param>
    public void JumpToTurn(int turnIndex)
    {
        // 리플레이가 없으면 무시
        if (_currentReplay == null) return;

        // 안전하게 Clamp로 범위내에 맞게 적용
        _currentTurnIndex = Mathf.Clamp(turnIndex, 0, _currentReplay.turns.Count);
        _currentActionIndex = 0;
    }

    /// <summary>
    /// 리플레이 일시정지/재개
    /// </summary>
    public void TogglePause()
    {
        _isReplaying = !_isReplaying;
    }

    /// <summary>
    /// 리플레이 간격 설정
    /// </summary>
    /// <param name="interval"></param>
    public void SetReplayInterval(float interval)
    {
        // 0.25 ~ 4초로 제한
        _replayInterval = Mathf.Clamp(interval, 0.25f, 4f);
    }

    /// <summary>
    /// 리플레이 중지
    /// </summary>
    public void StopReplay()
    {
        _isReplaying = false;
        _currentTurnIndex = 0;
        _currentActionIndex = 0;
    }

    #endregion

    #region Save/Load Methods

    /// <summary>
    /// 리플레이 메타데이터 (목록 표시용)
    /// </summary>
    [Serializable]
    public class ReplayMetadata
    {
        public string replayID;          // 리플레이 고유 ID
        public string blackPlayerName;   // 흑 플레이어 이름
        public string whitePlayerName;   // 백 플레이어 이름
        public string winner;            // 게임 승자
        public float totalGameTime;      // 게임 전체 시간(초)
        public string gameDate;          // 게임이 진행된 날짜 및 시간
        public long createdAt;          // 리플레이가 PlayFab에 저장된 시간
    }

    /// <summary>
    /// 리플레이 목록 데이터(PlayFab에서 다른 데이터와 구별하기 위해 사용)
    /// </summary>
    [Serializable]
    public class ReplayListData
    {
        public List<ReplayMetadata> replays = new List<ReplayMetadata>();
    }

    [SerializeField] private int _maxReplayCount = 10; // 저장 가능한 최대 리플레이 수

    /// <summary>
    /// PlayFab에 리플레이 저장
    /// </summary>
    /// <param name="onSuccess">저장 성공 시 호출될 콜백</param>
    /// <param name="onError">저장 실패 시 호출될 콜백</param>
    public void SaveReplayToPlayFab(Action onSuccess = null, Action<string> onError = null)
    {
        // 리플레이가 없으면 무시
        if (_currentReplay == null)
        {
            onError?.Invoke("저장할 리플레이 데이터가 없습니다.");
            return;
        }

        // 리플레이 목록 가져오기
        GetReplayListFromPlayFab(
            onSuccess: (list) =>
            {
                // 저장 가능한 최대 리플레이 수 이상이면 가장 오래된 리플레이 삭제하기
                if (list.Count >= _maxReplayCount)
                {
                    // 생성 시간 기준으로 정렬해서 가장 오래된 항목 찾기
                    var oldestReplay = list.OrderBy(x => x.createdAt).First();

                    Debug.Log($"리플레이 개수 초과. 가장 오래된 리플레이 삭제: {oldestReplay.replayID}");

                    // 가장 오래된 리플레이 삭제 후 새 리플레이 저장
                    DeleteReplayFromPlayFab(oldestReplay.replayID,
                        onSuccess: () => SaveNewReplay(onSuccess, onError),
                        onError: (error) => SaveNewReplay(onSuccess, onError)); // 삭제 실패해도 일단 저장
                }
                else
                {
                    // 10개 미만이면 바로 저장
                    SaveNewReplay(onSuccess, onError);
                }
            },
            onError: (error) =>
            {
                // 리플레이 목록 가져오기 실패해도 일단 저장
                Debug.LogWarning($"리플레이 목록 가져오기 실패, 리플레이 저장 시도: {error}");
                SaveNewReplay(onSuccess, onError);
            });
    }

    /// <summary>
    /// 새 리플레이 저장
    /// </summary>
    /// <param name="onSuccess">저장 성공 시 호출될 콜백</param>
    /// <param name="onError">저장 실패 시 호출될 콜백</param>
    private void SaveNewReplay(Action onSuccess, Action<string> onError)
    {
        // JSON으로 직렬화
        string json = JsonUtility.ToJson(_currentReplay);

        // 리플레이 ID 생성(PlayFab ID + 타임스탬프 + 랜덤값)
        string playerID = PlayFabSettings.staticPlayer.PlayFabId;
        string timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
        string randomSuffix = UnityEngine.Random.Range(1000, 9999).ToString();
        string replayID = $"replay_{playerID}_{timestamp}_{randomSuffix}";

        // PlayFab Player Data에 저장
        var request = new UpdateUserDataRequest
        {
            Data = new Dictionary<string, string>
            {
                { replayID, json }
            },
            Permission = UserDataPermission.Private // 본인만 볼 수 있게
        };

        // PlayFab으로 요청한 사용자 데이터 업데이트
        PlayFabClientAPI.UpdateUserData(request,
            result =>
            {
                Debug.Log($"리플레이 저장 성공: {replayID}");

                // 메타데이터 목록에 추가
                AddToReplayList(replayID, onSuccess, onError);
            },
            error =>
            {
                Debug.LogError($"리플레이 저장 실패: {error.ErrorMessage}");
                onError?.Invoke(error.ErrorMessage);
            });
    }

    /// <summary>
    /// PlayFab에서 본인의 모든 리플레이 목록 메타데이터 가져오기
    /// </summary>
    /// <param name="onSuccess">성공 시 호출될 콜백</param>
    /// <param name="onError">실패 시 호출될 콜백</param>
    public void GetReplayListFromPlayFab(Action<List<ReplayMetadata>> onSuccess = null, Action<string> onError = null)
    {
        // 요청 리스트
        var request = new GetUserDataRequest
        {
            Keys = new List<string> { "replay_list" }
        };

        // PlayFab에서 요청한 사용자 데이터 가져오기
        PlayFabClientAPI.GetUserData(request,
            result =>
            {
                // 리플레이 목록이 존재하는 경우
                if (result.Data.ContainsKey("replay_list"))
                {
                    // JSON 문자열로 저장된 리플레이 목록을 객체로 역직렬화
                    string json = result.Data["replay_list"].Value;
                    ReplayListData listData = JsonUtility.FromJson<ReplayListData>(json);

                    Debug.Log($"리플레이 목록 메타데이터 불러오기 성공: {listData.replays.Count}개");

                    // 성공 콜백 호출
                    onSuccess?.Invoke(listData.replays);
                }
                else
                {
                    // 리플레이 목록이 없는 경우 빈 리스트 반환
                    Debug.Log($"리플레이 목록 메타데이터가 없습니다.");
                    onSuccess?.Invoke(new List<ReplayMetadata>());
                }
            },
            error =>
            {
                Debug.LogError($"리플레이 목록 메타데이터 불러오기 실패: {error.ErrorMessage}");
                onError?.Invoke(error.ErrorMessage);
            });
    }

    /// <summary>
    /// PlayFab에서 특정 리플레이 데이터 불러오기
    /// </summary>
    /// <param name="replayID">리플레이 ID</param>
    /// <param name="onSuccess">성공 시 호출될 콜백</param>
    /// <param name="onError">실패 시 호출될 콜백</param>
    public void LoadReplayFromPlayFab(string replayID, Action<ReplayData> onSuccess = null, Action<string> onError = null)
    {
        // 요청 리스트
        var request = new GetUserDataRequest
        {
            Keys = new List<string> { replayID }
        };

        // PlayFab에서 요청한 사용자 데이터 가져오기
        PlayFabClientAPI.GetUserData(request,
            result =>
            {
                // 해당 리플레이가 존재하는 경우
                if(result.Data.ContainsKey(replayID))
                {
                    // JSON 문자열로 저장된 리플레이 데이터를 객체로 역직렬화
                    string json = result.Data[replayID].Value;
                    ReplayData replayData = JsonUtility.FromJson<ReplayData>(json);

                    Debug.Log($"리플레이 불러오기 성공: {replayID}");

                    // 성공 콜백 호출
                    onSuccess?.Invoke(replayData);
                } else
                {
                    // 리플레이 데이터가 없는 경우
                    string errorMsg = $"리플레이를 찾을 수 없습니다: {replayID}";

                    Debug.LogError(errorMsg);
                    onError?.Invoke(errorMsg);
                }
            },
            error =>
            {
                Debug.LogError($"리플레이 불러오기 실패: {error.ErrorMessage}");
                onError?.Invoke(error.ErrorMessage);
            });
    }

    /// <summary>
    /// 리플레이 목록에 메타데이터 추가
    /// </summary>
    private void AddToReplayList(string replayID, Action onSuccess, Action<string> onError)
    {
        GetReplayListFromPlayFab(
            onSuccess: (list) =>
            {
                // 새 메타데이터 추가
                var metadata = new ReplayMetadata
                {
                    replayID = replayID,
                    blackPlayerName = _currentReplay.blackPlayerName,
                    whitePlayerName = _currentReplay.whitePlayerName,
                    winner = _currentReplay.winner.ToString(),
                    totalGameTime = _currentReplay.totalGameTime,
                    gameDate = _currentReplay.gameDate,
                    createdAt = DateTime.Now.Ticks
                };

                // 기존 목록에 새 메타데이터 추가
                list.Add(metadata);

                // 메타데이터 목록 저장
                SaveReplayList(list, onSuccess, onError);
            },
            onError: (error) =>
            {
                // 목록 가져오기 실패해도 일단 새 메타데이터로 생성
                var newList = new List<ReplayMetadata>
                {
                    new ReplayMetadata
                    {
                        replayID = replayID,
                        blackPlayerName = _currentReplay.blackPlayerName,
                        whitePlayerName = _currentReplay.whitePlayerName,
                        winner = _currentReplay.winner.ToString(),
                        totalGameTime = _currentReplay.totalGameTime,
                        gameDate = _currentReplay.gameDate,
                        createdAt = DateTime.Now.Ticks
                    }
                };

                // 메타데이터 목록 저장
                SaveReplayList(newList, onSuccess, onError);
            });
    }

    /// <summary>
    /// 리플레이 목록 메타데이터 저장
    /// </summary>
    /// <param name="list">리플레이 메타데이터 목록</param>
    private void SaveReplayList(List<ReplayMetadata> list, Action onSuccess, Action<string> onError)
    {
        var listData = new ReplayListData { replays = list };
        string json = JsonUtility.ToJson(listData);

        // 요청 리스트
        var request = new UpdateUserDataRequest
        {
            Data = new Dictionary<string, string>
            {
                { "replay_list", json }
            },
            Permission = UserDataPermission.Private // 본인만 볼 수 있게
        };

        PlayFabClientAPI.UpdateUserData(request,
            result =>
            {
                Debug.Log("리플레이 목록 메타데이터 저장 성공");
                onSuccess?.Invoke();
            },
            error =>
            {
                Debug.LogError($"리플레이 목록 저장 실패: {error.ErrorMessage}");
                onError?.Invoke(error.ErrorMessage);
            });
    }

    /// <summary>
    /// PlayFab에서 리플레이 삭제
    /// </summary>
    /// <param name="replayID">리플레이 ID</param>
    /// <param name="onSuccess">성공 시 호출될 콜백</param>
    /// <param name="onError">실패 시 호출될 콜백</param>
    public void DeleteReplayFromPlayFab(string replayID, Action onSuccess = null, Action<string> onError = null)
    {
        // 요청 리스트
        var request = new UpdateUserDataRequest
        {
            KeysToRemove = new List<string> { replayID }
        };

        // PlayFab에서 요청한 사용자 데이터 삭제
        PlayFabClientAPI.UpdateUserData(request,
            result =>
            {
                Debug.Log($"리플레이 삭제 성공: {replayID}");

                // 메타데이터 목록에서도 제거
                RemoveFromReplayList(replayID, onSuccess, onError);
            },
            error =>
            {
                Debug.LogError($"리플레이 삭제 실패: {error.ErrorMessage}");
                onError?.Invoke(error.ErrorMessage);
            });
    }

    /// <summary>
    /// 리플레이 목록에서 메타데이터 제거
    /// </summary>
    private void RemoveFromReplayList(string replayID, Action onSuccess, Action<string> onError)
    {
        GetReplayListFromPlayFab(
            onSuccess: (list) =>
            {
                // 일치하는 replayID를 가진 메타데이터 제거
                list.RemoveAll(x => x.replayID == replayID);
                SaveReplayList(list, onSuccess, onError);
            },
            onError: onError);
    }

    #endregion
}
