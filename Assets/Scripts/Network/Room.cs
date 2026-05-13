using UnityEngine;
using Photon.Realtime;
using TMPro;
using UnityEngine.UI;
using Photon.Pun;

public class Room : MonoBehaviourPunCallbacks
{
    [Header("UI References")]
    public Button enterBtn;
    public TextMeshProUGUI roomName;
    public TextMeshProUGUI masterName;
    public TextMeshProUGUI playerCount;
    public Image lockImage;
    public Sprite lockIcone;
    public Sprite publicIcon;

    private RoomInfo roomInfo;
    private string targetRoomName;

    private void Start() => enterBtn.onClick.AddListener(OnClickJoin);

    public void SetRoom(RoomInfo info) //룸 셋팅
    {
        roomInfo = info;
        targetRoomName = info.Name;
        roomName.text = info.Name;
        playerCount.text = $"{info.PlayerCount} / {info.MaxPlayers}";

        if (info.CustomProperties.TryGetValue(RoomKeys.HostName, out object hostNameObj))
            masterName.text = hostNameObj.ToString();
        else
            masterName.text = "Unknown";


        bool isRandomRoom = info.CustomProperties.ContainsKey(RoomKeys.IsRandomMatch) && (bool)info.CustomProperties[RoomKeys.IsRandomMatch];

        if (isRandomRoom)
        {
            roomName.text = "[Quick] " + info.Name;
            enterBtn.interactable = false; // 랜덤매칭방은 방 버튼을 눌러서 들어갈 수 없음
        }

        if (info.CustomProperties.ContainsKey(RoomKeys.Password))
            lockImage.sprite = lockIcone;
        else
            lockImage.sprite = publicIcon;

    }

    public void OnClickJoin()
    {
        if (roomInfo.PlayerCount >= roomInfo.MaxPlayers) return;

        if (lockImage.sprite == publicIcon) PhotonNetwork.JoinRoom(targetRoomName);
        else RoomManager.Instance.OpenPasswordPanel(roomInfo);
    }
}
