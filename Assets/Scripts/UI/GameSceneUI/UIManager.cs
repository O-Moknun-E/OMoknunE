using UnityEngine;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    //-----기권버튼-----
    public GameObject giveUpPopup;                      

    // 기권 버튼 눌렀을 때
    public void OpenPopup()
    {
        giveUpPopup.SetActive(true);
    }

    // 아니오 버튼
    public void ClosePopup()
    {
        giveUpPopup.SetActive(false);
    }

    // 예 버튼
    public void ConfirmGiveUp()
    {
        SceneManager.LoadScene("LobbySceneUI");
    }
    //-----세팅버튼-----
    public GameObject settingsPopup;

    public void OpenSettings()
    {
        settingsPopup.SetActive(true);
    }

}
