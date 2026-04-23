using System; 
using UnityEngine;
using Photon.Pun;

public class NetworkOmokManager : MonoBehaviourPunCallbacks
{
    [SerializeField] private BoardInteraction _boardInteraction;
    [SerializeField] private Sprite[] _stoneSkins;

    // 전달 데이터: x좌표, y좌표, 진영번호(1=흑, 2=백)
    public static event Action<int, int, int> OnStonePlaced;

    private int _mySkinIndex = 0;   // 스킨 번호
    private int _myPlayerType = 1;  // 유저 1(흑) 2(백) 진영 구분용 번호

    private bool _isMasterTurn = true;

    public override void OnEnable()
    {
        base.OnEnable();
        if (_boardInteraction != null)
            _boardInteraction.OnStoneClicked += SendStoneToServer;
    }

    public override void OnDisable()
    {
        base.OnDisable();
        if (_boardInteraction != null)
            _boardInteraction.OnStoneClicked -= SendStoneToServer;
    }

    private void Start()
    {
        // 로비에서 고른 스킨 번호 (이건 나중에 색상을 반전시키든 맘대로 꾸미는 용도)
        _mySkinIndex = PlayerPrefs.GetInt("MySkinID", 0);

        if (PhotonNetwork.InRoom) SetupGame();
        else
        {
            if (_boardInteraction != null) _boardInteraction.SetMyTurn(false);
        }
    }

    public override void OnJoinedRoom()
    {
        SetupGame();
    }

    private void SetupGame()
    {
        if (_boardInteraction == null || _stoneSkins.Length == 0) return;

        // 스킨이 같더라도, 진영(정체)은 다르게
        if (PhotonNetwork.IsMasterClient)
        {
            _myPlayerType = 1; // 방장은 무조건 1번(흑) 진영
            _mySkinIndex = 0;// 임시스킨
        }
        else
        {
            _myPlayerType = 2; // 손님은 무조건 2번(백) 진영
            _mySkinIndex = 1;//임시스킨
        }
        _boardInteraction.ChangeStoneSkin(_stoneSkins[_mySkinIndex]);
        CheckAndApplyTurn();
    }

    private void CheckAndApplyTurn()
    {
        // 턴 확인은 스킨 번호가 아니라 진영 번호(PlayerType)로 계산
        bool isMyTurnNow = (_isMasterTurn && _myPlayerType == 1) || (!_isMasterTurn && _myPlayerType == 2);

        _boardInteraction.SetMyTurn(isMyTurnNow);
    }

    private void SendStoneToServer(int x, int y)
    {
        if (!PhotonNetwork.InRoom) return;

        // 통신 발송: 좌표(x, y), 진영(흑/백), 그리고 내가 고른 스킨 번호를 전송
        photonView.RPC("RPC_ReceiveAndDrawStone", RpcTarget.All, x, y, _myPlayerType, _mySkinIndex);
    }

    [PunRPC]
    public void RPC_ReceiveAndDrawStone(int x, int y, int playerType, int skinID)
    {
        // 1. 시각적 처리
        Sprite stoneSkin = _stoneSkins[skinID];
        _boardInteraction.PlaceStoneRemote(x, y, stoneSkin);

        // 2. 턴 변경
        _isMasterTurn = !_isMasterTurn;
        CheckAndApplyTurn();

        string playerName = (playerType == 1) ? "플레이어1(흑돌)" : "플레이어2(백돌)";
        Debug.Log($"[{playerName}]님이 ({x}, {y}) 좌표에 착수했습니다");

        // 3. 방송 송출
        // x, y 좌표에 playerType(1번 흑 or 2번 백) 유저가 돌을 둠
        OnStonePlaced?.Invoke(x, y, playerType);
    }
}