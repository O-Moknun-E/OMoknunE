using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RoomItemUI : MonoBehaviour
{
    
    public Button button;
    public TMP_Text roomNameText;
    public TMP_Text typeText;



    RoomData data;
    Roomlistmanager manager;

    public void Setup(RoomData roomData, Roomlistmanager mgr)
    {
        data = roomData;
        manager = mgr;

        roomNameText.text = data.roomName;
        typeText.text = data.type == 0 ? "Public" : "Private";

        button.onClick.AddListener(OnClickRoom);
    }

    void OnClickRoom()
    {
       // manager.JoinRoom(data);
    }
}