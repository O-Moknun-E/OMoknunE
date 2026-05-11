using Photon.Pun;
using UnityEngine;
using ExitGames.Client.Photon;
using Photon.Realtime;

public class GameMatcher : MonoBehaviourPunCallbacks
{

    int maxCount = 2;

    public void OnClickQuickMatch() //매칭시도
    {
        Debug.Log("랜덤 매칭 시도 중...");
        Hashtable expectedProps = new Hashtable { { RoomKeys.IsRandomMatch, true } };
        PhotonNetwork.JoinRandomRoom(expectedProps, (byte)maxCount);
    }

    public override void OnJoinRandomFailed(short returnCode, string message) //매칭 실패시 새로방을 만듬
    {
        Debug.Log("랜덤 방 없음, 전용 방 생성함");
        RoomOptions options = new RoomOptions { MaxPlayers = (byte)maxCount };
        Hashtable props = new Hashtable { { RoomKeys.IsRandomMatch, true } };

        options.CustomRoomProperties = props;
        options.CustomRoomPropertiesForLobby = new string[] { RoomKeys.IsRandomMatch };

        PhotonNetwork.CreateRoom(null, options);
    }

    public override void OnJoinedRoom()
    {
        if (RoomMaker.Instance != null) RoomMaker.Instance.CheckAndStartGame();
    }
}
