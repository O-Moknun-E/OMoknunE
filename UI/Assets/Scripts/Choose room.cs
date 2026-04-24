using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Chooseroom : MonoBehaviour
{
    public void createroom()
    {
        SceneManager.LoadSceneAsync("Createroom");
    }
    public void Joinroom()
    {
        SceneManager.LoadSceneAsync("Joinroom");
    }
    public void back()
    {
        SceneManager.LoadSceneAsync("Selectmode");
    }
}
