using UnityEngine;
using Photon.Realtime;
using TMPro;
using UnityEngine.UI;
using Photon.Pun;

public class Room : MonoBehaviourPunCallbacks
{
    public Button enterBtn;
    public TextMeshProUGUI roomNameText;
    public TextMeshProUGUI playerCountText;
    public GameObject lockIcon;

    private RoomInfo roomInfo;

    private int currentPlayers;
    private int maxPlayers;
    private string targetRoomName;

    private void Start()
    {
        enterBtn.onClick.AddListener(OnClickJoin);
    }

    public void SetRoom(RoomInfo info)
    {
        roomInfo = info;

        targetRoomName = info.Name;
        currentPlayers = info.PlayerCount;
        maxPlayers = info.MaxPlayers;

        roomNameText.text = info.Name;
        playerCountText.text = $"{info.PlayerCount} / {info.MaxPlayers}";

        bool isPrivateRoom = info.CustomProperties.ContainsKey("Password");
        lockIcon.SetActive(isPrivateRoom);
    }

    public void OnClickJoin()
    {

        if (currentPlayers >= maxPlayers) return;

        if (!lockIcon.activeSelf)
            PhotonNetwork.JoinRoom(targetRoomName);
        else
            RoomManager.Instance.OpenPasswordPanel(roomInfo);
    }

    public override void OnJoinedRoom() => Debug.Log("방 입장 성공!");
}
