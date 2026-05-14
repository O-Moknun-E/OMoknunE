using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using System;

public class RewardManager : PersistentSingleton<RewardManager>
{

    public static event Action OnRewardGranted;

    private const string rewardCord = "SD";

    private int winRewardAmount = 100;
    private int loseRewardAmount = 40;
    private bool isProcessing = false;

    public void GrantGameEndReward(bool isWin) //승패에 따른 재화
    {
        int rewardAmount = isWin ? winRewardAmount : loseRewardAmount;
        GiveReward(rewardAmount);
    }

    public void GiveReward(int amount)
    {
        if (isProcessing)
            return;

        isProcessing = true;

        var request = new AddUserVirtualCurrencyRequest
        {
            VirtualCurrency = rewardCord,
            Amount = amount
        };

        
        PlayFabClientAPI.AddUserVirtualCurrency(request, OnRewardSuccess, OnRewardFailure);

    } 

    public void UseReward(int amount)
    {
        var request = new AddUserVirtualCurrencyRequest
        {
            VirtualCurrency = rewardCord,
            Amount = amount
        };

    }


    #region 콜백 메서드

    private void OnRewardSuccess(ModifyUserVirtualCurrencyResult result)
    {
        isProcessing = false; // 요청 완료 후 해제
        OnRewardGranted?.Invoke();
        Debug.Log($"보상 지급 성공! 현재 잔액: {result.Balance}");
    }

    private void OnRewardFailure(PlayFabError error)
    {
        isProcessing = false;
        Debug.LogError("보상 지급 실패: " + error.GenerateErrorReport());
    }

    #endregion
}
