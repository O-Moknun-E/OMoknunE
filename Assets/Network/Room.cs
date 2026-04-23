using UnityEngine;
using Photon.Realtime;
using TMPro;

public class Room : MonoBehaviour
{
    public TextMeshProUGUI roomNameText;
    public TextMeshProUGUI playerCountText;
    public GameObject lockIcon;

    public void SetRoom(RoomInfo info)
    {
        roomNameText.text = info.Name;
        playerCountText.text = $"{info.PlayerCount} / {info.MaxPlayers}";

        bool isPrivateRoom = info.CustomProperties.ContainsKey("Password");
        lockIcon.SetActive(isPrivateRoom);
    }
}
