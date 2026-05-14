using PlayFab.ClientModels;
using PlayFab;
using UnityEngine;
using Photon.Realtime;
using Photon.Pun;
using System;

public class PlayFabManager : PersistentSingleton<PlayFabManager>
{
    private string userID;
    private string userNickName;
    private string error;

    private bool successLogin = false;
    private bool successRegister = false;

    public Action OnLogin;
    public Action OnRegister;

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

    /// <summary>
    ///  플레이팹 및 포톤 로그아웃 메서드
    /// </summary>
    public void Logout()
    {
        PlayFabClientAPI.ForgetAllCredentials();
        successLogin = false;
        successRegister = false;
        userID = null;
        userNickName = null;

        // 포톤 연결되어있을 때 연결 해제
        if(PhotonNetwork.IsConnected)
        {
            NetworkManager.Instance.Disconnect();
        }

        Debug.Log("로그아웃 완료");
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
            successRegister = true;
            this.error = "회원가입 성공!";

            var updateRequest = new UpdateUserTitleDisplayNameRequest
            {
                DisplayName = useName 
            };

            PlayFabClientAPI.UpdateUserTitleDisplayName(updateRequest, onUpdateSuccess => {
                Debug.Log($"플레이어 닉네임 설정 완료");
                OnRegister?.Invoke();
            }, error => {
                this.error = error.GenerateErrorReport();
                OnRegister?.Invoke();
            });

        }, OnRegusterFailure); 
    }

    public string UserNickName => userNickName;
    public string UserID => userID;
    public string Error => error;

    public bool SuccessLogin => successLogin;
    public bool SuccessRegister => successRegister; 

    #region 콜백 메서드

    private void OnLoginSuccess(LoginResult result)
    {
        successLogin = true;

        userID = result.PlayFabId;
        userNickName = result.InfoResultPayload.AccountInfo.TitleInfo.DisplayName;
        PhotonNetwork.NickName = userNickName;

        AchievementManager.Instance.LoadAchievementDatas();
        ItemManager.Instance.LoadItemDatas();
        RankingManager.Instance.GetScore();
        NetworkManager.Instance.Connect();
        UImanager.Instance.ShowLobby();

        OnLogin?.Invoke();
    }

    private void OnLoginFailure(PlayFabError error)
    {
        successLogin = false;

        this.error = PlayFabErrorHandler.GetErrorMessage(error.Error);
        OnLogin?.Invoke();
    }

    private void OnRegusterFailure(PlayFabError error)
    {
        this.error = PlayFabErrorHandler.GetErrorMessage(error.Error);
        OnRegister?.Invoke();   
    }

    #endregion
}
