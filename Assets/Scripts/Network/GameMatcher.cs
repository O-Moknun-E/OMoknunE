using Photon.Pun;
using UnityEngine;

public class GameMatcher : MonoBehaviourPunCallbacks
{
    public void OnClickQuickMatch()
    {
        Debug.Log("매칭 시작...");
        PhotonNetwork.JoinRandomRoom();
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.Log("현재 빈방이 없습니다.");
    }
}
