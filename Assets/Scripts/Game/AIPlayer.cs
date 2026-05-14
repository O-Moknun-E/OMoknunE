using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// 오목 AI 플레이어 클래스
/// </summary>
public class AIPlayer : MonoBehaviour
{
    [SerializeField] private float _thinkingTime = 2f;  // AI가 생각하는 데 걸리는 시간(초)
    [SerializeField] private AIDifficulty _difficulty;  // AI 난이도 설정

    private const int WinScore = 100000;    // 승리하는 수에 대한 점수
    private const int OpenFour = 10000;     // 열린 4에 대한 점수
    private const int ClosedFour = 1000;    // 닫힌 4에 대한 점수
    private const int OpenThree = 500;      // 열린 3에 대한 점수(3이 2개일 경우 닫힌 4와 같은 취급을 위해 500으로 설정)
    private const int ClosedThree = 100;    // 닫힌 3에 대한 점수
    private const int OpenTwo = 50;         // 열린 2에 대한 점수

    private OmokManager _omokManager;           // 오목매니저 참조
    private BoardInteraction _boardInteraction; // 보드 인터랙션 참조
    private Sprite _aiStoneSprite;              // AI의 돌 스프라이트
    private StoneType _aiStone;                 // AI의 돌 색상
    private StoneType _playerStone;             // 플레이어의 돌 색상
    private bool _isThinking;                   // AI가 생각 중인지 여부

    private StoneType[,] _boardCopy;            // 스레드 안전을 위한 보드 상태 복사본
    private readonly object _boardLock = new(); // 보드 상태 접근 동기화용 락 객체

    /// <summary>
    /// 초기화
    /// </summary>
    public void Initialize(OmokManager manager, AIDifficulty difficulty, StoneType aiColor)
    {
        _omokManager = manager;
        _boardInteraction = FindFirstObjectByType<BoardInteraction>();

        if (!_boardInteraction)
        {
            Debug.LogError("BoardInteraction 컴포넌트를 찾을 수 없습니다. AIPlayer 초기화 실패.");
            return;
        }
        _aiStoneSprite = SkinRegistry.Instance.GetStoneSkin(1);



        if (_aiStoneSprite == null)
        {
            Debug.LogError("돌 스프라이트를 찾을 수 없습니다. AIPlayer 초기화 실패.");
            return;
        }

        // 기본적인 초기화 작업
        _difficulty = difficulty;
        _aiStone = aiColor;
        _playerStone = aiColor == StoneType.Black ? StoneType.White : StoneType.Black;
        _isThinking = false;

        _boardCopy = new StoneType[OmokManager.BoardSize, OmokManager.BoardSize];
    }

    /// <summary>
    /// AI 턴 시작
    /// </summary>
    public void StartAITurn()
    {
        // 생각 중이 아니라면 AI가 수를 계산하기 시작
        if (!_isThinking)
        {
            StartCoroutine(ThinkAndPlace());
        }
    }

    /// <summary>
    /// 수를 계산하고 착수 (비동기 처리)
    /// </summary>
    private IEnumerator ThinkAndPlace()
    {
        _isThinking = true;

        // AI가 생각하는 시간 대기
        yield return new WaitForSeconds(_thinkingTime);

        // 현재 보드 상태를 복사 (메인스레드)
        CopyBoardState();

        // 최적의 수
        Vector2Int bestMove = Vector2Int.zero;
        bool moveFound = false;

        // 별도 스레드에서 AI 계산 수행
        // 난이도별 알고리즘 선택
        Task<Vector2Int> aiTask = Task.Run(() =>
        {
            try
            {
                switch (_difficulty)
                {
                    case AIDifficulty.Easy:
                        return FindBestMoveEasy();
                    case AIDifficulty.Normal:
                        return FindBestMoveNormal();
                    case AIDifficulty.Hard:
                        return FindBestMoveHard();
                    default:
                        return new Vector2Int(-1, -1);
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"<color=red>[AI Thread] 오류 발생: {e.Message}</color>");
                return new Vector2Int(-1, -1);
            }
        });

        // Task 완료까지 대기 (프레임마다 체크)
        while (!aiTask.IsCompleted)
        {
            yield return null;
        }

        // 결과 가져오기
        if (aiTask.Status == TaskStatus.RanToCompletion)
        {
            bestMove = aiTask.Result;
            moveFound = true;
        }
        else
        {
            Debug.LogError("<color=red>[AI] Task 실행 실패</color>");
        }

        // 착수지점 찾았으면 좌표값 확인
        if (moveFound && bestMove.x >= 0 && bestMove.y >= 0)
        {
            // 착수 가능한지 여부, 가능하면 착수함
            bool success = _omokManager.TryPlaceStoneLocal(bestMove.y, bestMove.x);

            // 착수했다면 화면에도 보여지기 위해 보드 인터랙션에 알려줌
            if (success)
            {
                _boardInteraction.PlaceStoneRemote(bestMove.x, bestMove.y, _aiStoneSprite);
            }
            else
            {
                Debug.LogWarning($"<color=red>AI가 선택한 수 ({bestMove.x}, {bestMove.y})에 착수할 수 없습니다.</color>");
            }
        }

        _isThinking = false;
    }

    #region Easy 난이도: 휴리스틱 (1수 탐색)

    /// <summary>
    /// 쉬움: 모든 빈 칸을 평가하여 가장 좋은 수 선택
    /// </summary>
    /// <returns>선택한 수의 좌표값 반환</returns>
    private Vector2Int FindBestMoveEasy()
    {
        // 최선의 수를 찾기 위해 최소값으로 초기화
        int bestScore = int.MinValue;
        Vector2Int bestMove = new Vector2Int(-1, -1);

        for (int y = 0; y < OmokManager.BoardSize; y++)
        {
            for (int x = 0; x < OmokManager.BoardSize; x++)
            {
                // 빈칸인지 확인
                if (GetBoardData(x, y) == StoneType.Empty)
                {
                    // 해당 위치에 착수했을 때의 점수
                    int score = EvaluatePosition(x, y);

                    // 이미 찾은 수보다 더 좋다면 갱신
                    if (score > bestScore)
                    {
                        bestScore = score;
                        bestMove = new Vector2Int(x, y);
                    }
                }
            }
        }

        return bestMove;
    }

    #endregion

    #region Normal 난이도: 미니맥스(알파베타 가지치기) (3수 탐색)

    /// <summary>
    /// 보통: 탐색 깊이 3인 미니맥스(알파베타 가지치기) 알고리즘
    /// </summary>
    /// <returns>선택한 수의 좌표값 반환</returns>
    private Vector2Int FindBestMoveNormal()
    {
        int depth = 3; // 탐색 깊이
        int bestScore = int.MinValue;
        Vector2Int bestMove = new Vector2Int(-1, -1);

        // 후보군 생성 (빈 칸 중에서 주변에 돌이 있는 위치만 고려)
        var candidates = GetCandidateMoves();

        foreach (var move in candidates)
        {
            // 해당 위치에 착수했다고 가정하고 보드 데이터 업데이트
            SetBoardData(move.x, move.y, _aiStone);

            // 미니맥스 알고리즘으로 해당 위치에 착수했을 때의 점수 계산
            int score = Minimax(depth - 1, false, int.MinValue, int.MaxValue);

            // 보드 데이터 원복
            SetBoardData(move.x, move.y, StoneType.Empty);

            // 이미 찾은 수보다 더 좋다면 갱신
            if (score > bestScore)
            {
                bestScore = score;
                bestMove = move;
            }
        }

        return bestMove;
    }

    #endregion

    #region Hard 난이도: 미니맥스(알파베타 가지치기) (5수 탐색)

    /// <summary>
    /// 어려움: 탐색 깊이 4인 미니맥스(알파베타 가지치기) 알고리즘
    /// </summary>
    /// <returns></returns>
    private Vector2Int FindBestMoveHard()
    {
        int depth = 4; // 탐색 깊이
        int bestScore = int.MinValue;
        Vector2Int bestMove = new Vector2Int(-1, -1);

        // 후보군 생성 (빈 칸 중에서 주변에 돌이 있는 위치만 고려)
        var candidates = GetCandidateMoves();

        foreach (var move in candidates)
        {
            // 해당 위치에 착수했다고 가정하고 보드 데이터 업데이트
            SetBoardData(move.x, move.y, _aiStone);

            // 미니맥스 알고리즘으로 해당 위치에 착수했을 때의 점수 계산
            int score = Minimax(depth - 1, false, int.MinValue, int.MaxValue);

            // 보드 데이터 원복
            SetBoardData(move.x, move.y, StoneType.Empty);

            // 이미 찾은 수보다 더 좋다면 갱신
            if (score > bestScore)
            {
                bestScore = score;
                bestMove = move;
            }
        }

        return bestMove;
    }

    #endregion

    #region 미니맥스

    /// <summary>
    /// 미니맥스 알고리즘
    /// </summary>
    /// <param name="depth">탐색 깊이</param>
    /// <param name="isMaximizing">현재 노드가 최대화 노드인지 여부</param>
    /// <param name="alpha">알파 값</param>
    /// <param name="beta">베타 값</param>
    /// <returns>평가 점수</returns>
    private int Minimax(int depth, bool isMaximizing, int alpha, int beta)
    {
        // 깊이가 0이면 현재 보드 상태 평가하여 점수 반환
        if (depth == 0) return EvaluateBoard();

        // 후보군 생성
        var candidates = GetCandidateMoves();
        if (candidates.Count == 0) return EvaluateBoard();

        // 최대화 노드
        if (isMaximizing)
        {
            int maxScore = int.MinValue;

            foreach (var move in candidates)
            {
                // 해당 위치에 착수했다고 가정하고 보드 데이터 업데이트
                SetBoardData(move.x, move.y, _aiStone);

                // 해당 위치에 착수했을 때 승리하는 수인지 확인
                if (CheckWinAtPosition(move.x, move.y, _aiStone))
                {
                    // 승리하는 수라면 보드 원복하고 최대 점수 반환
                    SetBoardData(move.x, move.y, StoneType.Empty);
                    return WinScore;
                }

                // 승리하는 수가 아니라면 다음 깊이로 탐색 후 완료되면 오목판 원복
                int score = Minimax(depth - 1, false, alpha, beta);
                SetBoardData(move.x, move.y, StoneType.Empty);

                // 최대 점수 갱신 및 알파 값 업데이트
                maxScore = Mathf.Max(maxScore, score);
                alpha = Mathf.Max(alpha, score);

                // 알파베타 가지치기: 베타 값보다 알파 값이 크거나 같으면 더 이상 탐색할 필요 없음
                if (beta <= alpha) break;
            }

            return maxScore;
        }
        else
        {
            int minScore = int.MaxValue;

            foreach (var move in candidates)
            {
                // 해당 위치에 착수했다고 가정하고 보드 데이터 업데이트
                SetBoardData(move.x, move.y, _playerStone);

                // 해당 위치에 착수했을 때 승리하는 수인지 확인
                if (CheckWinAtPosition(move.x, move.y, _playerStone))
                {
                    // 승리하는 수라면 보드 원복하고 최소 점수 반환
                    SetBoardData(move.x, move.y, StoneType.Empty);
                    return -WinScore;
                }

                // 승리하는 수가 아니라면 다음 깊이로 탐색 후 완료되면 오목판 원복
                int score = Minimax(depth - 1, true, alpha, beta);
                SetBoardData(move.x, move.y, StoneType.Empty);

                // 최소 점수 갱신 및 베타 값 업데이트
                minScore = Mathf.Min(minScore, score);
                beta = Mathf.Min(beta, score);

                // 알파베타 가지치기: 알파 값보다 베타 값이 작거나 같으면 더 이상 탐색할 필요 없음
                if (beta <= alpha) break;
            }

            return minScore;
        }
    }

    #endregion

    #region 후보군 생성

    /// <summary>
    /// 후보군 생성 메서드: 빈 칸 중에서 주변에 돌이 있는 위치만 후보로 고려하여 탐색 공간 줄이기
    /// </summary>
    /// <returns>후보군 반환</returns>
    private List<Vector2Int> GetCandidateMoves()
    {
        var candidates = new List<Vector2Int>();
        bool[,] visited = new bool[OmokManager.BoardSize, OmokManager.BoardSize];

        // 만약 보드가 비어있으면 중앙에 착수
        if (IsEmptyBoard())
        {
            candidates.Add(new Vector2Int(7, 7));
            return candidates;
        }

        for (int y = 0; y < OmokManager.BoardSize; y++)
        {
            for (int x = 0; x < OmokManager.BoardSize; x++)
            {
                // 빈칸이 아니라면
                if (GetBoardData(x, y) != StoneType.Empty)
                {
                    // 해당 위치에서의 주변 5x5 영역 탐색
                    for (int dy = -2; dy <= 2; dy++)
                    {
                        for (int dx = -2; dx <= 2; dx++)
                        {
                            int nx = x + dx;
                            int ny = y + dy;

                            // 범위 내에 있고, 아직 방문하지 않았으며, 빈 칸이라면 후보군에 추가
                            if (IsInBounds(nx, ny) && !visited[ny, nx] && GetBoardData(nx, ny) == StoneType.Empty)
                            {
                                candidates.Add(new Vector2Int(nx, ny));
                                visited[ny, nx] = true;
                            }
                        }
                    }
                }
            }
        }

        // 후보군이 너무 많으면 평가 점수 기준으로 상위 20개만 남기기
        if (candidates.Count > 20)
        {
            candidates.Sort((a, b) => EvaluatePosition(b.x, b.y).CompareTo(EvaluatePosition(a.x, a.y)));
            candidates = candidates.GetRange(0, 20);
        }

        return candidates;
    }

    #endregion

    #region 평가 메서드

    /// <summary>
    /// 해당 위치 평가 메서드
    /// </summary>
    /// <returns>평가한 점수 반환</returns>
    private int EvaluatePosition(int x, int y)
    {
        int score = 0;

        // 해당 위치에 착수 했을 때의 공격 점수 계산 (AI 자신, 2배 가산)
        score += EvaluateDirection(x, y, _aiStone) * 2;

        // 해당 위치에 착수 했을 때의 방어 점수 계산
        score += EvaluateDirection(x, y, _playerStone);

        // 중앙에 가까울수록 가산(확장 방향이 많으니)
        int centerBonus = 7 - Mathf.Max(Mathf.Abs(7 - x), Mathf.Abs(7 - y));
        score += centerBonus;

        return score;
    }

    /// <summary>
    /// 8방향으로 연속된 돌의 개수와 열린 여부를 평가하는 메서드
    /// </summary>
    /// <returns>평가한 점수 반환</returns>
    private int EvaluateDirection(int x, int y, StoneType stoneType)
    {
        // 우, 상, 우상, 우하 방향벡터
        int[] dx = { 1, 0, 1, 1 };
        int[] dy = { 0, 1, 1, -1 };
        int totalScore = 0;

        for (int dir = 0; dir < 4; dir++)
        {
            int count = 1;
            int openCount = 0;

            // 정방향 탐색 (6목 이상일 경우를 위해 보드 끝까지 탐색)
            for (int i = 1; i < OmokManager.BoardSize; i++)
            {
                int nx = x + dx[dir] * i;
                int ny = y + dy[dir] * i;

                // 범위 내에 없다면 해당 방향으로 확장이 불가하니 빠져나오기
                if (!IsInBounds(nx, ny)) break;

                // 넘겨받은 돌 타입으로 연속된 돌이 있으면 카운트 증가
                if (GetBoardData(nx, ny) == stoneType)
                {
                    count++;
                }
                // 열려 있으면 열린 카운트 증가
                else if (GetBoardData(nx, ny) == StoneType.Empty)
                {
                    openCount++;
                    break;
                }
                // 닫힘
                else
                {
                    break;
                }
            }

            // 역방향 탐색(좌, 하, 좌하, 좌상) (6목 이상일 경우를 위해 보드 끝까지 탐색)
            for (int i = 1; i < OmokManager.BoardSize; i++)
            {
                int nx = x - dx[dir] * i;
                int ny = y - dy[dir] * i;

                // 범위 내에 없다면 해당 방향으로 확장 불가
                if (!IsInBounds(nx, ny)) break;

                // 넘겨받은 돌 타입으로 연속된 돌이 있으면 카운트 증가
                if (GetBoardData(nx, ny) == stoneType)
                {
                    count++;
                }
                // 열려 있으면 열린 카운트 증가
                else if (GetBoardData(nx, ny) == StoneType.Empty)
                {
                    openCount++;
                    break;
                }
                // 닫힘
                else
                {
                    break;
                }
            }

            // 현재 방향에 놓인 돌에 대한 점수 계산
            if (count == 5)
                totalScore += WinScore;
            // 6목 이상은 닫힌 4와 같은 취급
            else if (count >= 6)
                totalScore += ClosedFour;
            else if (count == 4 && openCount == 2)
                totalScore += OpenFour;
            else if (count == 4 && openCount == 1)
                totalScore += ClosedFour;
            else if (count == 3 && openCount == 2)
                totalScore += OpenThree;
            else if (count == 3 && openCount == 1)
                totalScore += ClosedThree;
            else if (count == 2 && openCount == 2)
                totalScore += OpenTwo;

        }

        return totalScore;
    }

    /// <summary>
    /// 오목판 전체 평가 메서드
    /// </summary>
    /// <returns></returns>
    private int EvaluateBoard()
    {
        int score = 0;

        for (int y = 0; y < OmokManager.BoardSize; y++)
        {
            for (int x = 0; x < OmokManager.BoardSize; x++)
            {
                // AI 돌이면 가산
                if (GetBoardData(x, y) == _aiStone)
                    score += EvaluateDirection(x, y, _aiStone);
                // 플레이어 돌이면 감산
                else if (GetBoardData(x, y) == _playerStone)
                    score -= EvaluateDirection(x, y, _playerStone);
            }
        }

        return score;
    }

    #endregion

    #region 유틸리티 메서드

    /// <summary>
    /// 오목판의 경계에 해당 좌표가 있는지 확인하는 메서드
    /// </summary>
    /// <returns>오목판 범위에 있으면 true, 아니면 false</returns>
    private bool IsInBounds(int x, int y) => x >= 0 && x < OmokManager.BoardSize && y >= 0 && y < OmokManager.BoardSize;

    /// <summary>
    /// 오목판 전체가 빈 칸인지 확인하는 메서드
    /// </summary>
    /// <returns>전체가 빈 칸이면 true, 아니면 false</returns>
    private bool IsEmptyBoard()
    {
        for (int y = 0; y < OmokManager.BoardSize; y++)
        {
            for (int x = 0; x < OmokManager.BoardSize; x++)
            {
                if (GetBoardData(x, y) != StoneType.Empty)
                    return false;
            }
        }

        return true;
    }

    /// <summary>
    /// 해당 좌표에 착수했을 때 승리하는 수인지 확인하는 메서드
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="stoneType"></param>
    /// <returns></returns>
    private bool CheckWinAtPosition(int x, int y, StoneType stoneType)
    {
        // 우, 상, 우상, 우하 방향벡터
        int[] dx = { 1, 0, 1, 1, };
        int[] dy = { 0, 1, 1, -1 };

        // 8방향으로 연속된 돌의 개수 세기
        for (int dir = 0; dir < 4; dir++)
        {
            int count = 1;

            // 정방향 탐색 (6목 이상일 경우를 위해 보드 끝까지 탐색)
            for (int i = 1; i < OmokManager.BoardSize; i++)
            {
                int nx = x + dx[dir] * i;
                int ny = y + dy[dir] * i;

                // 범위 내에 있고, 넘겨받은 돌 타입으로 연속된 돌이 있으면 카운트 증가
                if (IsInBounds(nx, ny) && GetBoardData(nx, ny) == stoneType)
                    count++;
                // 아니면 빠져나오기
                else
                    break;
            }

            // 역방향 탐색 (6목 이상일 경우를 위해 보드 끝까지 탐색)
            for (int i = 1; i < OmokManager.BoardSize; i++)
            {
                int nx = x - dx[dir] * i;
                int ny = y - dy[dir] * i;
                // 범위 내에 있고, 넘겨받은 돌 타입으로 연속된 돌이 있으면 카운트 증가
                if (IsInBounds(nx, ny) && GetBoardData(nx, ny) == stoneType)
                    count++;
                // 아니면 빠져나오기
                else
                    break;
            }

            // 연속으로 놓인 돌이 5개면 승리하는 수
            if (count == 5) return true;
        }

        return false;
    }

    /// <summary>
    /// 오목판 상태를 스레드 안전하게 복사하는 메서드
    /// </summary>
    private void CopyBoardState()
    {
        lock (_boardLock)
        {
            for (int y = 0; y < OmokManager.BoardSize; y++)
            {
                for (int x = 0; x < OmokManager.BoardSize; x++)
                {
                    _boardCopy[y, x] = _omokManager.GetBoardData(x, y);
                }
            }
        }
    }

    /// <summary>
    /// 스레드 안전하게 보드 상태를 가져오는 메서드
    /// </summary>
    private StoneType GetBoardData(int x, int y)
    {
        lock (_boardLock)
        {
            return _boardCopy[y, x];
        }
    }

    /// <summary>
    /// 스레드 안전하게 보드 상태 설정
    /// </summary>
    private void SetBoardData(int x, int y, StoneType stoneType)
    {
        lock (_boardLock)
        {
            _boardCopy[y, x] = stoneType;
        }
    }

    #endregion
}
