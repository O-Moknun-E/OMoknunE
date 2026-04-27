using PlayFab;
using PlayFab.ClientModels;
using System.Collections.Generic;
using UnityEngine;

public class RankingManager : MonoBehaviour
{

    private const string stateHighScore = "HighScore";

    public void UpdateScore(int score) //점수 추가시 사용할 메서드
    {
        var request = new UpdatePlayerStatisticsRequest
        {
            Statistics = new List<StatisticUpdate>
            {
                new StatisticUpdate { StatisticName = stateHighScore, Value = score }
            }
        };

        PlayFabClientAPI.UpdatePlayerStatistics(request,
            OnUpdateSuccess,
            OnUpdateFailure);
    }

    private void OnUpdateSuccess(UpdatePlayerStatisticsResult result) => Debug.Log("점수 갱신 성공");
    private void OnUpdateFailure(PlayFabError error) => Debug.LogError(error.GenerateErrorReport());

}
