using UnityEngine;

public class OmokManager : MonoBehaviour
{
    [SerializeField] private BoardInteraction _boardInteraction;

    [Header("유저 스킨 설정")]
    [SerializeField] private Sprite _player1Skin;
    [SerializeField] private Sprite _player2Skin;

    private bool _isPlayer1Turn = true;

    private void Start()
    {
        if (_boardInteraction != null && _player1Skin != null)
        {
            // 게임 시작 시 1P 스킨 장전
            _boardInteraction.ChangeStoneSkin(_player1Skin);

            // 마우스 입력을 활성화
            _boardInteraction.SetMyTurn(true);
        }
    }

    private void OnEnable()
    {
        if (_boardInteraction != null)
            _boardInteraction.OnStoneClicked += HandleStonePlaced;
    }

    private void OnDisable()
    {
        if (_boardInteraction != null)
            _boardInteraction.OnStoneClicked -= HandleStonePlaced;
    }

    // 바둑판을 클릭하면 실행됨
    private void HandleStonePlaced(int x, int y)
    {
        // 1. (나중에 쓰일) 서버로 전송하는 척 로그만 띄움
        Debug.Log($"[가짜 서버 발송] 좌표: ({x}, {y})");

        // --- [가짜 서버 응답 시뮬레이션] ---

        // 2. 방금 클릭한 자리에 현재 턴의 돌을 강제로 그림 (서버가 허락했다고 가정)
        Sprite currentSkin = _isPlayer1Turn ? _player1Skin : _player2Skin;
        _boardInteraction.PlaceStoneRemote(x, y, currentSkin);

        // 3. 턴 넘기기
        _isPlayer1Turn = !_isPlayer1Turn;
        Sprite nextSkin = _isPlayer1Turn ? _player1Skin : _player2Skin;

        // 4. 다음 돌 스킨 장전
        _boardInteraction.ChangeStoneSkin(nextSkin);

        // 5. 혼자 양쪽 다 두면서 테스트해야 하니, 계속 마우스 클릭을 허용함
        _boardInteraction.SetMyTurn(true);
    }
}