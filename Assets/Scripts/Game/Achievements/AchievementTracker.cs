using System.Collections.Generic;
using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;

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

        PlayFabClientAPI.UpdatePlayerStatistics(request, GetLatestStats, OnUpdateFailure);
    }

    private void GetLatestStats(UpdatePlayerStatisticsResult result)
    {
        var request = new GetPlayerStatisticsRequest { StatisticNames = new List<string> { playStatistic } };

        PlayFabClientAPI.GetPlayerStatistics(request, result =>
        {
            foreach (var stat in result.Statistics)
            {
                if (stat.StatisticName == playStatistic)
                    AchievementManager.Instance.CheckAchievements("GameCount", stat.Value);

            }
        }, OnUpdateFailure);
    }

    private void OnUpdateFailure(PlayFabError error) => Debug.LogError(error.GenerateErrorReport());
}
