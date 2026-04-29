using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using Unity.VisualScripting;
using JetBrains.Annotations;

public class ReplayManager : MonoBehaviour
{
    [Header("리플레이 시스템")]
    [SerializeField] private Replay _replay;            // 리플레이 시스템 컴포넌트

    [Header("보드 설정")] // BoardInteraction의 수치와 같게 맞추기
    [SerializeField] private Transform _boardTransform;          // 보드 오브젝트의 Transform
    [SerializeField] private int _gridSize = 15;                 // 보드 크기 (15x15)
    [SerializeField] private float _spacing = 1.0f;              // 격자 간격
    [SerializeField] private Vector2 _gridOffset = Vector2.zero; // 격자 오프셋

    [Header("돌 프리팹 및 스프라이트(돌 스킨)")]
    [SerializeField] private GameObject _stonePrefab;   // 돌 프리팹(BoardInteraction의 프리팹과 동일하게 맞추기)
    [SerializeField] private Sprite[] _stoneSkins;      // 돌 스킨 배열(NetworkOmokManager의 스킨과 동일하게 맞추기)

    [Header("UI 연결")]
    public TextMeshProUGUI turnLabel;
    public Slider turnSlider;
    public Button btnPrev, btnPlayPause, btnNext;
    public TMP_InputField jumpInput;
    public Button btnJump;
    public TextMeshProUGUI playPauseLabel; // BtnPlayPause 안의 텍스트

    [Header("재생 설정")]
    [SerializeField] private bool _autoPlayOnStart = false;  // 씬 시작 시 자동 재생 여부

    // 턴 히스토리 저장(턴 되돌리기용)
    private class TurnHistory
    {
        public List<GameObject> addedStones = new List<GameObject>();          // 현재 턴에 추가된 돌들
        public List<(int row, int col, StoneType beforeType)> changes = new(); // 보드 변경전 상태
    }

    private StoneType[,] _board;            // 오목판
    private Replay.ReplayData _replayData;  // 현재 리플레이 데이터
    private Stack<TurnHistory> _history;    // 턴 히스토리 스택

    private bool _isAutoPlaying = false;
    private float _timer = 0f;

    void Start()
    {
        // 리플레이 데이터 가져오기
        _replayData = ReplayList.SelectedReplayData;

        // 리플레이 데이터가 없으면 무시
        if (_replayData == null)
        {
            Debug.LogError("리플레이 데이터가 없습니다.");
            return;
        }

        // 초기화 작업
        _board = new StoneType[_gridSize, _gridSize];
        _history = new Stack<TurnHistory>();

        // Replay 시스템 시작
        _replay.StartReplay(_replayData);

        turnSlider.minValue = 0;
        turnSlider.maxValue = _replay.TotalTurns;
        turnSlider.wholeNumbers = true;

        btnPrev.onClick.AddListener(OnPreviousTurn);
        btnNext.onClick.AddListener(OnNextTurn);
        btnPlayPause.onClick.AddListener(TogglePlay);
        btnJump.onClick.AddListener(JumpToInput);
        turnSlider.onValueChanged.AddListener(OnSliderChanged);

        UpdateUI();

        // 자동 재생 시작
        if (_autoPlayOnStart)
        {
            _isAutoPlaying = true;
            playPauseLabel.text = "일시정지";
        }
    }

    void Update()
    {
        // 자동 재생 중이 아니면 무시
        if (!_isAutoPlaying) return;

        _timer += Time.deltaTime;

        // 지정한 시간이 지나면 다음 턴으로 넘어가기
        if (_timer >= _replay.ReplayInterval)
        {
            _timer -= _replay.ReplayInterval;

            if (_replay.CurrentTurnIndex >= _replay.TotalTurns)
            {
                // 끝이라면 재생 종료
                _isAutoPlaying = false;
                playPauseLabel.text = "재생";

                Debug.Log("리플레이 재생 완료!");
                return;
            }

            OnNextTurn();
        }
    }

    void TogglePlay()
    {
        _isAutoPlaying = !_isAutoPlaying;
        playPauseLabel.text = _isAutoPlaying ? "일시정지" : "재생";
        _timer = 0f;
    }

    /// <summary>
    /// 이전 턴으로 이동
    /// </summary>
    private void OnPreviousTurn()
    {
        // 첫 턴이면 무시
        if (_replay.CurrentTurnIndex <= 0 || _history.Count == 0) return;

        // 수동으로 턴을 변경하면 자동 재생이 멈추도록 설정
        _isAutoPlaying = false;
        playPauseLabel.text = "재생";

        // 히스토리에서 마지막 턴 가져오기
        TurnHistory lastTurn = _history.Pop();

        // 추가된 돌들 제거
        foreach (var stone in lastTurn.addedStones)
        {
            if (stone != null)
                Destroy(stone);
        }

        // 보드 데이터 원복
        foreach (var (row, col, beforeType) in lastTurn.changes)
        {
            _board[row, col] = beforeType;
        }

        // Replay 시스템에서 이전 턴으로 이동 후 UI 새로 고침
        _replay.JumpToTurn(_replay.CurrentTurnIndex - 1);
        UpdateUI();
    }

    /// <summary>
    /// 다음 턴으로 이동
    /// </summary>
    private void OnNextTurn()
    {
        // 마지막 턴이라면 무시
        if (_replay.CurrentTurnIndex >= _replay.TotalTurns) return;

        // 다음 턴 데이터 가져오기
        var turnData = _replay.PlayNextTurn();

        // 현재 턴의 히스토리 생성
        TurnHistory history = new TurnHistory();

        foreach (var action in turnData.Value.actions)
        {
            ExecuteAction(action, history);
        }

        // 히스토리 스택에 넣고 UI 업데이트
        _history.Push(history);
        UpdateUI();
    }

    /// <summary>
    /// 슬라이더로 특정 턴 이동
    /// </summary>
    private void OnSliderChanged(float value)
    {
        int targetTurn = (int)value;

        // 현재 턴이랑 같으면 무시
        if (targetTurn == _replay.CurrentTurnIndex) return;

        // 자동 재생 중이라면 끄기
        _isAutoPlaying = false;
        playPauseLabel.text = "재생";

        // 처음부터 특정턴까지 다시 구성
        RebuildToTurn(targetTurn);
    }

    /// <summary>
    /// 입력 값으로 특정 턴 이동
    /// </summary>
    private void JumpToInput()
    {
        if (int.TryParse(jumpInput.text, out int targetTurn))
        {
            targetTurn = Mathf.Clamp(targetTurn, 0, _replay.TotalTurns);

            // 현재 턴이랑 같으면 무시
            if (targetTurn == _replay.CurrentTurnIndex) return;

            // 자동 재생 중이라면 끄기
            _isAutoPlaying = false;
            playPauseLabel.text = "재생";

            // 처음부터 특정턴까지 다시 구성
            RebuildToTurn(targetTurn);
        }
    }


    /// <summary>
    /// 특정 턴까지 처음부터 다시 구성
    /// </summary>
    private void RebuildToTurn(int targetTurn)
    {
        // 보드 초기화
        ResetBoard();
        _history.Clear();

        // Replay 시스템 초기화
        _replay.JumpToTurn(0);

        // 특정 턴까지 실행
        for (int i = 0; i < targetTurn; i++)
        {
            var turnData = _replay.PlayNextTurn();

            // 턴 정보가 없으면 빠져나오기
            if (turnData == null) break;

            TurnHistory history = new TurnHistory();

            // 해당 턴의 모든 행동 실행
            foreach (var action in turnData.Value.actions)
            {
                ExecuteAction(action, history);
            }

            // 히스토리에 추가
            _history.Push(history);
        }

        // UI 업데이트
        UpdateUI();
    }

    /// <summary>
    /// 행동 실행한 후 히스토리에 기록
    /// </summary>
    /// <param name="action">행동 데이터</param>
    /// <param name="history">히스토리 데이터</param>
    private void ExecuteAction(Replay.ActionData action, TurnHistory history)
    {
        switch (action.actionType)
        {
            case ActionType.UseMagic:
                // 마법 사용으로 인한 보드 변화 적용
                foreach (var change in action.boardChanges)
                {
                    // 보드 변화 후 상태가 빈 공간이 아니라면 돌 놓기
                    if (change.stateAfter != StoneType.Empty)
                    {
                        PlaceStone(change.row, change.col, change.stateAfter, history);
                    }
                    // 비어있으면 돌 제거
                    else
                    {
                        RemoveStone(change.row, change.col, history);
                    }
                }
                break;

            case ActionType.PlaceStone:
                // 착수 처리
                if (action.boardChanges.Count > 0)
                {
                    var change = action.boardChanges[0];
                    PlaceStone(change.row, change.col, change.stateAfter, history);
                }
                break;
        }
    }

    /// <summary>
    /// 히스토리에 기록 후 돌 놓기
    /// </summary>
    private void PlaceStone(int row, int col, StoneType stoneType, TurnHistory history)
    {
        // Empty라면 무시
        if (stoneType == StoneType.Empty) return;

        // 돌 놓기전 변경전 보드 상태 기록
        history.changes.Add((row, col, _board[row, col]));

        // 보드 업데이트
        _board[row, col] = stoneType;

        // 화면에 돌 생성
        Vector3 worldPos = GetWorldPositionFromIndex(col, row);
        GameObject stone = Instantiate(_stonePrefab, worldPos, Quaternion.identity, _boardTransform);

        // 스프라이트 설정
        SpriteRenderer renderer = stone.GetComponent<SpriteRenderer>();

        // 스프라이트가 있고 스킨이 있다면 설정
        if (renderer != null && _stoneSkins != null && _stoneSkins.Length > 0)
        {
            // 일단 흑은 0 백은 1로 하고 나중에 스킨쪽 완성되면 바꾸기
            int skinIndex = stoneType == StoneType.Black ? 0 : 1;

            if (skinIndex < _stoneSkins.Length)
            {
                renderer.sprite = _stoneSkins[skinIndex];
            }
        }

        // 화면에 그려진 돌 기록
        history.addedStones.Add(stone);
    }

    /// <summary>
    ///  히스토리에 기록 후 돌 제거
    /// </summary>
    private void RemoveStone(int row, int col, TurnHistory history)
    {
        // 돌 제거 전 변경전 보드 상태 기록
        history.changes.Add((row, col, _board[row, col]));
        _board[row, col] = StoneType.Empty;
    }

    /// <summary>
    /// 보드 초기화
    /// </summary>
    private void ResetBoard()
    {
        // 보드 데이터 초기화
        for (int row = 0; row < _gridSize; row++)
        {
            for (int col = 0; col < _gridSize; col++)
            {
                _board[row, col] = StoneType.Empty;
            }
        }

        // 모든 히스토리 내역 삭제 및 화면에서 돌 제거
        while (_history.Count > 0)
        {
            var history = _history.Pop();

            foreach (var stone in history.addedStones)
            {
                if (stone != null)
                    Destroy(stone);
            }
        }
    }

    /// <summary>
    /// 인덱스(x, y)를 월드 좌표로 변환
    /// </summary>
    /// <returns>월드 좌표</returns>
    private Vector3 GetWorldPositionFromIndex(int x, int y)
    {
        // 오목판이 없으면 무시
        if (_boardTransform == null)
        {
            Debug.LogError("Board Transform이 할당되지 않았습니다.");
            return Vector3.zero;
        }

        float halfSize = (_gridSize - 1) * _spacing / 2f;
        float localX = (x * _spacing) - halfSize + _gridOffset.x;
        float localY = (y * _spacing) - halfSize + _gridOffset.y;

        // 돌이 바둑판보다 살짝 앞에 오도록 하기
        return _boardTransform.TransformPoint(new Vector3(localX, localY, -0.1f));
    }

    /// <summary>
    /// UI 업데이트
    /// </summary>
    private void UpdateUI()
    {
        int currentTurn = _replay.CurrentTurnIndex;

        // 플레이어 이름 및 돌 정보
        string playerInfo = "";

        // 유효한 턴
        if(currentTurn > 0 && currentTurn <= _replay.TotalTurns)
        {
            // 착수한 턴 정보 가져오기
            var turn = _replayData.turns[currentTurn - 1];
            string playerName = turn.playerType == PlayerType.Black ? _replayData.blackPlayerName : _replayData.whitePlayerName;
            string playerType = turn.playerType == PlayerType.Black ? "흑" : "백";

            playerInfo = $" - {playerName} ({playerType})";
        }

        // 턴 정보 UI 갱신
        turnLabel.text = $"{currentTurn}턴 / {_replay.TotalTurns}턴 {playerInfo}";
        turnSlider.SetValueWithoutNotify(currentTurn);

        btnPrev.interactable = currentTurn > 0;
        btnNext.interactable = currentTurn < _replay.TotalTurns;
    }
}