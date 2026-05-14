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
        // 로그인 되어있을때만 PlayFab 로그아웃 처리
        if(PlayFabManager.Instance != null && PlayFabManager.Instance.SuccessLogin)
        {
            PlayFabManager.Instance.Logout();
        }

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
    // ===== 커스텀 창 추가 =====
    public GameObject customizationPanel;

    // 커스텀 창 열기
    public void OpenCustomization()
    {
        customizationPanel.SetActive(true);
    }

    // 커스텀 창 닫기
    public void CloseCustomization()
    {
        customizationPanel.SetActive(false);
    }



}
