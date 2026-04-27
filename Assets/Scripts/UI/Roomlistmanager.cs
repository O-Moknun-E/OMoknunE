using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Roomlistmanager : MonoBehaviour
{
    public GameObject roomPrefab; // RoomItem 프리팹
    public Transform content;     // ScrollView의 Content

    public void CreateRoom(string roomName, int count)
    {
        GameObject obj = Instantiate(roomPrefab, content);
        obj.GetComponent<Roomitem>().Setup(roomName, count);
    }
    public Transform contentParent;      // Content
    public GameObject roomItemPrefab;    // RoomItem 프리팹

    public List<Roomdata> roomList = new List<Roomdata>();

    void Start()
    {
        // 테스트용 데이터
        roomList.Add(new Roomdata { roomName = "방1", isPrivate = false });
        roomList.Add(new Roomdata { roomName = "방2", isPrivate = true });
        roomList.Add(new Roomdata { roomName = "방3", isPrivate = false });

        ShowPublicRooms(); // 처음엔 공개방 보여주기
    }

    // 🔥 공개방만
    public void ShowPublicRooms()
    {
        RefreshUI();

        foreach (Roomdata room in roomList)
        {
            if (!room.isPrivate)
            {
                CreateRoom(room);
            }
        }
    }

    // 🔥 비공개방만
    public void ShowPrivateRooms()
    {
        RefreshUI();

        foreach (Roomdata room in roomList)
        {
            if (room.isPrivate)
            {
                CreateRoom(room);
            }
        }
    }

    // UI 싹 지우기
    void RefreshUI()
    {
        foreach (Transform child in contentParent)
        {
            Destroy(child.gameObject);
        }
    }

    // 방 UI 생성
    void CreateRoom(Roomdata room)
    {
        GameObject item = Instantiate(roomItemPrefab, contentParent);

        // 이름 표시 (임시)
        item.GetComponentInChildren<UnityEngine.UI.Text>().text = room.roomName;
    }
}

    //void Start()
    //{
    //    // 테스트용
    //    for (int i = 0; i < 10; i++)
    //    {
    //        CreateRoom("Room " + i, Random.Range(0, 2));
    //    }
    //}

