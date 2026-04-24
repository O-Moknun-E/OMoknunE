using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Joinroom : MonoBehaviour
{
   
    public void back()
    {
        SceneManager.LoadSceneAsync("Choose room");
    }
}
