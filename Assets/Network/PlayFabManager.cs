using PlayFab.ClientModels;
using PlayFab;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayFabManager : MonoBehaviour
{
    public  TMP_InputField idInput, passwordInput, userNameInput;

    public void Login()
    {
        var request = new LoginWithEmailAddressRequest { Email = idInput.text, Password = passwordInput.text};
        PlayFabClientAPI.LoginWithEmailAddress(request, OnLoginSuccess, OnLoginFailure);
    }

    public void Register()
    {
        var request = new RegisterPlayFabUserRequest { Email = idInput.text, Password = passwordInput.text, Username = userNameInput.text };
        PlayFabClientAPI.RegisterPlayFabUser(request, OnRegusterSuccess, OnRegusterFailure);
    }

    private void OnLoginSuccess(LoginResult result) => Debug.Log("로그인성공");
    private void OnLoginFailure(PlayFabError error) => Debug.LogError("로그인실패");
    private void OnRegusterSuccess(RegisterPlayFabUserResult result) => Debug.Log("회원가입 성공");
    private void OnRegusterFailure(PlayFabError error)
    {
        Debug.LogError("회원가입 실패");

        if (error.Error == PlayFabErrorCode.InvalidEmailAddress)
            Debug.Log("이메일을 정확히 입력해주세요");
        else if(error.Error == PlayFabErrorCode.EmailAddressNotAvailable)
            Debug.Log("이미 사용중인 이메일 입니다");
    }


}
