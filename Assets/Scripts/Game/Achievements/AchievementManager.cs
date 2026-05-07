using PlayFab.ClientModels;
using PlayFab;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

public class AchievementManager : Singleton<AchievementManager>
{
    public Dictionary<string, AchievementData> achievementConfigs = new Dictionary<string, AchievementData>();
    public AchievementTracker achievementTracker;

    private HashSet<string> completedAchievements = new HashSet<string>();
    private bool isUserDataLoaded = false;

    public void LoadAchievementDatas()
    {
        var request = new GetTitleDataRequest { Keys = new List<string> { "AchievementList" } };
        PlayFabClientAPI.GetTitleData(request, OnLoadSuccess, OnError);

        LoadUserCompletedList();
    }

    private void LoadUserCompletedList()
    {
        PlayFabClientAPI.GetUserData(new GetUserDataRequest(), result => {
            if (result.Data.ContainsKey("CompletedAchievements"))
            {
                string[] ids = result.Data["CompletedAchievements"].Value.Split(',');
                foreach (var id in ids) completedAchievements.Add(id);
                Debug.Log("기존 달성 목록 로드 완료!");
            }
        }, OnError);
    }

    public void CheckAchievements(string statName, int currentValue)
    {
        foreach (var pair in achievementConfigs)
        {
            string achievementId = pair.Key;
            AchievementData config = pair.Value;

            if (config.StatName == statName)
            {
                if (currentValue >= config.Target)
                {
                    Debug.Log($"업적 달성 확인: {achievementId} ({config.Description})");
                    GiveRewardAndTitle(achievementId, config);
                }
            }
        }
    }

    private void GiveRewardAndTitle(string achievementId, AchievementData data)
    {
        RewardManager.Instance.GiveReward(data.Reward);

        var request = new UpdateUserDataRequest
        {
            Data = new Dictionary<string, string> { { "ActiveTitle", data.Title } }
        };

        PlayFabClientAPI.UpdateUserData(request, result => {
            Debug.Log("칭호 업데이트 완료!");
        }, OnError);
    }


    private void OnLoadSuccess(GetTitleDataResult result)
    {
        if (result.Data.ContainsKey("AchievementList"))
        {
            string json = result.Data["AchievementList"];
            var container = JsonConvert.DeserializeObject<AchievementContainer>(json);
            achievementConfigs = container.Achievements;
            Debug.Log("업적 로드 완료!");
        }
    }
    private void OnError(PlayFabError error) => Debug.LogError(error.GenerateErrorReport());
}
