using Photon.Pun;
using System;
using UnityEngine;

/// <summary>
/// 오목 게임의 전반적인 관리를 담당하는 싱글톤 클래스
/// </summary>
public class OmokManager : SceneSingleton<OmokManager>
{
    [Tooltip("마나 획득에 걸리는 시간(초)")]
    [SerializeField] private float _manaIncomeTime = 3f;
    [Tooltip("마나를 획득할 때 받는 양")]
    [SerializeField] private int _manaIncome = 1;
    [Tooltip("흑 초기 마나")]
    [SerializeField] private int _manaBlack = 0;
    [Tooltip("백 초기 마나")]
    [SerializeField] private int _manaWhite = 0;
    [Tooltip("착수 제한시간")]
    [SerializeField] private float _turnTimeLimit = 30f;
    [Tooltip("리플레이 시스템")]
    [SerializeField] private Replay _replay;

    //==========================================
    //======================================추가된 부분
    // 외부에서 남은 시간을 계산할 수 있도록 열기 (시간 UI 표시용)
    public float TurnTimer => _turnTimer;
    public float TurnTimeLimit => _turnTimeLimit;
    //==========================================

    public static readonly int BoardSize = 15;   // 오목판의 크기 (15x15)

    private BoardInteraction _boardInteraction;  // 보드 인터랙션 참조
    private Sprite _playerStone;                 // 플레이어 돌 스프라이트 (AI 모드에서 플레이어 돌 스킨 변경용)
    private IMagic _loadedSkill;                  // AI 모드 전용 현재 장전된 스킬

    private GameMode _gameMode;         // 게임 모드 (PvP, PvE)
    private AIPlayer _aiPlayer;         // AI 플레이어 (PvE 모드에서 사용)
    private AIDifficulty _aiDifficulty; // AI 난이도 설정 (PvE 모드에서 사용)

    private StoneType[,] _board;        // 오목판 상태를 저장하는 2D 배열
    private Player[] _players;          // 플레이어 배열 (0: 흑, 1: 백)
    private IOmokRule _rule;            // 오목 게임 규칙
    private StoneType _currentTurn;     // 현재 턴의 돌 색상
    private PlayerType _myPlayerType;   // 내 플레이어 타입
    private float _turnTimer;           // 턴 타이머
    private float[] _manaIncomeTimer;   // 마나 획득 타이머 (0: 흑, 1: 백)
    private bool _isGameOver;           // 게임 종료 여부

    //==========================================
    //======================================추가된 부분
    private double _turnStartNetworkTime;            // 포톤 서버 시간 기준으로 턴이 시작된 시간 기록
    private double _lastNetworkTime;                 // 한 프레임동안 흐른 서버 시간 기록 (마나 획득 타이머 계산용)
    //==========================================

    public event Action OnUsedMagic;            // 마법이 사용되었을 때 발생하는 이벤트
    public event Action OnManaChanged;          // 마나가 변경되었을 때 발생하는 이벤트
    public event Action<StoneType> OnGameOver;  // 게임이 종료되었을 때 발생하는 이벤트(승리한 돌 전달)

    ///////////////////////////////////////////////////////////////////////////////////
    // OmokManager 내부에 추가된 부분
    public void SetBoardData(int x, int y, StoneType type) => _board[y, x] = type;
    public StoneType GetBoardData(int x, int y) => _board[y, x];
    public Player GetPlayer(PlayerType type) => _players[(int)type];

    ///////////////////////////////////////////////////////////////////////////////////

    public bool IsMyTurn
    {
        get
        {
            // PvE 모드
            if (_gameMode == GameMode.PvE)
                return _currentTurn == StoneType.Black;

            // PvP 모드
            return _currentTurn == (_myPlayerType == PlayerType.Black ? StoneType.Black : StoneType.White);
        }
    }

    //==========================================
    //======================================추가된 부분 (시간 과부하 기능)
    private int[] _overloadTurnsLeft = new int[2] { 0, 0 }; // 시간 과부하 남은 턴 수 (0: 흑, 1: 백)

    // 현재 적용되는 '실제' 턴 제한 시간 (과부하 시 절반으로 계산)
    public float CurrentTurnTimeLimit
    {
        get
        {
            int pIndex = GetPlayerIndex(true);
            return _overloadTurnsLeft[pIndex] > 0 ? _turnTimeLimit / 2f : _turnTimeLimit;
        }
    }

    // 시간 과부하를 적용하는 함수 (스킬에서 호출)
    public void ApplyTimeOverload(PlayerType targetPlayer, int turns)
    {
        int targetIndex = (targetPlayer == PlayerType.Black) ? 0 : 1;
        _overloadTurnsLeft[targetIndex] = turns;
        Debug.Log($"<color=magenta>[System] {targetPlayer} 진영에 {turns}턴 동안 시간 과부하(제한시간 절반)가 적용됩니다</color>");
    }
    //==========================================

    private void OnEnable()
    {
        // AI 모드라면 AI 매치 초기화
        if (AIMatchManager.IsAIMode)
        {
            InitAIGame();
        }
        else
        {
            InitGame();

            NetworkOmokManager.OnStonePlaced += UpdateBoardFromServer;
        }

        OnGameOver += EndGame;
    }

    private void OnDisable()
    {
        // PvP 모드라면 이벤트 해제(IsAIMode는 게임이 끝날때 false로 바뀌므로 게임모드로 체크)
        if (_gameMode == GameMode.PvP)
        {
            NetworkOmokManager.OnStonePlaced -= UpdateBoardFromServer;
        }
        // PvE 모드
        else
        {
            // 보드 인터랙션 이벤트 해제
            _boardInteraction.OnStoneClicked -= OnBoardClickedAI;
        }

        OnGameOver -= EndGame;
    }

    private void Update()
    {
        // 게임이 종료된 상태에서는 X
        if (_isGameOver) return;

        IncomeMana();
        CheckTurnTimer();

        // AI 모드 스킬 입력
        if (_gameMode == GameMode.PvE && _currentTurn == StoneType.Black)
        {
            HandleSkillInput();
        }
    }

    // 게임 초기화
    public void InitGame()
    {
        // PvP
        _gameMode = GameMode.PvP;

        _board = new StoneType[BoardSize, BoardSize];
        _players = new Player[2];

        // 기본 오목 규칙 사용
        _rule = new StandardOmokRule();

        // 플레이어 초기화
        int index = 0;
        foreach (Photon.Realtime.Player photonPlayer in PhotonNetwork.PlayerList) // 민정추가 유저닉네임 저장
        {

            if (index >= _players.Length) break;

            int playerColor = (index == 0) ? _manaBlack : _manaWhite;

            _players[index] = new Player(photonPlayer.NickName, playerColor);

            index++;
        }

        // 게임 상태 초기화
        _isGameOver = false;

        // 임시로 흑이 먼저 시작
        _currentTurn = StoneType.Black;
        _myPlayerType = PhotonNetwork.IsMasterClient ? PlayerType.Black : PlayerType.White;

        // 타이머 초기화
        _turnTimer = 0f;
        _manaIncomeTimer = new float[2] { 0f, 0f };

        //==========================================
        //======================================추가된 부분
        // 게임 시작 시 포톤 룸 안에 있다면 현재 서버 시간 초기화
        if (PhotonNetwork.InRoom)
        {
            _turnStartNetworkTime = PhotonNetwork.Time;
            _lastNetworkTime = PhotonNetwork.Time;
        }
        //==========================================

        // 리플레이 기록 시작
        _replay.StartRecording(_players[0].Name, _players[1].Name);

        // 첫 턴은 바로 기록
        _replay.StartTurn(_currentTurn == StoneType.Black ? PlayerType.Black : PlayerType.White);
    }

    /// <summary>
    /// AI 게임 초기화 (PvE)
    /// </summary>
    private void InitAIGame()
    {
        // 게임 모드 및 난이도 설정
        _gameMode = GameMode.PvE;
        _aiDifficulty = AIMatchManager.SelectedDifficulty;

        // 보드 및 플레이어 초기화
        _board = new StoneType[BoardSize, BoardSize];
        _players = new Player[2];

        // 기본 오목 규칙 사용
        _rule = new StandardOmokRule();

        // 플레이어 초기화(흑: 플레이어, 백: AI)
        _players[0] = new Player(PlayFabManager.Instance.UserNickName ?? "플레이어", _manaBlack);
        _players[1] = new Player($"AI ({_aiDifficulty})", _manaWhite);

        // AI 플레이어 초기화
        GameObject aiObject = new GameObject("AIPlayer");
        _aiPlayer = aiObject.AddComponent<AIPlayer>();
        _aiPlayer.Initialize(this, _aiDifficulty, StoneType.White);

        // 보드 인터랙션 초기화
        SetupBoardInteractionForAI();

        // 게임 상태 초기화
        _isGameOver = false;

        // 흑이 먼저 시작
        _currentTurn = StoneType.Black;
        _myPlayerType = PlayerType.Black;

        // 스킬 초기화
        _loadedSkill = null;

        // 타이머 초기화
        _turnTimer = 0f;
        _manaIncomeTimer = new float[2] { 0f, 0f };

        // 리플레이 기록 시작
        _replay.StartRecording(_players[0].Name, _players[1].Name);

        // 첫 턴은 바로 기록
        _replay.StartTurn(_currentTurn == StoneType.Black ? PlayerType.Black : PlayerType.White);

        Debug.Log($"<color=cyan>AI 게임 초기화 완료 - 난이도: {_aiDifficulty}</color>");
    }

    /// <summary>
    /// AI 모드용 보드 인터랙션 설정
    /// </summary>
    private void SetupBoardInteractionForAI()
    {
        _boardInteraction = FindFirstObjectByType<BoardInteraction>();

        if (_boardInteraction == null)
        {
            Debug.LogError("BoardInteraction을 찾을 수 없습니다.");
            return;
        }

        // 플레이어 돌 스킨 설정 (임시로 흑돌. 스킨 적용 기능이 생기면 변경)
        _playerStone = PlayerEquipItem.Instance.customStone; //민정추가

        if (_playerStone == null)
        {
            Debug.LogError("돌 스킨을 찾을 수 없습니다.");
            return;
        }

        _boardInteraction.ChangeStoneSkin(_playerStone);

        // 플레이어 턴 활성화
        _boardInteraction.SetMyTurn(true);

        // 착수 이벤트 구독
        _boardInteraction.OnStoneClicked += OnBoardClickedAI;
    }

    /// <summary>
    /// AI 모드 보드 클릭 처리
    /// </summary>
    private void OnBoardClickedAI(int x, int y)
    {
        // AI 모드가 아니면 무시
        if (_gameMode != GameMode.PvE) return;

        // 플레이어가 아니면 무시
        if (_currentTurn != StoneType.Black) return;

        // 스킬 장전 상태일 때
        if (_loadedSkill != null)
        {
            UseSkillAI(x, y);
            _loadedSkill = null;
            _boardInteraction.SetSkillLoadedState(false);
            return;
        }

        // 착수 시도
        if (TryPlaceStoneLocal(y, x))
        {
            _boardInteraction.PlaceStoneRemote(x, y, _playerStone);
        }
    }

    /// <summary>
    /// AI 모드 스킬 입력 처리
    /// </summary>
    private void HandleSkillInput()
    {
        // 스킬 사용 안했을때만
        if (!_players[0].UsedMagicThisTurn)
        {
            // 스킬 장전
            if (Input.GetKeyDown(KeyCode.Alpha1)) LoadSkill(0);
            else if (Input.GetKeyDown(KeyCode.Alpha2)) LoadSkill(1);
            else if (Input.GetKeyDown(KeyCode.Alpha3)) LoadSkill(2);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Alpha3))
        {
            Debug.Log("<color=red>이번 턴에 이미 스킬을 사용했습니다.</color>");
        }

        // 우클릭으로 스킬 장전 취소
        if (Input.GetMouseButtonDown(1) && _loadedSkill != null)
        {
            _loadedSkill = null;
            _boardInteraction.SetSkillLoadedState(false);
            Debug.Log("<color=red>스킬 장전 취소</color>");
        }
    }

    /// <summary>
    /// 스킬 ID로 마법 장전
    /// </summary>
    private void LoadSkill(int skillID)
    {
        IMagic skill = MagicRegistry.Instance.GetMagicByID(skillID);

        if (skill != null)
        {
            _loadedSkill = skill;
            _boardInteraction.SetSkillLoadedState(true);
            Debug.Log($"<color=cyan>{skill.Name} 장전 완료</color>");
        }
    }

    /// <summary>
    /// AI 모드 스킬 사용
    /// </summary>
    private void UseSkillAI(int x, int y)
    {
        SkillBase skill = _loadedSkill as SkillBase;
        if (skill == null) return;

        // 타겟 설정
        skill.SetTarget(x, y, PlayerType.Black, 0);

        // 사용 시도. 가능하면 사용
        if (TryUseMagic(skill))
        {
            Debug.Log($"<color=cyan>{skill.Name} 사용: ({x}, {y})</color>");
        }
    }

    //// 돌을 놓을 수 있는지 여부
    //// 놓을 수 있다면 ture, 놓을 수 없다면 false 반환
    //public bool TryPlaceStone(int row, int col)
    //{
    //    // 게임이 종료된 상태에선 안되게
    //    if (_isGameOver) return false;

    //    // 정한 규칙대로 돌을 놓을 수 있는지 체크
    //    if (_rule.CanPlaceStone(_board, row, col, _currentTurn))
    //    {
    //        // 가능하다면 실제로 돌을 놓기
    //        ExecutePlacement(row, col);
    //        return true;
    //    }

    //    return false;
    //}

    // 마법을 사용할 수 있는지 여부
    // 사용할 수 있다면 true, 사용할 수 없다면 false 반환
    public bool TryUseMagic(IMagic magic)
    {
        // 게임이 종료된 상태에선 안되게
        if (_isGameOver) return false;

        int pIndex = GetPlayerIndex(true);
        Player player = _players[pIndex];

        // 해당 플레이어가 마법 사용 가능하면
        if (player.TryUseMagic(magic.Cost))
        {
            // 마법 사용 전 보드 상태 저장
            StoneType[,] boardBefore = GetBoardCopy();

            // 마법 실제 사용
            magic.Execute(false);

            // 마법 사용 후 보드 상태 저장
            StoneType[,] boardAfter = GetBoardCopy();

            // 리플레이 - 마법 사용 기록
            _replay.RecordUseMagic(magic.ID, ((SkillBase)magic).CurrentContext, boardBefore, boardAfter);

            // 마법 사용 이벤트
            OnUsedMagic?.Invoke();
            return true;
        }

        return false;
    }

    /// <summary>
    /// 현재 보드 상태를 복사해서 반환
    /// </summary>
    /// <returns>현재 보드 상태</returns>
    private StoneType[,] GetBoardCopy()
    {
        StoneType[,] copy = new StoneType[BoardSize, BoardSize];
        Array.Copy(_board, copy, _board.Length);
        return copy;
    }

    //// 돌을 실제로 놓는 메서드
    //private void ExecutePlacement(int row, int col)
    //{
    //    _board[row, col] = _currentTurn;

    //    // 돌을 놓고 난 뒤의 이벤트
    //    OnStonePlaced?.Invoke();

    //    // 승리조건 만족하는지 확인 후 게임 종료 여부 결정
    //    if (_rule.CheckWin(_board, row, col, _currentTurn))
    //    {
    //        _isGameOver = true;

    //        // 게임 종료 이벤트
    //        OnGameOver?.Invoke(_currentTurn);
    //    }
    //    else
    //    {
    //        // 턴 변경
    //        ChangeTurn();
    //    }
    //}

    /// <summary>
    /// PvE 모드용 로컬 착수 메서드
    /// </summary>
    /// <returns>착수할 수 있으면 true, 아니면 false</returns>
    public bool TryPlaceStoneLocal(int row, int col)
    {
        // 게임이 종료된 상태면 무시
        if (_isGameOver) return false;

        // 규칙 체크
        if (_rule.CanPlaceStone(_board, row, col, _currentTurn))
        {
            // 보드에 돌 놓기
            _board[row, col] = _currentTurn;

            // 이벤트 발생
            GameEvents.TriggerStonePlaced(col, row, _currentTurn);

            // 리플레이 - 착수 기록
            _replay.RecordPlaceStone(row, col, _currentTurn);

            Debug.Log($"<color=green>[Local] ({col}, {row})에 {_currentTurn} 착수</color>");

            // 승리 조건 체크
            if (_rule.CheckWin(_board, row, col, _currentTurn))
            {
                // 게임 종료 이벤트
                OnGameOver?.Invoke(_currentTurn);
            }
            else
            {
                // 턴 변경
                ChangeTurn();
            }

            return true;
        }

        return false;
    }

    // ===============>>서버에서 돌이 놓였다는 정보를 받았을 때 보드 업데이트================
    private void UpdateBoardFromServer(int x, int y, StoneType playerType)
    {
        if (_isGameOver) return;

        // 통신으로 받은 타입을 효빈님 타입으로 변환
        StoneType placedStone = (playerType == StoneType.Black) ? StoneType.Black : StoneType.White;

        // 이 시점에 배열을 채우고 승패를 판정
        _board[y, x] = placedStone;

        // 리플레이 - 착수 기록
        _replay.RecordPlaceStone(y, x, _currentTurn);

        if (_rule.CheckWin(_board, y, x, placedStone))
        {
            // 게임 종료 이벤트
            OnGameOver?.Invoke(placedStone);
        }
        else
        {
            //턴 변경
            ChangeTurn();
        }
    }

    // 마나 획득 메서드
    private void IncomeMana()
    {
        int waitingPlayerIndex = GetPlayerIndex(false);

        //==========================================
        //======================================추가된 부분 (서버 시간 동기화)
        if (PhotonNetwork.InRoom)
        {
            // 이번 프레임 동안 흐른 시간
            float elapsed = (float)(PhotonNetwork.Time - _lastNetworkTime);

            // 음수 방어
            if (elapsed < 0f)
                elapsed = 0f;

            _manaIncomeTimer[waitingPlayerIndex] += elapsed;

            _lastNetworkTime = PhotonNetwork.Time;
        }
        // 오프라인
        else
        {
            _manaIncomeTimer[waitingPlayerIndex] += Time.deltaTime;
        }
        //==========================================

        bool manaChanged = false;

        while (_manaIncomeTimer[waitingPlayerIndex] >= _manaIncomeTime)
        {
            // 초과된 시간은 다음 타이머로 이월
            _manaIncomeTimer[waitingPlayerIndex] -= _manaIncomeTime;

            _players[waitingPlayerIndex].AddMana(_manaIncome);

            manaChanged = true;
        }

        if (manaChanged)
            OnManaChanged?.Invoke();
    }

    // 턴 타이머 체크
    private void CheckTurnTimer()
    {
        //==========================================
        //======================================추가된 부분 (서버 시간 동기화)
        if (PhotonNetwork.InRoom)
        {
            _turnTimer = (float)(PhotonNetwork.Time - _turnStartNetworkTime);
        }
        else
        {
            _turnTimer += Time.deltaTime;
        }
        //==========================================

        //==========================================
        //======================================추가된 부분 (TurnTimeLimit -> CurrentTurnTimeLimit)
        // 턴 제한 시간이 지났을 때 (과부하 상태면 15초, 아니면 30초와 비교)
        if (_turnTimer >= CurrentTurnTimeLimit)
        //==========================================
        {
            StoneType winner = (_currentTurn == StoneType.Black) ? StoneType.White : StoneType.Black;
            OnGameOver?.Invoke(winner);
        }
    }

    // 턴 변경 메서드
    private void ChangeTurn()
    {
        //==========================================
        //======================================추가된 부분 (턴 끝나기 직전에 과부하 턴 감소)
        int pIndex = GetPlayerIndex(true);
        if (_overloadTurnsLeft[pIndex] > 0)
        {
            _overloadTurnsLeft[pIndex]--;
            if (_overloadTurnsLeft[pIndex] == 0)
            {
                Debug.Log($"<color=cyan>[System] {((pIndex == 0) ? PlayerType.Black : PlayerType.White)} 진영의 시간 과부하가 해제되었습니다</color>");
            }
        }
        //==========================================

        _currentTurn = _currentTurn == StoneType.Black ? StoneType.White : StoneType.Black;

        _turnTimer = 0f;

        //==========================================
        //======================================추가된 부분
        // 턴이 바뀔 때마다 포톤 서버 시간 갱신
        if (PhotonNetwork.InRoom)
        {
            // 이미 턴이 바뀐 상태이니 현재 턴이 아닌 플레이어의 마나 획득 타이머도 갱신.
            int waitingPlayerIndex = GetPlayerIndex(false);
            _turnStartNetworkTime = PhotonNetwork.Time;
            _lastNetworkTime = PhotonNetwork.Time;
        }
        //==========================================

        // 리플레이 - 현재 턴 종료 및 시작
        _replay.EndTurn();
        _replay.StartTurn(_currentTurn == StoneType.Black ? PlayerType.Black : PlayerType.White);

        // PvE 모드전용
        if (_gameMode == GameMode.PvE)
        {
            // AI 턴
            if (_currentTurn == StoneType.White)
            {
                // 플레이어 착수 비활성화
                _boardInteraction.SetMyTurn(false);
                _aiPlayer.StartAITurn();
            }
            // 플레이어 턴
            else
            {
                // 플레이어 착수 활성화
                _boardInteraction.SetMyTurn(true);
            }
        }
    }

    // 플레이어 인덱스 반환
    // 인자 true: 현재 턴의 플레이어 인덱스 반환, false: 상대 플레이어의 인덱스 반환
    private int GetPlayerIndex(bool isCurrent)
    {
        if (isCurrent)
            return _currentTurn == StoneType.Black ? (int)PlayerType.Black : (int)PlayerType.White;

        return _currentTurn == StoneType.Black ? (int)PlayerType.White : (int)PlayerType.Black;
    }

    /// <summary>
    /// 게임 종료 이벤트
    /// </summary>
    /// <param name="winner">게임 승리한 플레이어의 돌</param>
    private void EndGame(StoneType winner)
    {
        _isGameOver = true;
        FindFirstObjectByType<BoardInteraction>().SetGameOver();
        string winnerName = (winner == StoneType.Black) ? _players[0].Name : _players[1].Name;
        Debug.Log($"<color=yellow><b>[SERVER INFO] {winnerName} 승리 모든 착수가 금지됩니다.</b></color>");

        // PvP 모드 일때만
        if (_gameMode == GameMode.PvP)
            RankingManager.Instance.AddScoreAndSync(winnerName == PlayFabManager.Instance.UserNickName); //민정추가

        // 리플레이 - 현재 턴 종료 및 기록 종료
        _replay.EndTurn();
        _replay.EndRecording(winner == StoneType.Black ? PlayerType.Black : PlayerType.White);

        // PlayFab에 리플레이 저장
        _replay.SaveReplayToPlayFab();

        // 플레이어 이벤트 구독 해제
        for (int i = 0; i < _players.Length; i++)
            _players[i].Cleanup();

        // AI 모드 플래그 초기화
        if (_gameMode == GameMode.PvE)
        {
            AIMatchManager.ResetAIMode();
        }

    }

    //======유니티 내장 UI 이용 테스틑 시간/마나======
    // 유니티 내장 UI 함수로 화면에 시간/마나 띄우기
    private void OnGUI()
    {
        GUIStyle style = new GUIStyle();
        style.fontSize = 35;
        style.fontStyle = FontStyle.Bold;

        // 남은 시간 통일
        float timeLeft = Mathf.Max(0, CurrentTurnTimeLimit - _turnTimer);

        if (IsMyTurn)
        {
            style.normal.textColor = timeLeft < 5f ? Color.red : Color.yellow; // 5초 남으면 빨간색!
            GUI.Label(new Rect(20, 20, 500, 50), $"[내 턴] 남은 시간: {timeLeft:F1}초", style);
        }
        else
        {
            //상대 턴일 때도 똑같이 남은 시간으로 표시
            style.normal.textColor = Color.gray;
            GUI.Label(new Rect(20, 20, 500, 50), $"[상대 턴] 남은 시간: {timeLeft:F1}초", style);
        }

        // 마나 표시
        Player myPlayer = GetPlayer(_myPlayerType);

        if (myPlayer != null)
        {
            style.normal.textColor = Color.cyan;
            GUI.Label(new Rect(20, Screen.height - 70, 500, 50), $"💎 내 마나: {myPlayer.CurrentMana}", style);
        }
    }
}