using System.Collections.Generic;
using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using UnityEngine.SocialPlatforms.Impl;
using UnityEditor.PackageManager;

public class AchievementTracker : MonoBehaviour
{
    private string playStatistic = "TotalGames";

    public void UpdatePlayerGameCount()
    {
        var stats = new List<StatisticUpdate>
        {
            new StatisticUpdate { StatisticName = playStatistic, Value = 1 }
        };

        var request = new UpdatePlayerStatisticsRequest
        {
            Statistics = stats
        };

        PlayFabClientAPI.UpdatePlayerStatistics(request, result =>
        {
            Debug.Log("판수 업데이트 성공!");
            GetLatestStats();
        }, OnUpdateFailure);
    }

    private void GetLatestStats()
    {
        var request = new GetPlayerStatisticsRequest { StatisticNames = new List<string> { playStatistic } };

        PlayFabClientAPI.GetPlayerStatistics(request, result =>
        {
            foreach (var stat in result.Statistics)
            {
                if (stat.StatisticName == playStatistic)
                {
                    Debug.Log("업적 확인중");
                    AchievementManager.Instance.CheckAchievements("WinCount", stat.Value);
                }

            }
        }, OnUpdateFailure);
    }

    private void OnUpdateFailure(PlayFabError error) => Debug.LogError(error.GenerateErrorReport());
}
