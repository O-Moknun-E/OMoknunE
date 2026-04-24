using UnityEngine;

public class Roomlistmanager : MonoBehaviour
{
    public GameObject roomPrefab; // RoomItem 프리팹
    public Transform content;     // ScrollView의 Content

    public void CreateRoom(string roomName, int count)
    {
        GameObject obj = Instantiate(roomPrefab, content);
        obj.GetComponent<Roomitem>().Setup(roomName, count);
    }

    //void Start()
    //{
    //    // 테스트용
    //    for (int i = 0; i < 10; i++)
    //    {
    //        CreateRoom("Room " + i, Random.Range(0, 2));
    //    }
    //}
}
