using PlayFab.ClientModels;
using PlayFab;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using Unity.VisualScripting;

public class AchievementManager : MonoBehaviour
{
    public static AchievementManager Instance;

    public AchievementTracker achievementTracker;
    public AchievementsPopup achievementsPopup;

    private static Dictionary<string, AchievementData> achievementConfigs = new Dictionary<string, AchievementData>();
    private HashSet<string> completedAchievements = new HashSet<string>();

    private string achievementKey = "AchievementList";
    private string completedAchievementKey = "CompletedAchievements";
    private bool isUserDataLoaded = false;


    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else
            Destroy(this.gameObject);
    }

    public void LoadAchievementDatas() // 접속시 업적리스트 , 이전 업적들을 로드
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

    public void CheckAchievements(string statName, int currentValue) //업적을 달성했는지 확인
    {

        foreach (var pair in achievementConfigs)
        {
            string achievementId = pair.Key;
            AchievementData config = pair.Value;

            if (completedAchievements.Contains(achievementId))
                continue;
 
            if (config.StatName == statName && currentValue >= config.Target)
            {
                completedAchievements.Add(achievementId);
                GiveReward(achievementId, config);

                achievementsPopup.Move(achievementId);
            }
        }
    }

    private void GiveReward(string achievementId, AchievementData data) //달성시 리워드 지급
    {
        RewardManager.Instance.GiveReward(data.Reward);

        string combinedIds = string.Join(",", completedAchievements);

        var request = new UpdateUserDataRequest
        {
            Data = new Dictionary<string, string> {{ completedAchievementKey, combinedIds }}
        };

        PlayFabClientAPI.UpdateUserData(request, result => {Debug.Log("업적 업데이트 완료!");}, OnError);
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
