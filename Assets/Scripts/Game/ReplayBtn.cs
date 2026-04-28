using UnityEngine;
using UnityEngine.SceneManagement;

public class ReplayBtn : MonoBehaviour
{
    //// <summary>
    /// 리플레이 목록 씬으로 이동
    /// </summary>
    public void LoadReplayList()
    {
        SceneManager.LoadScene("ReplayListScene");
    }
}
