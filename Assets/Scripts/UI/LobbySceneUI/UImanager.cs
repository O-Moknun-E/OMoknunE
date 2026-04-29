using UnityEngine;

public class UImanager : MonoBehaviour
{
    public static UImanager Instance;//πŒ¡§√ﬂ∞°

    private void Awake() //πŒ¡§√ﬂ∞°
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
}
