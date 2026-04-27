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

        PlayFabClientAPI.UpdatePlayerStatistics(request, OnUpdateSuccess, OnUpdateFailure);
    }

    public void GetScore() //특정 플레이어의 스코어를 가지고 오고 싶을때
    {
        var request = new GetLeaderboardAroundPlayerRequest
        {
            StatisticName = "HighScore",
            PlayFabId = PlayFabManager.Instance.GetCurrntUserID(),
            MaxResultsCount = 1
        };

        PlayFabClientAPI.GetLeaderboardAroundPlayer(request, OnGetPlayerScoreSuccess, OnGetPlayerScoreFailure);
    }

    private void OnGetPlayerScoreSuccess(GetLeaderboardAroundPlayerResult result)
    {
        if (result.Leaderboard != null && result.Leaderboard.Count > 0)
        {
            var entry = result.Leaderboard[0];
            string playerName = string.IsNullOrEmpty(entry.DisplayName) ? entry.PlayFabId : entry.DisplayName;
            int score = entry.StatValue;

            Debug.Log($"{playerName}님의 점수는 {score}점임.");
        }
        else
        {
            Debug.Log("해당 플레이어의 데이터를 찾을 수 없음.");
        }
    }

    private void OnGetPlayerScoreFailure(PlayFabError error)
    {
        Debug.LogError($"플레이어 점수 조회 실패: {error.GenerateErrorReport()}");
    }

    private void OnUpdateSuccess(UpdatePlayerStatisticsResult result) => Debug.Log("점수 갱신 성공");
    private void OnUpdateFailure(PlayFabError error) => Debug.LogError(error.GenerateErrorReport());

}
