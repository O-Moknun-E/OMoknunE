using System;
using System.Collections.Generic;
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
        public int row;                 // 변경된 행
        public int col;                 // 변경된 열
        public StoneType stateBefore;   // 변경 전 상태(돌)
        public StoneType stateAfter;    // 변경 후 상태(돌)

        public CellChange(int row, int col, StoneType before, StoneType after)
        {
            this.row = row;
            this.col = col;
            this.stateBefore = before;
            this.stateAfter = after;
        }
    }

    /// <summary>
    /// 한 턴 내에서 발생하는 행동 데이터
    /// </summary>
    [Serializable]
    public struct ActionData
    {
        public ActionType actionType;         // 행동 종류 (마법 사용, 돌 착수)
        public int magicId;                   // 사용한 마법 ID (마법 사용 시에만 유효)
        public int targetRow;                 // 대상 행
        public int targetCol;                 // 대상 열
        public List<CellChange> boardChanges; // 행동으로 인한 보드 변화

        /// <summary>
        /// 마법 사용 행동 생성
        /// </summary>
        public static ActionData CreateMagicAction(int magicId, int targetRow, int targetCol, List<CellChange> changes, float timestamp) =>
            new ActionData
            {
                actionType = ActionType.UseMagic,
                magicId = magicId,
                targetRow = targetRow,
                targetCol = targetCol,
                boardChanges = changes ?? new List<CellChange>() // 만약 보드판에 변화가 없는 마법일 수 있으니 null 일경우 빈 리스트로 초기화
            };

        /// <summary>
        /// 착수 행동 생성
        /// </summary>
        public static ActionData CreatePlaceStoneAction(int row, int col, StoneType stoneType, float timestamp) =>
            new ActionData
            {
                actionType = ActionType.PlaceStone,
                magicId = -1, // 착수는 마법이 아니므로 -1로 설정
                targetRow = row,
                targetCol = col,
                boardChanges = new List<CellChange> {
                    new CellChange(row, col, StoneType.Empty, stoneType) // 빈 공간 -> 착수된 돌
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

    private ReplayData _currentReplay; // 현재 리플레이
    private TurnData _currentTurn;     // 현재 리플레이의 현재 턴 데이터
    private int _currentTurnIndex;     // 현재 리플레이의 턴 인덱스
    private int _currentActionIndex;   // 현재 리플레이의 특정 턴의 현재 행동 인덱스
    private bool _isRecording;         // 리플레이 기록 여부
    private bool _isReplaying;         // 리플레이 재생 여부
    private float _replaySpeed = 1f;   // 리플레이 재생 속도(배속)
    private float _gameStartTime;      // 게임 시작 시간

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
    /// <param name="magicId">사용된 마법의 ID</param>
    /// <param name="targetRow">마법이 적용된 행</param>
    /// <param name="targetCol">마법이 적용된 열</param>
    /// <param name="boardBefore">마법 사용 전 보드 상태</param>
    /// <param name="boardAfter">마법 사용 후 보드 상태</param>
    public void RecordUseMagic(int magicId, int targetRow, int targetCol, StoneType[,] boardBefore, StoneType[,] boardAfter)
    {
        // 기록 중이 아니면 무시
        if (!_isRecording) return;

        // 보드 변화 감지
        var changes = DetectBoardChanges(boardBefore, boardAfter);

        // 마법 사용 데이터 기록 후 행동 리스트에 추가
        ActionData action = ActionData.CreateMagicAction(
            magicId,
            targetRow,
            targetCol,
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
    public void EndRecording(PlayerType winner) {
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
    private List<CellChange> DetectBoardChanges(StoneType[,] before, StoneType[,] after)
    {
        List<CellChange> changes = new();

        int rows = before.GetLength(0);
        int cols = before.GetLength(1);

        for(int r = 0; r < rows; r++)
        {
            for(int c = 0; c < cols; c++)
            {
                if (before[r, c] != after[r, c])
                {
                    changes.Add(new CellChange(r, c, before[r, c], after[r, c]));
                }
            }
        }

        return changes;
    }

    #endregion

    #region Playback Methods

    /// <summary>
    /// 리플레이 시작
    /// </summary>
    public void StartReplay(ReplayData replayData) { 
        // 리플레이 데이터가 유효한지 확인
        if(replayData == null)
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
        if(_currentActionIndex < turn.actions.Count)
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
        if(_currentTurnIndex > 0)
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
    /// 리플레이 속도 설정
    /// </summary>
    /// <param name="speed"></param>
    public void SetReplaySpeed(float speed)
    {
        // 0.25 ~ 4배속으로 제한
        _replaySpeed = Mathf.Clamp(speed, 0.25f, 4f);
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

    // 플레이팹 연동해서 하기

    #endregion
}
