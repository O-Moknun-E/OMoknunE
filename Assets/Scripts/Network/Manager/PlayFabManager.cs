using PlayFab.ClientModels;
using PlayFab;
using UnityEngine;
using TMPro;
using Photon.Pun;

public class PlayFabManager : Singleton<PlayFabManager>
{
    public TMP_InputField emailInput, passwordInput, userNameInput; // ui작업시 삭제

    private string userID;
    private string userNickName;

    public void Login(/*string email, string password*/) // 로그인
    {
        var request = new LoginWithEmailAddressRequest
        {
            Email = emailInput.text,
            Password = passwordInput.text,

            InfoRequestParameters = new GetPlayerCombinedInfoRequestParams
            {
                GetUserAccountInfo = true
            }
        };

        PlayFabClientAPI.LoginWithEmailAddress(request, OnLoginSuccess, OnLoginFailure);
    }


    public void Register(/*string email, string password, string useName*/) //회원가입
    {
        if (!IsValidInput())
            return;

        var request = new RegisterPlayFabUserRequest
        {
            Email = emailInput.text,
            Password = passwordInput.text,
            Username = userNameInput.text,
            DisplayName = userNameInput.text
        };

        PlayFabClientAPI.RegisterPlayFabUser(request, OnRegisterSuccess, OnRegusterFailure);
    }

    public string UserNickName => userNickName;
    public string UserID => userID;

    private bool IsValidInput()
    {
        if (string.IsNullOrWhiteSpace(userNameInput.text) ||
            string.IsNullOrWhiteSpace(emailInput.text) ||
            string.IsNullOrWhiteSpace(passwordInput.text))
            return false;

        return true;
    }

    #region 콜백메서드

    private void OnLoginSuccess(LoginResult result)
    {
        userID = result.PlayFabId;
        userNickName = result.InfoResultPayload.AccountInfo.TitleInfo.DisplayName;
        PhotonNetwork.NickName = userNickName;

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

    private void OnRegisterSuccess(RegisterPlayFabUserResult result)
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
    }

    private void OnRegusterFailure(PlayFabError error)
    {
        Debug.LogError("회원가입 실패");

        string userMassge = PlayFabErrorHandler.GetErrorMessage(error.Error);
        Debug.Log(userMassge); // 이부분 나중에 ui text로 띄울것
    }

    #endregion
}
