using PlayFab.ClientModels;
using PlayFab;
using UnityEngine;
using TMPro;

public class PlayFabManager : MonoBehaviour
{
    public static PlayFabManager Instance;
    public  TMP_InputField emailInput, passwordInput, userNameInput; // ui작업시 삭제

    private string currntUserID;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
            Destroy(this.gameObject);
    }

    public void Login(/*string email, string password*/) // 로그인
    {
        var request = new LoginWithEmailAddressRequest { Email = emailInput.text, Password = passwordInput.text};
        PlayFabClientAPI.LoginWithEmailAddress(request, OnLoginSuccess, OnLoginFailure);
    }

    public void Register(/*string email, string password, string useName*/) //회원가입
    {
        var request = new RegisterPlayFabUserRequest
        {
            Email = emailInput.text,
            Password = passwordInput.text,
            Username = userNameInput.text,
            DisplayName = userNameInput.text 
        };

        PlayFabClientAPI.RegisterPlayFabUser(request, OnRegisterSuccess, OnRegusterFailure);
    }

    public string GetCurrntUserID()
    {
        return currntUserID;
    }


    private void OnLoginSuccess(LoginResult result)
    {
        currntUserID = result.PlayFabId;
        NetworkManager.Instance.Connect();
    }

    private void OnLoginFailure(PlayFabError error)
    {
        Debug.LogError("로그인실패");

        string userMassge = PlayFabErrorHandler.GetErrorMessage(error.Error);
        Debug.Log(userMassge); // 이부분 나중에 ui text로 띄울것
    }

    private void OnRegisterSuccess(RegisterPlayFabUserResult result)
    {
        Debug.Log("회원가입 성공!");

        var updateRequest = new UpdateUserTitleDisplayNameRequest
        {
            DisplayName = userNameInput.text 
        };

        PlayFabClientAPI.UpdateUserTitleDisplayName(updateRequest, onUpdateSuccess => {
            Debug.Log($"리더보드 닉네임 설정 완료: {onUpdateSuccess.DisplayName}");
        }, error => {
            Debug.LogWarning("회원가입은 됐으나 닉네임 설정 실패: " + error.GenerateErrorReport());
        });
    }

    private void OnRegusterFailure(PlayFabError error)
    {
        Debug.LogError("회원가입 실패");

        string userMassge = PlayFabErrorHandler.GetErrorMessage(error.Error);
        Debug.Log(userMassge); // 이부분 나중에 ui text로 띄울것
    }


}
