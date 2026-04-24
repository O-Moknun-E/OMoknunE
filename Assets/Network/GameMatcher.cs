using Photon.Pun;
using UnityEngine;
using ExitGames.Client.Photon;

public class GameMatcher : MonoBehaviourPunCallbacks
{

    int maxCount = 2;

    public void OnClickQuickMatch()
    {
        Debug.Log("매칭 시작...");
        Hashtable expectedCustomProps = new Hashtable { { "IsLocked", false } };

        PhotonNetwork.JoinRandomRoom(expectedCustomProps, maxCount);
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.Log("현재 빈방이 없습니다.");
    }
}
