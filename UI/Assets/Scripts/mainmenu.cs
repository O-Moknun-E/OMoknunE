using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Mainmenu : MonoBehaviour
{
  public void PlayGame()
    {
        SceneManager.LoadSceneAsync("Selectmode");
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
