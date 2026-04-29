using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LoginUI : MonoBehaviour
{
    public TMP_InputField emailInput, passwordInput, userNameInput;

    public void TryLogin()
    {
        PlayFabManager.Instance.Login(emailInput.text, passwordInput.text);
    }

    public void TryRegister()
    {
        if (!IsValidInput())
            return;
        PlayFabManager.Instance.Register(emailInput.text, passwordInput.text, userNameInput.text);
    }


    private bool IsValidInput()
    {
        if (string.IsNullOrWhiteSpace(userNameInput.text) ||
            string.IsNullOrWhiteSpace(emailInput.text) ||
            string.IsNullOrWhiteSpace(passwordInput.text))
            return false;

        return true;
    }

}