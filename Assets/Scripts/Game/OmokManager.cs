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

    private const int BoardSize = 15;   // 오목판의 크기 (15x15)

    private StoneType[,] _board;        // 오목판 상태를 저장하는 2D 배열
    private Player[] _players;          // 플레이어 배열 (0: 흑, 1: 백)
    private IOmokRule _rule;            // 오목 게임 규칙
    private StoneType _currentTurn;     // 현재 턴의 돌 색상
    private float _turnTimer;           // 턴 타이머
    private float[] _manaIncomeTimer;   // 마나 획득 타이머 (0: 흑, 1: 백)
    private bool _isGameOver;           // 게임 종료 여부

    //==========================================
    //======================================추가된 부분
    // 포톤 서버 시간 기준으로 턴이 시작된 시간 기록
    private double _turnStartNetworkTime;
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


    private void OnEnable()
    {
        InitGame();

        NetworkOmokManager.OnStonePlaced += UpdateBoardFromServer;
        OnGameOver += EndGame;
    }

    private void OnDisable()
    {
        NetworkOmokManager.OnStonePlaced -= UpdateBoardFromServer;
        OnGameOver -= EndGame;
    }

    private void Update()
    {
        // 게임이 종료된 상태에서는 X
        if (_isGameOver) return;

        IncomeMana();
        CheckTurnTimer();
    }

    // 게임 초기화
    public void InitGame()
    {
        _board = new StoneType[BoardSize, BoardSize];
        _players = new Player[2];

        // 기본 오목 규칙 사용
        _rule = new StandardOmokRule();

        // 플레이어 초기화
        // 나중에 닉네임 추가(임시로 "흑", "백")

        int index = 0;
        foreach (Photon.Realtime.Player photonPlayer in PhotonNetwork.PlayerList) // 민정추가 유저닉네임 저장
        {

            if (index >= _players.Length) break;

            int playerColor = (index == 0) ? _manaBlack : _manaWhite;

            _players[index] = new Player(photonPlayer.NickName, playerColor);

            index++;
        }

        /*        _players[0] = new Player("흑", _manaBlack);
                _players[1] = new Player("백", _manaWhite);*/

        // 게임 상태 초기화
        _isGameOver = false;

        // 임시로 흑이 먼저 시작
        // 네트워크 연동되면 그때 결정
        _currentTurn = StoneType.Black;

        // 타이머 초기화
        _turnTimer = 0f;
        _manaIncomeTimer = new float[2] { 0f, 0f };

        //==========================================
        //======================================추가된 부분
        // 게임 시작 시 포톤 룸 안에 있다면 현재 서버 시간을 기록
        if (PhotonNetwork.InRoom) _turnStartNetworkTime = PhotonNetwork.Time;
        //==========================================

        // 리플레이 기록 시작
        _replay.StartRecording(_players[0].Name, _players[1].Name);

        // 첫 턴은 바로 기록
        _replay.StartTurn(_currentTurn == StoneType.Black ? PlayerType.Black : PlayerType.White);
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
            if (_turnTimer - _manaIncomeTimer[waitingPlayerIndex] >= _manaIncomeTime)
            {
                _players[waitingPlayerIndex].AddMana(_manaIncome);
                OnManaChanged?.Invoke();
                _manaIncomeTimer[waitingPlayerIndex] += _manaIncomeTime;
            }
            return; // 오프라인 계산 건너뛰기
        }
        //==========================================

        _manaIncomeTimer[waitingPlayerIndex] += Time.deltaTime;

        // 마나 획득 시간이 지나면 상대방에게 마나 추가
        if (_manaIncomeTimer[waitingPlayerIndex] >= _manaIncomeTime)
        {
            _players[waitingPlayerIndex].AddMana(_manaIncome);

            // 마나 변경 이벤트
            OnManaChanged?.Invoke();

            // 시간 초과분은 다음 타이머로 이월
            _manaIncomeTimer[waitingPlayerIndex] -= _manaIncomeTime;
        }
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

        // 턴 제한 시간이 지났을 때
        if (_turnTimer >= _turnTimeLimit)
        {
            // 턴 시간 초과 시, 현재 턴이었던 사람의 반대가 승리
            StoneType winner = (_currentTurn == StoneType.Black) ? StoneType.White : StoneType.Black;
            OnGameOver?.Invoke(winner);

            // 더 이상 시간 안 흐르게 게임 오버 처리
            _isGameOver = true;
        }
    }

    // 턴 변경 메서드
    private void ChangeTurn()
    {
        _currentTurn = _currentTurn == StoneType.Black ? StoneType.White : StoneType.Black;

        _turnTimer = 0f;

        //==========================================
        //======================================추가된 부분
        // 턴이 바뀔 때마다 포톤 서버 시간 갱신 및 마나 타이머 초기화
        if (PhotonNetwork.InRoom) _turnStartNetworkTime = PhotonNetwork.Time;
        _manaIncomeTimer[0] = 0f;
        _manaIncomeTimer[1] = 0f;
        //==========================================

        // 리플레이 - 현재 턴 종료 및 시작
        _replay.EndTurn();
        _replay.StartTurn(_currentTurn == StoneType.Black ? PlayerType.Black : PlayerType.White);
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

        RankingManager.Instance.AddScoreAndSync(winnerName == PlayFabManager.Instance.UserNickName); //민정추가

        // 리플레이 - 현재 턴 종료 및 기록 종료
        _replay.EndTurn();
        _replay.EndRecording(winner == StoneType.Black ? PlayerType.Black : PlayerType.White);

        // PlayFab에 리플레이 저장
        _replay.SaveReplayToPlayFab();
    }
}