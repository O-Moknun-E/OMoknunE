using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class NetworkManager : MonoBehaviourPunCallbacks
{

    //회원가입

    public void Disconnect() => PhotonNetwork.Disconnect();
    public void StartConnection() => PhotonNetwork.ConnectUsingSettings();

    public override void OnConnectedToMaster()
    {
        Debug.Log("마스터 서버 접속 성공! 이제 로비로 들어감.");

        PhotonNetwork.JoinLobby();
    }

    //public void CteateRoom() => PhotonNetwork.CreateRoom() 룸이름,유형
    public void JoinToRandomRoom() => PhotonNetwork.JoinRandomRoom();
    public void LeaveRoom() => PhotonNetwork.LeaveRoom();

}
