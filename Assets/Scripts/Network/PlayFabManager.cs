using PlayFab.ClientModels;
using PlayFab;
using UnityEngine;
using TMPro;

public class PlayFabManager : MonoBehaviour
{
    public static PlayFabManager Instance;
    public  TMP_InputField emailInput, passwordInput, userNameInput; // ui작업시 삭제

    [Header("UI 패널 연결")]
    public GameObject loginUIPanel; // 로그인/회원가입 화면 그룹
    public GameObject lobbyUIPanel; // 로비(방 만들기/입장) 화면 그룹

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
        var request = new RegisterPlayFabUserRequest { Email = emailInput.text, Password = passwordInput.text, Username = userNameInput.text };
        PlayFabClientAPI.RegisterPlayFabUser(request, OnRegusterSuccess, OnRegusterFailure);
    }

    private void OnLoginSuccess(LoginResult result)
    {
        Debug.Log("로그인성공");
        NetworkManager.Instance.Connect();
    }

    private void OnLoginFailure(PlayFabError error)
    {
        Debug.LogError("로그인실패");

        string userMassge = PlayFabErrorHandler.GetErrorMessage(error.Error);
        Debug.Log(userMassge); // 이부분 나중에 ui text로 띄울것
    }

    private void OnRegusterSuccess(RegisterPlayFabUserResult result) => Debug.Log("회원가입 성공");
    private void OnRegusterFailure(PlayFabError error)
    {
        Debug.LogError("회원가입 실패");

        string userMassge = PlayFabErrorHandler.GetErrorMessage(error.Error);
        Debug.Log(userMassge); // 이부분 나중에 ui text로 띄울것
    }


}
