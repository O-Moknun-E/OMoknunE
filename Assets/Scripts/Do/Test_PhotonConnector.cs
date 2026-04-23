using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

// 테스트 씬에서만 쓸 1회용 포톤 접속기
public class Test_PhotonConnector : MonoBehaviourPunCallbacks
{
    private void Start()
    {
        Debug.Log("[테스트 접속기] 마스터 서버 접속 시도 중...");
        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("[테스트 접속기] 접속 완료 TestRoom 방으로 들어갑니다.");
        RoomOptions roomOptions = new RoomOptions { MaxPlayers = 2 };
        PhotonNetwork.JoinOrCreateRoom("TestRoom", roomOptions, TypedLobby.Default);
    }

    public override void OnJoinedRoom()
    {
        // 방에 완벽하게 들어왔습니다! 이제 OmokManager의 RPC 통신이 정상 작동합니다.
        Debug.Log("[테스트 접속기] 방 입장 완료");
    }
}