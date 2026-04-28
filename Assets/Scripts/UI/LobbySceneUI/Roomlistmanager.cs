using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Roomlistmanager : MonoBehaviour
{
    public GameObject roomItemPrefab; // 방 하나 UI
    public Transform content; // ScrollView Content
    public GameObject passwordPanel;
    public TMP_InputField passwordInput;

    

    public void QuickMatch()
    {
        SceneManager.LoadScene("GameSceneUI"); // 씬 이름 정확히 맞춰요
    }

    List<RoomData> roomList = new List<RoomData>();

    int currentFilter = -1; // -1 = All, 0 = Public, 1 = Private

    // 방 생성
    public void CreateRoom(string roomName, int type, string password)
    {
        roomList.Add(new RoomData(roomName, type));
        RefreshUI();
    }

    public void JoinRoom(RoomData room)
    {
        Debug.Log("입장: " + room.roomName);

        // 나중에 서버 붙이면 여기서 처리
        SceneManager.LoadScene("GameSceneUI");
    }
    // 필터 버튼용
    public void ShowAll()
    {
        currentFilter = -1;
        RefreshUI();
    }

    public void ShowPublic()
    {
        currentFilter = 0;
        RefreshUI();
    }

    public void ShowPrivate()
    {
        currentFilter = 1;
        RefreshUI();
    }

    // UI 다시 그리기
    void RefreshUI()
    {
        // 기존 UI 삭제
        foreach (Transform child in content)
        {
            Destroy(child.gameObject);
        }

        // 데이터 기반으로 다시 생성
        foreach (RoomData room in roomList)
        {
            if (currentFilter != -1 && room.type != currentFilter)
                continue;

            GameObject item = Instantiate(roomItemPrefab, content);
            item.GetComponent<RoomItemUI>().Setup(room, this);
        }
    }
}













//public class Roomlistmanager : MonoBehaviour
//{
//    public GameObject roomPrefab; // RoomItem 프리팹
//    public Transform content;     // ScrollView의 Content



//    public void CreateRoom(string roomName, int count)
//    {
//        GameObject obj = Instantiate(roomPrefab, content);
//        obj.GetComponent<Roomitem>().Setup(roomName, count);
//    }
//    public Transform contentParent;      // Content
//    public GameObject roomItemPrefab;    // RoomItem 프리팹

//    public List<Roomdata> roomList = new List<Roomdata>();

//    public void QuickMatch()
//    {
//        SceneManager.LoadScene("GameSceneUI"); // 씬 이름 정확히 맞춰요
//    }

//    void Start()
//    {
//        // 테스트용 데이터
//        roomList.Add(new Roomdata { roomName = "방1", isPrivate = false });
//        roomList.Add(new Roomdata { roomName = "방2", isPrivate = true });
//        roomList.Add(new Roomdata { roomName = "방3", isPrivate = false });

//        ShowPublicRooms(); // 처음엔 공개방 보여주기
//    }

//    // 🔥 공개방만
//    public void ShowPublicRooms()
//    {
//        RefreshUI();

//        foreach (Roomdata room in roomList)
//        {
//            if (!room.isPrivate)
//            {
//                CreateRoom(room);
//            }
//        }
//    }

//    // 🔥 비공개방만
//    public void ShowPrivateRooms()
//    {
//        RefreshUI();

//        foreach (Roomdata room in roomList)
//        {
//            if (room.isPrivate)
//            {
//                CreateRoom(room);
//            }
//        }
//    }

//    // UI 싹 지우기
//    void RefreshUI()
//    {
//        foreach (Transform child in contentParent)
//        {
//            Destroy(child.gameObject);
//        }
//    }

//    // 방 UI 생성
//    void CreateRoom(Roomdata room)
//    {
//        GameObject item = Instantiate(roomItemPrefab, contentParent);

//        // 이름 표시 (임시)
//        item.GetComponentInChildren<UnityEngine.UI.Text>().text = room.roomName;
//    }
//}

//    //void Start()
//    //{
//    //    // 테스트용
//    //    for (int i = 0; i < 10; i++)
//    //    {
//    //        CreateRoom("Room " + i, Random.Range(0, 2));
//    //    }
//    //}

