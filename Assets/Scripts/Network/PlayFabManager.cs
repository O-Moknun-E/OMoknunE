using PlayFab.ClientModels;
using PlayFab;
using UnityEngine;
using TMPro;

public class PlayFabManager : MonoBehaviour
{
    public static PlayFabManager Instance;
    public  TMP_InputField emailInput, passwordInput, userNameInput; // ui작업시 삭제


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


/*    private void CheckLoginStatus()
    {
        // 유저 데이터 가져오기
        PlayFabClientAPI.GetUserData(new GetUserDataRequest(), result => {
            if (result.Data != null && result.Data.ContainsKey("IsLoggedIn"))
            {
                if (result.Data["IsLoggedIn"].Value == "True")
                {
                    Debug.LogWarning("이미 다른 기기에서 접속 중임!");
                    // 여기서 로그아웃 처리 혹은 팝업 띄우기
                    return;
                }
            }

            // 접속 중이 아니면 상태를 True로 변경
            SetLoginStatus("True");
        }, OnError);
    }
    private void SetLoginStatus(string status)
    {
        var request = new UpdateUserDataRequest
        {
            Data = new Dictionary<string, string> { { "IsLoggedIn", status } }
        };
        PlayFabClientAPI.UpdateUserData(request, result => Debug.Log($"접속 상태 변경: {status}"), OnError);
    }

    // 앱 종료 시 호출되는 유니티 콜백
    private void OnApplicationQuit()
    {
        SetLoginStatus("False");
    }

    private void OnError(PlayFabError error) => Debug.LogError(error.GenerateErrorReport());
*/

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
