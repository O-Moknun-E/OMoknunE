using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;
using TMPro;


public static class RoomKeys
{
    public const string Password = "Password";
    public const string IsRandomMatch = "IsRandomMatch";
    public const string GameScene = "GameScene";
    public const string HostName = "HostName";
}

public class RoomMaker : MonoBehaviourPunCallbacks
{

    public static RoomMaker Instance;

    [Header("UI References")]
    public TMP_InputField roomNameInput;
    public TMP_InputField passwordInput;

    [Header("Settings")]
    private int maxCount = 2;
    private bool isSceneLoading = false;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }


    public void CreateRoom() //방만들기
    {
        // 로비 접속 확인
        if(!NetworkManager.Instance.IsInLobby)
        {
            Debug.LogWarning("로비 접속 중입니다. 잠시 후 다시 시도해주세요.");
            return;
        }

        if (string.IsNullOrEmpty(roomNameInput.text)) return;

        RoomOptions roomOptions = new RoomOptions { MaxPlayers = (byte)maxCount };
        Hashtable customProps = new Hashtable
        {
            { RoomKeys.IsRandomMatch, false },
            { RoomKeys.HostName, PlayFabManager.Instance.UserNickName } // 현재 플레이어의 닉네임 저장
        };

        if (!string.IsNullOrEmpty(passwordInput.text))
            customProps.Add(RoomKeys.Password, passwordInput.text);


        roomOptions.CustomRoomProperties = customProps;
        roomOptions.CustomRoomPropertiesForLobby = new string[]
        {
            RoomKeys.Password,
            RoomKeys.IsRandomMatch,
            RoomKeys.HostName
        };

        PhotonNetwork.CreateRoom(roomNameInput.text, roomOptions);
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("방 입장 성공! 플레이어 대기 중...");
    }

    public override void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer) => CheckAndStartGame();

    public void CheckAndStartGame() //방상태 체크
    {
        if (!PhotonNetwork.IsMasterClient || isSceneLoading) return;

        if (PhotonNetwork.CurrentRoom.PlayerCount == maxCount)
        {
            isSceneLoading = true;
            Debug.Log("모든 조건 충족: 게임 씬으로 이동함");
            PhotonNetwork.LoadLevel(RoomKeys.GameScene);
        }
    }

    public void OnClickExit() => PhotonNetwork.LeaveRoom();

    public override void OnLeftRoom() => isSceneLoading = false;

}



