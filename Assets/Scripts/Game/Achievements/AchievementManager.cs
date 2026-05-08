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

    private string achievementKey = "AchievementList";
    private string completedAchievementKey = "CompletedAchievements";
    private bool isUserDataLoaded = false;

    public void LoadAchievementDatas()
    {

        if (isUserDataLoaded) return;

        var request = new GetTitleDataRequest { Keys = new List<string> { achievementKey } };
        PlayFabClientAPI.GetTitleData(request, OnLoadSuccess, OnError);

        LoadUserCompletedList();    
    }

    private void LoadUserCompletedList()
    {
        PlayFabClientAPI.GetUserData(new GetUserDataRequest(), result =>
        {
            if (result.Data.ContainsKey(completedAchievementKey))
            {
                string[] ids = result.Data[completedAchievementKey].Value.Split(',');
                completedAchievements.Clear();
                foreach (var id in ids) completedAchievements.Add(id);
            }

            isUserDataLoaded = true;

        }, OnError);
    }

    public void CheckAchievements(string statName, int currentValue)
    {
        foreach (var pair in achievementConfigs)
        {
            string achievementId = pair.Key;
            AchievementData config = pair.Value;

            if (completedAchievements.Contains(achievementId)) continue;

            if (config.StatName == statName && currentValue >= config.Target)
            {

                Debug.Log($"업적 달성 확인: {achievementId}");
                completedAchievements.Add(achievementId);
                GiveRewardAndTitle(achievementId, config);

            }
        }
    }

    private void GiveRewardAndTitle(string achievementId, AchievementData data)
    {
        RewardManager.Instance.GiveReward(data.Reward);

        string combinedIds = string.Join(",", completedAchievements);

        var request = new UpdateUserDataRequest
        {
            Data = new Dictionary<string, string> {{ completedAchievementKey, combinedIds }}
        };

    }


    private void OnLoadSuccess(GetTitleDataResult result)
    {
        if (result.Data.ContainsKey(achievementKey))
        {
            string json = result.Data[achievementKey];
            var container = JsonConvert.DeserializeObject<AchievementContainer>(json);
            achievementConfigs = container.Achievements;
        }
    }
    private void OnError(PlayFabError error) => Debug.LogError(error.GenerateErrorReport());
}
