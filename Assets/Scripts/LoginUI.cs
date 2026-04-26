using UnityEngine;
using UnityEngine.UI;

public class LoginUI : MonoBehaviour
{
    public Button loginButton;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            loginButton.onClick.Invoke();
        }
    }
}