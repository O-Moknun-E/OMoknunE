using PlayFab.ClientModels;
using PlayFab;
using UnityEngine;
using TMPro;

public class PlayFabManager : MonoBehaviour
{
    public static PlayFabManager Instance;

    private string userID;
    private string userNickName;


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

    public void Login(string email, string password) // 로그인
    {
        var request = new LoginWithEmailAddressRequest 
        {
            Email = email, 
            Password = password,

            InfoRequestParameters = new GetPlayerCombinedInfoRequestParams
            {
                GetUserAccountInfo = true
            }
        };

        PlayFabClientAPI.LoginWithEmailAddress(request, OnLoginSuccess, OnLoginFailure);
    }
    

    public void Register(string email, string password, string useName) //회원가입
    {
        var request = new RegisterPlayFabUserRequest
        {
            Email = email,
            Password = password,
            Username = useName,
            DisplayName = useName
        };

        PlayFabClientAPI.RegisterPlayFabUser(request, result =>
        {
            Debug.Log("회원가입 성공!");

            var updateRequest = new UpdateUserTitleDisplayNameRequest
            {
                DisplayName = useName 
            };

            PlayFabClientAPI.UpdateUserTitleDisplayName(updateRequest, onUpdateSuccess => {
                Debug.Log($"리더보드 닉네임 설정 완료");
            }, error => {
                Debug.LogWarning("닉네임 설정 실패: " + error.GenerateErrorReport());
            });

        }, OnRegusterFailure); 
    }

    public string GetUserNickName()
    {
        return userNickName;
    }

    public string GetUserID()
    {
        return userID;
    }

    #region 콜백메서드

    private void OnLoginSuccess(LoginResult result)
    {
        userID = result.PlayFabId;
        userNickName = result.InfoResultPayload.AccountInfo.TitleInfo.DisplayName;
        RankingManager.Instance.GetScore();
        NetworkManager.Instance.Connect();
        RewardManager.Instance.GrantDailyBonus();
    }

    private void OnLoginFailure(PlayFabError error)
    {
        Debug.LogError("로그인실패");

        string userMassge = PlayFabErrorHandler.GetErrorMessage(error.Error);
        Debug.Log(userMassge); // 이부분 나중에 ui text로 띄울것
    }

/*    private void OnRegisterSuccess(RegisterPlayFabUserResult result)
    {
        Debug.Log("회원가입 성공!");

        var updateRequest = new UpdateUserTitleDisplayNameRequest
        {
            DisplayName = userNameInput.text 
        };

        PlayFabClientAPI.UpdateUserTitleDisplayName(updateRequest, onUpdateSuccess => {
            Debug.Log($"리더보드 닉네임 설정 완료");
        }, error => {
            Debug.LogWarning("닉네임 설정 실패: " + error.GenerateErrorReport());
        });
    }*/

    private void OnRegusterFailure(PlayFabError error)
    {
        Debug.LogError("회원가입 실패");

        string userMassge = PlayFabErrorHandler.GetErrorMessage(error.Error);
        Debug.Log(userMassge); // 이부분 나중에 ui text로 띄울것
    }

    #endregion
}
