using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Createroom : MonoBehaviour
{
    public void publicroom()
    {
        SceneManager.LoadSceneAsync("Publicroom");
    }
    public void privateroom()
    {
        SceneManager.LoadSceneAsync("Privateroom");
    }
    public void back()
    {
        SceneManager.LoadSceneAsync("Choose room");
    }
    
}
