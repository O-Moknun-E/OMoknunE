using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Loginscene : MonoBehaviour
{
    public void back()
    {
        SceneManager.LoadSceneAsync("mainmenu");
    }
    public void Login()
    {
        SceneManager.LoadSceneAsync("Choose room");
    }
}
