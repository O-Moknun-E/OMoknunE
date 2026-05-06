using System.Collections;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;

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

        _aiStoneSprite = StoneSkinRegistry.Instance.GetStoneSkin(1);

        if (_aiStoneSprite == null)
        {
            Debug.LogError("돌 스프라이트를 찾을 수 없습니다. AIPlayer 초기화 실패.");
            return;
        }

        _difficulty = difficulty;
        _aiStone = aiColor;
        _playerStone = aiColor == StoneType.Black ? StoneType.White : StoneType.Black;
        _isThinking = false;
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
    /// 수를 계산하고 착수
    /// </summary>
    private IEnumerator ThinkAndPlace()
    {
        _isThinking = true;

        // AI가 생각하는 시간 대기
        yield return new WaitForSeconds(_thinkingTime);

        // 최적의 수
        Vector2Int bestMove = Vector2Int.zero;

        // 난이도별 알고리즘 선택
        switch (_difficulty)
        {
            case AIDifficulty.Easy:
                bestMove = FindBestMoveEasy();
                break;
            case AIDifficulty.Normal:
                bestMove = FindBestMoveNormal();
                break;
            case AIDifficulty.Hard:
                bestMove = FindBestMoveHard();
                break;
        }

        // 좌표값 확인
        if (bestMove.x >= 0 && bestMove.y >= 0)
        {
            // 착수 가능한지 여부, 가능하면 착수함
            bool success = _omokManager.TryPlaceStoneLocal(bestMove.y, bestMove.x);

            // 착수했다면 화면에도 보여지기 위해 보드 인터랙션에 알려줌
            if (success)
            {
                _boardInteraction.PlaceStoneRemote(bestMove.y, bestMove.x, _aiStoneSprite);
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
            for(int x = 0; x < OmokManager.BoardSize; x++)
            {
                // 빈칸인지 확인
                if(_omokManager.GetBoardData(x, y) == StoneType.Empty)
                {
                    // 해당 위치에 착수했을 때의 점수
                    int score = EvaluatePosition(x, y);

                    // 이미 찾은 수보다 더 좋다면 갱신
                    if(score > bestScore)
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

    #region Normal 난이도: 미니맥스 (2~3수 탐색)

    /// <summary>
    /// 보통: 미니맥스 알고리즘
    /// </summary>
    /// <returns>선택한 수의 좌표값 반환</returns>
    private Vector2Int FindBestMoveNormal()
    {
        Vector2Int bestMove = new Vector2Int(-1, -1);

        return bestMove;
    }

    #endregion

    #region Hard 난이도: 알파베타 가지치기 (4~5수 탐색)

    private Vector2Int FindBestMoveHard()
    {
        Vector2Int bestMove = new Vector2Int(-1, -1);

        return bestMove;
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
    /// 특정 방향으로 연속된 돌의 개수와 열린/닫힌 여부를 평가하는 메서드
    /// </summary>
    /// <returns>평가한 점수 반환</returns>
    private int EvaluateDirection(int x, int y, StoneType stoneType)
    {
        int[] dx = { 1, 0, 1, 1 };
        int[] dy = { 0, 1, 1, -1 };
        int totalScore = 0;

        for(int dir = 0; dir < 4; dir++)
        {

        }

        return totalScore;
    }

    #endregion
}
