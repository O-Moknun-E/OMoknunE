using UnityEngine;
using Photon.Realtime;
using Photon.Pun;

public class NetworkManager : MonoBehaviourPunCallbacks
{
    public static NetworkManager Instance;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
            Destroy(this.gameObject);  
    }

    public void Connect() => PhotonNetwork.ConnectUsingSettings();
    public override void OnConnectedToMaster()
    {
        JoinLobby();
        Debug.Log("로비접속 성공");
        //로비 팝업
    }
    public void Disconnect() => PhotonNetwork.Disconnect();
    public void JoinLobby() => PhotonNetwork.JoinLobby();

}
