using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;


public enum RoomFilter { All, Public, Private }


public class RoomManager : MonoBehaviourPunCallbacks
{

    public static RoomManager Instance;

    [Header("Password UI")]
    public GameObject passwordPanel;
    public TMP_InputField passwordInput;

    [Header("Room List UI")]
    public GameObject roomPrefab;
    public Transform contentParent;

    private Dictionary<string, GameObject> roomDict = new Dictionary<string, GameObject>();
    private RoomInfo selectedRoom;
    private List<RoomInfo> cachedRoomList = new List<RoomInfo>();
    private RoomFilter currentFilter = RoomFilter.All;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }
   

    public void SetFilter(int filterIndex) //공개여부 필터
    {
        currentFilter = (RoomFilter)filterIndex;
        ApplyFilter();
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        UpdateCachedRoomList(roomList); 
        SyncRoomUI(); 
        ApplyFilter(); 
    }

    private void UpdateCachedRoomList(List<RoomInfo> roomList)//방리스트 업데이트
    {
        foreach (var room in roomList)
        {
            int index = cachedRoomList.FindIndex(r => r.Name == room.Name);
            if (index != -1)
            {
                if (room.RemovedFromList) cachedRoomList.RemoveAt(index);
                else cachedRoomList[index] = room;
            }
            else if (!room.RemovedFromList) cachedRoomList.Add(room);
        }
    }

    private void SyncRoomUI()
    {
        var removedRooms = roomDict.Keys.Where(name => !cachedRoomList.Any(r => r.Name == name)).ToList();
        foreach (var name in removedRooms)
        {
            Destroy(roomDict[name]);
            roomDict.Remove(name);
        }

        foreach (var info in cachedRoomList)
        {
            if (!roomDict.ContainsKey(info.Name))
            {
                GameObject obj = Instantiate(roomPrefab, contentParent);
                roomDict.Add(info.Name, obj);
            }
            roomDict[info.Name].GetComponent<Room>().SetRoom(info);
        }
    }

    private void ApplyFilter()
    {
        foreach (var entry in roomDict)
        {
            var info = cachedRoomList.FirstOrDefault(r => r.Name == entry.Key);
            if (info == null) continue;

            // 조건문을 데이터 기반으로 최적화 (LINQ 식 활용)
            bool hasPassword = info.CustomProperties.ContainsKey(RoomKeys.Password);

            bool isVisible = currentFilter switch
            {
                RoomFilter.Public => !hasPassword,
                RoomFilter.Private => hasPassword,
                _ => true // All인 경우
            };

            entry.Value.SetActive(isVisible);
        }
    }

    public void OpenPasswordPanel(RoomInfo info) 
    {
        selectedRoom = info;
        passwordInput.text = string.Empty; 
        passwordPanel.SetActive(true);

        
    }

    public void OnClickConfirmPassword() 
    {
        if (selectedRoom == null) return;

        if (selectedRoom.CustomProperties.TryGetValue(RoomKeys.Password, out object passObj)) 
        {
            if (passwordInput.text == passObj.ToString())
            {
                PhotonNetwork.JoinRoom(selectedRoom.Name); 
                passwordPanel.SetActive(false); 
            }
            else
            {
                Debug.LogWarning("비밀번호가 틀렸음!"); 
            }
        }
    }

    public void ClosePasswordPanel() => passwordPanel.SetActive(false);

    
}
