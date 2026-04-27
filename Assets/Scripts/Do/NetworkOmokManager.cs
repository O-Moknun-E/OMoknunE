using System; 
using UnityEngine;
using Photon.Pun;

public class NetworkOmokManager : MonoBehaviourPunCallbacks
{
    [SerializeField] private BoardInteraction _boardInteraction;
    [SerializeField] private Sprite[] _stoneSkins;

    // 전달 데이터: x좌표, y좌표, 진영번호(1=흑, 2=백)
    public static event Action<int, int, StoneType> OnStonePlaced;

    private int _mySkinIndex = 0;   // 스킨 번호
    private StoneType _myPlayerType = StoneType.Black;  // 유저 1(흑) 2(백) 진영 구분용 번호

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
            _myPlayerType = StoneType.Black; // 방장은 무조건 1번(흑) 진영
            _mySkinIndex = 0;// 임시스킨
        }
        else
        {
            _myPlayerType = StoneType.White; // 손님은 무조건 2번(백) 진영
            _mySkinIndex = 1;//임시스킨
        }
        _boardInteraction.ChangeStoneSkin(_stoneSkins[_mySkinIndex]);
        CheckAndApplyTurn();
    }
    public override void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer)
    {
        // 상대방이 들어오면 인원수가 2명이 되므로 다시 체크해서 입력을 열어줌
        Debug.Log("새 플레이어 입장! 게임을 준비합니다.");

        CheckAndApplyTurn();
    }

    private void CheckAndApplyTurn()
    {
        // 방에 2명이 안 모였다면 내 턴 권한을 주지 않음
        if (PhotonNetwork.CurrentRoom.PlayerCount < 2)
        {
            _boardInteraction.SetMyTurn(false);
            return;
        }
        // 턴 확인은 스킨 번호가 아니라 진영 번호(PlayerType)로 계산
        bool isMyTurnNow = (_isMasterTurn && _myPlayerType == StoneType.Black) || (!_isMasterTurn && _myPlayerType == StoneType.White);

        _boardInteraction.SetMyTurn(isMyTurnNow);
    }

    private void SendStoneToServer(int x, int y)
    {
        if (!PhotonNetwork.InRoom || PhotonNetwork.CurrentRoom.PlayerCount < 2)
        {
            Debug.LogWarning("상대방이 아직 입장하지 않았습니다.");
            return;
        }

        photonView.RPC("RPC_ReceiveAndDrawStone", RpcTarget.All, x, y, _myPlayerType, _mySkinIndex);
    }

    ///////////////////////////////////////////////////////////////

    private void Update()
    {
        //스킬 테스트용 로직
        // 숫자 1번 키를 누르면 (7, 7) 위치에 있는 돌을 지우는 스킬 발동
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            // "DeleteSkill"은 Resources/Skills/ 폴더 안의 에셋 이름이랑 무조건 동일하게
            UseSkill("DeleteSkill", 7, 7);
        }
        //숫자 2번 키를 누르면 강제 착수 스킬 발동
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            // 1. 타겟 좌표 설정 (여기서는 일단 테스트용으로 5, 5 고정)
            int testX = 7;
            int testY = 7;

            // 2. 스킬 이름은 Resources/Skills/ 폴더 안의 '에셋 파일 이름'과 정확히 같아야 함
            string skillAssetName = "ForceSkill";

            // 3. 발사! (우리가 만든 UseSkill 함수 호출)
            UseSkill(skillAssetName, testX, testY);

            Debug.Log($"[Test] {skillAssetName} 스킬을 {testX}, {testY} 좌표에 사용 시도!");
        }



    }
    public void SendSkillToServer(string skillPath, int x = -1, int y = -1)
    {
        // RPC를 통해 모든 클라이언트에서 스킬 실행
        photonView.RPC("RPC_ExecuteSkill", RpcTarget.All, skillPath, x, y);
    }
    /////////////////////////////////////////////////////////////


    [PunRPC]
    public void RPC_ReceiveAndDrawStone(int x, int y, StoneType playerType, int skinID)
    {
        // 1. 시각적 처리
        Sprite stoneSkin = _stoneSkins[skinID];
        _boardInteraction.PlaceStoneRemote(x, y, stoneSkin);

        // 2. 턴 변경
        _isMasterTurn = !_isMasterTurn;
        CheckAndApplyTurn();

        string playerName = (playerType == StoneType.Black) ? "플레이어1(흑돌)" : "플레이어2(백돌)";
        Debug.Log($"[{playerName}]님이 ({x}, {y}) 좌표에 착수했습니다");

        // 3. 방송 송출
        // x, y 좌표에 playerType(1번 흑 or 2번 백) 유저가 돌을 둠
        OnStonePlaced?.Invoke(x, y, playerType);
    }

    /////////////////////////////////////////////////////////////////////////
    [PunRPC]
    public void RPC_ExecuteSkill(string skillPath, int x, int y, PlayerType casterType)
    {
        // 1. Resources 폴더에서 해당 스킬 SO 로드 (경로 예: "Skills/RemoveStone")
        SkillBase skill = Resources.Load<SkillBase>(skillPath);

        if (skill == null)
        {
            Debug.LogError($"[Skill] {skillPath} 경로에서 스킬을 찾을 수 없습니다.");
            return;
        }

        // 2. 확장된 SO 구조에 맞게 데이터(좌표 + 시전자) 주입
        // 이 정보는 내부적으로 SkillContext 보따리에 담깁니다.
        skill.SetTarget(x, y, casterType);

        // 3. OmokManager를 통해 마나 체크 후 실행
        // OmokManager 내부에서 player.TryUseMagic() -> skill.Execute() 순으로 호출될 것입니다.
        if (OmokManager.Instance.TryUseMagic(skill))
        {
            Debug.Log($"[Skill] {casterType} 플레이어가 {skill.Name} 스킬을 ({x}, {y})에 사용!");
        }

    }

    //public void UseSkill(string skillName, int x, int y)
    //{
    //    // 본인의 PlayerType을 가져옵니다. (NetworkOmokManager에 정의된 _myPlayerType 활용)
    //    // 인트형이라면 (PlayerType)_myPlayerType 으로 캐스팅 필요
    //    PlayerType myType = (PlayerType)_myPlayerType;

    //    // 스킬 파일이 Resources/Skills/ 안에 있다면 경로를 맞춰줍니다.
    //    string path = "Skills/" + skillName;

    //    photonView.RPC("RPC_ExecuteSkill", RpcTarget.All, path, x, y, myType);
    //}



    // 스킬 테스트용

    public void UseSkill(string skillName, int x, int y)
    {
        // 1. 본인의 PlayerType 결정 (서버에 저장된 _myPlayerType이 1이면 Black, 2이면 White)
        PlayerType myType = (_myPlayerType == StoneType.Black) ? PlayerType.Black : PlayerType.White;

        // 2. Resources/Skills/ 폴더 내의 에셋 경로 생성
        string path = "Skills/" + skillName;

        // 3. 모든 플레이어에게 RPC로 스킬 실행 명령 전달
        // 이 RPC_ExecuteSkill 함수도 제가 이전에 드린 코드로 수정되어 있어야 합니다!
        photonView.RPC("RPC_ExecuteSkill", RpcTarget.All, path, x, y, myType);
    }
    /////////////////////////////////////////////////////////////////////////

}