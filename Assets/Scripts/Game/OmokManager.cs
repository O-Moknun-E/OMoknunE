using System;
using UnityEngine;

// 오목 게임의 전반적인 관리를 담당하는 싱글톤 클래스
public class OmokManager : Singleton<OmokManager>
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

    private const int BoardSize = 15;   // 오목판의 크기 (15x15)

    private StoneType[,] _board;        // 오목판 상태를 저장하는 2D 배열
    private Player[] _players;          // 플레이어 배열 (0: 흑, 1: 백)
    private IOmokRule _rule;            // 오목 게임 규칙
    private StoneType _currentTurn;     // 현재 턴의 돌 색상
    private float _turnTimer;           // 턴 타이머
    private float[] _manaIncomeTimer;   // 마나 획득 타이머 (0: 흑, 1: 백)
    private bool _isGameOver;           // 게임 종료 여부

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
        NetworkOmokManager.OnStonePlaced += UpdateBoardFromServer;
    }

    private void OnDisable()
    {
        NetworkOmokManager.OnStonePlaced -= UpdateBoardFromServer;
    }

    protected override void Awake()
    {
        base.Awake();

        InitGame();
    }

    private void Update()
    {
        // 게임이 종료된 상태에서는 X
        if (_isGameOver) return;

        IncomeMana();
        CheckTurnTimer();




}

    // 게임 초기화
    private void InitGame()
    {
        _board = new StoneType[BoardSize, BoardSize];
        _players = new Player[2];

        // 기본 오목 규칙 사용
        _rule = new StandardOmokRule();

        // 플레이어 초기화
        _players[0] = new Player(_manaBlack);
        _players[1] = new Player(_manaWhite);

        // 게임 상태 초기화
        _isGameOver = false;

        // 임시로 흑이 먼저 시작
        // 네트워크 연동되면 그때 결정
        _currentTurn = StoneType.Black;

        // 타이머 초기화
        _turnTimer = 0f;
        _manaIncomeTimer = new float[2] { 0f, 0f };
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
        if(player.TryUseMagic(magic.Cost))
        {
            // 마법 실제 사용
            magic.Execute();

            // 마법 사용 이벤트
            OnUsedMagic?.Invoke();
            return true;
        }

        return false;
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
    private void UpdateBoardFromServer(int x, int y, int playerType)
    {
        if (_isGameOver) return;
        // 통신으로 받은 타입을 효빈님 타입으로 변환
        StoneType placedStone = (playerType == 1) ? StoneType.Black : StoneType.White;

        // 이 시점에 배열을 채우고 승패를 판정
        _board[y, x] = placedStone;

        if (_rule.CheckWin(_board, y, x, placedStone))
        {
            _isGameOver = true;
            FindObjectOfType<BoardInteraction>().SetGameOver();
            string winnerName = (placedStone == StoneType.Black) ? "흑(플레이어1)" : "백(플레이어2)";
            Debug.Log($"<color=yellow><b>[SERVER INFO] {winnerName} 승리 모든 착수가 금지됩니다.</b></color>");
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
        _turnTimer += Time.deltaTime;

        // 턴 제한 시간이 지났을 때
        if (_turnTimer >= _turnTimeLimit)
        {
            // 턴 넘기기
            ChangeTurn();
        }
    }

    // 턴 변경 메서드
    private void ChangeTurn()
    {
        _currentTurn = _currentTurn == StoneType.Black ? StoneType.White : StoneType.Black;

        _turnTimer = 0f;
    }

    // 플레이어 인덱스 반환
    // 인자 true: 현재 턴의 플레이어 인덱스 반환, false: 상대 플레이어의 인덱스 반환
    private int GetPlayerIndex(bool isCurrent)
    {
        if (isCurrent)
            return _currentTurn == StoneType.Black ? (int)PlayerType.Black : (int)PlayerType.White;

        return _currentTurn == StoneType.Black ? (int)PlayerType.White : (int)PlayerType.Black;
    }
}
