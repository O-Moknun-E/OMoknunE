using PlayFab.ClientModels;
using PlayFab;
using UnityEngine;

public class Wallet : MonoBehaviour
{
    private string rewardCord = "SD";
    private int currentMoney;

    private void OnEnable()
    {
        RewardManager.OnRewardGranted += RefreshWallet;
    }

    private void OnDisable()
    {
        RewardManager.OnRewardGranted -= RefreshWallet;
    }
    public int CurrentStarDust => currentMoney; //현재 돈 체크

    public void RefreshWallet()
    {
        var request = new GetUserInventoryRequest();
        PlayFabClientAPI.GetUserInventory(request, OnGetInventorySuccess, OnGetInventoryFailure);
    }

    public void SpendMoney(int price) //돈을 사용할때
    {
        if (!CheckMoney(price))
            return;

        var request = new SubtractUserVirtualCurrencyRequest
        {
            VirtualCurrency = rewardCord,
            Amount = price
        };

        PlayFabClientAPI.SubtractUserVirtualCurrency(request, OnSpendSuccess, OnSpendFailure);
    }


    public bool CheckMoney(int price)
    {
        return currentMoney >= price;
    }

    #region 콜백 메서드

    private void OnSpendSuccess(ModifyUserVirtualCurrencyResult result) => currentMoney = result.Balance;

    private void OnSpendFailure(PlayFabError error) => Debug.LogError($"구매 요청 실패: {error.GenerateErrorReport()}");

    private void OnGetInventorySuccess(GetUserInventoryResult result)
    {
        if (result.VirtualCurrency.TryGetValue(rewardCord, out int balance))
            currentMoney = balance;
    }

    private void OnGetInventoryFailure(PlayFabError error) => Debug.LogError($"지갑 정보 동기화 실패: {error.GenerateErrorReport()}");

    #endregion

}
