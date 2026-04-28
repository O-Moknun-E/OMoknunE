using UnityEngine;

public class CreateroomUI : MonoBehaviour
{
    public GameObject panel;
    public Roomlistmanager Roomlistmanager;

    int roomCount = 1;

    public void OpenPanel()
    {
        panel.SetActive(true);
    }

    public void ClosePanel()
    {
        panel.SetActive(false);
    }

    public void CreatePublicRoom()
    {
        string roomName = "Room" + roomCount;

        Roomlistmanager.CreateRoom(roomName, 0); // public

        roomCount++;
        panel.SetActive(false);
    }

    public void CreatePrivateRoom()
    {
        string roomName = "Room" + roomCount;

        Roomlistmanager.CreateRoom(roomName, 1); // private

        roomCount++;
        panel.SetActive(false);
    }
}