using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using TMPro;

public class RoomManager : MonoBehaviourPunCallbacks
{
    public static RoomManager Instance;

    public TMP_InputField roomPasswordInput;
    public GameObject passwordPanel;
    public GameObject roomPrefab;
    public Transform contentParent;

    
    private Dictionary<string, GameObject> rooms = new Dictionary<string, GameObject>();
    private RoomInfo currntRoom;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(this.gameObject);
    }


    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        foreach (RoomInfo info in roomList)
        {
            if (info.RemovedFromList)
            {
                if (rooms.ContainsKey(info.Name))
                {
                    Destroy(rooms[info.Name]);
                    rooms.Remove(info.Name);
                }
                continue;
            }

            if (!rooms.ContainsKey(info.Name))
            {
                GameObject roomObj = Instantiate(roomPrefab, contentParent);
                Room newRoom = roomObj.GetComponent<Room>();
                newRoom.SetRoom(info);
                rooms.Add(info.Name, roomObj);
            }
            else
            {
                rooms[info.Name].GetComponent<Room>().SetRoom(info);
            }
        }
    }

    public void OpenPasswordPanel(RoomInfo info)
    {
        passwordPanel.SetActive(true);
        currntRoom = info; 
    }

    public void OnClickConfirmPassword()
    {
        string input = roomPasswordInput.text; 
        string actual = currntRoom.CustomProperties["Password"].ToString();

        if (input == actual)
        {
            PhotonNetwork.JoinRoom(currntRoom.Name);
            passwordPanel.SetActive(false);
        }
        else
        {
            Debug.Log("비밀번호가 틀렸음!");
        }
    }
}
