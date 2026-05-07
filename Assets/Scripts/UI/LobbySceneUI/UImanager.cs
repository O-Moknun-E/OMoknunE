using UnityEngine;

public class UImanager : MonoBehaviour
{
    public static UImanager Instance;//민정추가

    private void Awake() //민정추가
    {
        if(Instance == null)
            Instance = this;
        else
            Destroy(this.gameObject);
    }

    public GameObject mainMenuPanel;
    public GameObject loginPanel;
    public GameObject lobbyPanel;

    public void ShowMainMenu()
    {
        mainMenuPanel.SetActive(true);
        loginPanel.SetActive(false);
        lobbyPanel.SetActive(false);
    }

    public void ShowLogin()
    {
        mainMenuPanel.SetActive(false);
        loginPanel.SetActive(true);
        lobbyPanel.SetActive(false);
    }

    public void ShowLobby()
    {
        mainMenuPanel.SetActive(false);
        loginPanel.SetActive(false);
        lobbyPanel.SetActive(true);
    }

    public GameObject soundOptionPanel; //// 사운드 옵션 패널 채빈추가==========

    // 사운드 옵션 열기
    public void OpenSoundOption()
    {
        soundOptionPanel.SetActive(true);
    }

    // 사운드 옵션 닫기
    public void CloseSoundOption()
    {
        soundOptionPanel.SetActive(false);
    }
    //========================================
}
