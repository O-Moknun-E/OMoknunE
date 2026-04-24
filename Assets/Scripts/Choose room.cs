using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Chooseroom : MonoBehaviour
{

    public void Randomroom()
    {
        SceneManager.LoadSceneAsync("Gamescreen");
    }
    public void Createpublicroom()
    {
        SceneManager.LoadSceneAsync("Gamescreen");
    }
    public void back()
    {
        SceneManager.LoadSceneAsync("Selectmode");
    }
}
