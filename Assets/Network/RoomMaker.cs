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
        Debug.Log("规 涝厘 己傍");
        //波具窍绰 ui
    }

    public override void OnCreatedRoom() => Debug.Log("规 积己 夸没 己傍");
    public override void OnCreateRoomFailed(short returnCode, string message) => Debug.LogError($"规 积己 角菩: {message}");

}
