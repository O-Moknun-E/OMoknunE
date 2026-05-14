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

    public void QuitGame() //민정수정
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false; 
#else
        Application.Quit(); // 실제 빌드된 게임을 종료함
#endif
    }
}
