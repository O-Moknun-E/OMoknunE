using UnityEngine;

public class CreateroomUI : MonoBehaviour
{
    public GameObject panel;
    public Roomlistmanager Roomlistmanager;

    public void OpenPanel()
    {
        panel.SetActive(true);
    }

    public void ClosePanel()
    {
        panel.SetActive(false);
    }

    int roomCount = 1;

    public void CreatePublicRoom()
    {
        string roomName = "Room" + roomCount;

        Roomlistmanager.CreateRoom(roomName, 1); // ⭐ 여기서 리스트 추가

        roomCount++;
        panel.SetActive(false);
    }

}
