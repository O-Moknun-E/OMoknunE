using PlayFab.ClientModels;
using PlayFab;
using UnityEngine;
using TMPro;
using Photon.Pun;

public class PlayFabManager : PersistentSingleton<PlayFabManager>
{
    private string userID;
    private string userNickName;

    public void Login(string email, string password) // 占싸깍옙占쏙옙
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


    public void Register(string email, string password, string useName) //회占쏙옙占쏙옙占쏙옙
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
            Debug.Log("회占쏙옙占쏙옙占쏙옙 占쏙옙占쏙옙!");

            var updateRequest = new UpdateUserTitleDisplayNameRequest
            {
                DisplayName = useName 
            };

            PlayFabClientAPI.UpdateUserTitleDisplayName(updateRequest, onUpdateSuccess => {
                Debug.Log($"占쏙옙占쏙옙占쏙옙占쏙옙 占싻놂옙占쏙옙 占쏙옙占쏙옙 占싹뤄옙");
            }, error => {
                Debug.LogWarning("占싻놂옙占쏙옙 占쏙옙占쏙옙 占쏙옙占쏙옙: " + error.GenerateErrorReport());
            });

        }, OnRegusterFailure); 
    }

    public string UserNickName => userNickName;
    public string UserID => userID;

    #region 占쌥뱄옙氷占쏙옙占�

    private void OnLoginSuccess(LoginResult result)
    {
        userID = result.PlayFabId;
        userNickName = result.InfoResultPayload.AccountInfo.TitleInfo.DisplayName;
        PhotonNetwork.NickName = userNickName;

        RankingManager.Instance.GetScore();
        NetworkManager.Instance.Connect();
        RewardManager.Instance.GrantDailyBonus();
        UImanager.Instance.ShowLobby();
    }

    private void OnLoginFailure(PlayFabError error)
    {
        Debug.LogError("占싸깍옙占싸쏙옙占쏙옙");

        string userMassge = PlayFabErrorHandler.GetErrorMessage(error.Error);
        Debug.Log(userMassge); // 占싱부븝옙 占쏙옙占쌩울옙 ui text占쏙옙 占쏙옙占쏙옙
    }

/*    private void OnRegisterSuccess(RegisterPlayFabUserResult result)
    {
        Debug.Log("회占쏙옙占쏙옙占쏙옙 占쏙옙占쏙옙!");

        var updateRequest = new UpdateUserTitleDisplayNameRequest
        {
            DisplayName = userNameInput.text
        };

        PlayFabClientAPI.UpdateUserTitleDisplayName(updateRequest, onUpdateSuccess => {
            Debug.Log($"占쏙옙占쏙옙占쏙옙占쏙옙 占싻놂옙占쏙옙 占쏙옙占쏙옙 占싹뤄옙");
        }, error => {
            Debug.LogWarning("占싻놂옙占쏙옙 占쏙옙占쏙옙 占쏙옙占쏙옙: " + error.GenerateErrorReport());
        });
    }*/

    private void OnRegusterFailure(PlayFabError error)
    {
        Debug.LogError("회占쏙옙占쏙옙占쏙옙 占쏙옙占쏙옙");

        string userMassge = PlayFabErrorHandler.GetErrorMessage(error.Error);
        Debug.Log(userMassge); // 占싱부븝옙 占쏙옙占쌩울옙 ui text占쏙옙 占쏙옙占쏙옙
    }

    #endregion
}
