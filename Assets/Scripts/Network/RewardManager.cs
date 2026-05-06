using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using System;

public class RewardManager : PersistentSingleton<RewardManager>
{

    public static event Action OnRewardGranted;

    private const string rewardCord = "SD";

    private int dailyRewardAmount = 20;
    private int winRewardAmount = 100;
    private int loseRewardAmount = 40;

    public void GrantGameEndReward(bool isWin) //승패에 따른 재화
    {
        int rewardAmount = isWin ? winRewardAmount : loseRewardAmount;
        GiveReward(rewardAmount);
    }

    public void GrantDailyBonus()//데일리 보너스 재화
    {
        GiveReward(dailyRewardAmount);
    }


    public void GiveReward(int amount)
    {
        var request = new AddUserVirtualCurrencyRequest
        {
            VirtualCurrency = rewardCord,
            Amount = amount
        };

        OnRewardGranted?.Invoke();
        PlayFabClientAPI.AddUserVirtualCurrency(request, OnRewardSuccess, OnRewardFailure);

    }


    #region 콜백 메서드

    private void OnRewardSuccess(ModifyUserVirtualCurrencyResult result)
    {
        Debug.Log($"보상 지급 성공! 현재 잔액: {result.Balance}");
    }

    private void OnRewardFailure(PlayFabError error)
    {
        Debug.LogError("보상 지급 실패: " + error.GenerateErrorReport());
    }

    #endregion
}
