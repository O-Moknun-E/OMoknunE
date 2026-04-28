using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Selectmode : MonoBehaviour
{
    public void selectmode()
    {
        SceneManager.LoadSceneAsync("Choose room");

    }
    public void back()
    {
        SceneManager.LoadSceneAsync("mainmenu");
    }


}
