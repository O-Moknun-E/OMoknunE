using PlayFab.ClientModels;
using PlayFab;
using UnityEngine;
using TMPro;
using Photon.Pun;

public class PlayFabManager : Singleton<PlayFabManager>
{
    private string userID;
    private string userNickName;

    private bool successLogin = false;
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
                Debug.Log($"플레이어 닉네임 설정 완료");
            }, error => {
                Debug.LogWarning("닉네임 설정 실패: " + error.GenerateErrorReport());
            });

        }, OnRegusterFailure); 
    }

    public string UserNickName => userNickName;
    public string UserID => userID;

    public bool SuccessLogin => successLogin;

    #region 콜백 메서드

    private void OnLoginSuccess(LoginResult result)
    {
        successLogin = true;

        userID = result.PlayFabId;
        userNickName = result.InfoResultPayload.AccountInfo.TitleInfo.DisplayName;
        PhotonNetwork.NickName = userNickName;

        AchievementManager.Instance.LoadAchievementDatas();
        RankingManager.Instance.GetScore();
        NetworkManager.Instance.Connect();
        UImanager.Instance.ShowLobby();
    }

    private void OnLoginFailure(PlayFabError error)
    {
        Debug.LogError("로그인 실패");

        string userMassge = PlayFabErrorHandler.GetErrorMessage(error.Error);
        Debug.Log(userMassge); 
    }

    private void OnRegusterFailure(PlayFabError error)
    {
        Debug.LogError("회원가입 실패");

        string userMassge = PlayFabErrorHandler.GetErrorMessage(error.Error);
        Debug.Log(userMassge); 
    }

    #endregion
}
