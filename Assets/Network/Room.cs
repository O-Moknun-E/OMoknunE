using UnityEngine;
using Photon.Realtime;
using TMPro;
using UnityEngine.UI;
using Photon.Pun;

public class Room : MonoBehaviourPunCallbacks
{
    [Header("UI References")]
    public Button enterBtn;
    public TextMeshProUGUI roomNameText;
    public TextMeshProUGUI playerCountText;
    public GameObject lockIcon;

    private RoomInfo roomInfo;
    private string targetRoomName;

    private void Start() => enterBtn.onClick.AddListener(OnClickJoin);

    public void SetRoom(RoomInfo info) //룸 셋팅
    {
        roomInfo = info;
        targetRoomName = info.Name;

        roomNameText.text = info.Name;
        playerCountText.text = $"{info.PlayerCount} / {info.MaxPlayers}";

        bool isRandomRoom = info.CustomProperties.ContainsKey(RoomKeys.IsRandomMatch) && (bool)info.CustomProperties[RoomKeys.IsRandomMatch];

        if (isRandomRoom)
        {
            roomNameText.text = "[Quick] " + info.Name;
            enterBtn.interactable = false; // 랜덤매칭방은 방 버튼을 눌러서 들어갈 수 없음
        }

        lockIcon.SetActive(info.CustomProperties.ContainsKey(RoomKeys.Password));
    }

    public void OnClickJoin()
    {
        if (roomInfo.PlayerCount >= roomInfo.MaxPlayers) return;

        if (!lockIcon.activeSelf) PhotonNetwork.JoinRoom(targetRoomName);
        else RoomManager.Instance.OpenPasswordPanel(roomInfo);
    }
}
