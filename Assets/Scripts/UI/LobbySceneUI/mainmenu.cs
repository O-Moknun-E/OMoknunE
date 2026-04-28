using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class mainmenu : MonoBehaviour
{
    public GameObject mainMenuPanel;
    public GameObject loginPanel;

    public void PlayGame()
    {
        mainMenuPanel.SetActive(false);
        loginPanel.SetActive(true);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
