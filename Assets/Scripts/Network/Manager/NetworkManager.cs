using UnityEngine;
using Photon.Pun;

public class NetworkManager : MonoBehaviourPunCallbacks
{
    public static NetworkManager Instance;

    public bool IsInLobby { get; private set; } = false;    // 로비 접속 상태 추적

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
            Destroy(this.gameObject);

        PhotonNetwork.AutomaticallySyncScene = true;
    }

    public void Connect() => PhotonNetwork.ConnectUsingSettings();
    public override void OnConnectedToMaster()
    {
        JoinLobby();
        Debug.Log("마스터 접속 성공");
    }

    public override void OnJoinedLobby()
    {
        IsInLobby = true;
        Debug.Log("로비 접속 성공");
        //로비 팝업
    }

    public override void OnLeftLobby()
    {
        IsInLobby = false;
        Debug.Log("로비 나감");
    }

    public void Disconnect() => PhotonNetwork.Disconnect();
    public void JoinLobby() => PhotonNetwork.JoinLobby();

}
