using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using TMPro;

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

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList) //방 추가시 ui에 보여주는부분
    {
        foreach (RoomInfo info in roomList)
        {
            if (info.RemovedFromList)
            {
                if (roomDict.ContainsKey(info.Name))
                {
                    Destroy(roomDict[info.Name]);
                    roomDict.Remove(info.Name);
                }
                continue;
            }

            if (!roomDict.ContainsKey(info.Name))
            {
                GameObject obj = Instantiate(roomPrefab, contentParent);
                obj.GetComponent<Room>().SetRoom(info);
                roomDict.Add(info.Name, obj);
            }
            else roomDict[info.Name].GetComponent<Room>().SetRoom(info);
        }
    }

    public void OpenPasswordPanel(RoomInfo info)
    {
        selectedRoom = info;
        passwordPanel.SetActive(true);

    }

    public void OnClickConfirmPassword()
    {

        if (selectedRoom == null)
        {
            Debug.Log("비어있음");
            return;
        }

            if (passwordInput.text == selectedRoom.CustomProperties[RoomKeys.Password].ToString())
        {
            PhotonNetwork.JoinRoom(selectedRoom.Name);
            passwordPanel.SetActive(false);
        }
        else Debug.LogWarning("비밀번호 틀림!");
    }
}
