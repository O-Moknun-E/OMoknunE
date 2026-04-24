using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;
using TMPro;

public class RoomMaker : MonoBehaviourPunCallbacks
{
    public TMP_InputField roomNameInput;
    public TMP_InputField passwordInput;

    int maxCount = 2;

    public void CreateRoom()
    {
        if (string.IsNullOrEmpty(roomNameInput.text)) return;

        RoomOptions roomOptions = new RoomOptions { MaxPlayers = 2 };

        if (!string.IsNullOrEmpty(passwordInput.text))
        {
            Hashtable customProps = new Hashtable();
            customProps.Add("Password", passwordInput.text);

            roomOptions.CustomRoomProperties = customProps;
            roomOptions.CustomRoomPropertiesForLobby = new string[] { "Password" };
        }

        PhotonNetwork.CreateRoom(roomNameInput.text, roomOptions);
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("플레이어 기다리는중..");
        //꺼야하는 ui
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        if (PhotonNetwork.IsMasterClient && PhotonNetwork.CurrentRoom.PlayerCount == maxCount)
        {
            Debug.Log("인원이 다 찼음! 게임 씬으로 이동");
            PhotonNetwork.LoadLevel("GameScene"); //오목게임씬으로 이동
        }
    }

    public void OnClickExit()
    {
        PhotonNetwork.LeaveRoom();
    }

    public override void OnLeftRoom()
    {
        Debug.Log("로비로 돌아감");
    }

    public override void OnCreatedRoom() => Debug.Log("방 생성 요청 성공");
    public override void OnCreateRoomFailed(short returnCode, string message) => Debug.LogError($"방 생성 실패: {message}");

}
