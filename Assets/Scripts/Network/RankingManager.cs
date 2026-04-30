using PlayFab;
using PlayFab.ClientModels;
using System.Collections.Generic;
using UnityEngine;

public class RankingManager : MonoBehaviour
{
    public static RankingManager Instance;

    private List<StatisticUpdate> statUpdateBuffer = new List<StatisticUpdate>(1);
    private const string stateHighScore = "HighScore";

    public int CachedMyHighScore { get; private set; }

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(this.gameObject);
    }


    public void GetRanking() //현재 랭킹 확인
    {
        var request = new GetLeaderboardRequest
        {
            StartPosition = 0,
            StatisticName = stateHighScore,
            MaxResultsCount = 10,
            ProfileConstraints = new PlayerProfileViewConstraints() { ShowLocations = true, ShowDisplayName = true }
        };

        PlayFabClientAPI.GetLeaderboard(request,OnGetUserRankingSuccess,OnGetRankingFailure);

    }

    public void AddScoreAndSync(int amount) //현재 플레이어 점수 추가시 사용 메서드 (로컬점수 먼저올림)
    {
        CachedMyHighScore += amount;

        UpdateScore(CachedMyHighScore);
    }


    public void UpdateScore(int score) //서버에 점수를 올림
    {

        statUpdateBuffer.Clear(); 
        statUpdateBuffer.Add(new StatisticUpdate { StatisticName = stateHighScore, Value = score });

        var request = new UpdatePlayerStatisticsRequest
        {
            Statistics = statUpdateBuffer 
        };

        PlayFabClientAPI.UpdatePlayerStatistics(request, OnUpdateSuccess, OnUpdateFailure);
    }

    public void GetScore() //현재 플레이어의 스코어를 가지고 오고 싶을때
    {
        var request = new GetLeaderboardAroundPlayerRequest
        {
            StatisticName = stateHighScore,
            PlayFabId = PlayFabManager.Instance.UserID,
            MaxResultsCount = 1
        };

        PlayFabClientAPI.GetLeaderboardAroundPlayer(request, OnGetPlayerScoreSuccess, OnGetPlayerScoreFailure);
    }

    #region 콜백메서드

    private void OnGetUserRankingSuccess(GetLeaderboardResult result)
    {
        foreach (var player in result.Leaderboard)
        {

            string country = "??";
            if (player.Profile?.Locations != null && player.Profile.Locations.Count > 0)
            {
                country = player.Profile.Locations[0].CountryCode.Value.ToString();
            }

            string name = string.IsNullOrEmpty(player.DisplayName) ? player.PlayFabId : player.DisplayName;
            int rank = player.Position + 1; 
            int score = player.StatValue;

            Debug.Log($"{rank}위: {country} / {name} (점수: {score})");

        }
    }

    private void OnGetRankingFailure(PlayFabError error) => Debug.LogError($"랭킹 불러오기 실패: {error.GenerateErrorReport()}");

    private void OnGetPlayerScoreSuccess(GetLeaderboardAroundPlayerResult result)
    {
        if (result.Leaderboard != null && result.Leaderboard.Count > 0)
        {
            var entry = result.Leaderboard[0];
            string playerName = string.IsNullOrEmpty(entry.DisplayName) ? entry.PlayFabId : entry.DisplayName;
            int score = entry.StatValue;

            CachedMyHighScore = entry.StatValue;
        }
        else
            Debug.Log("해당 플레이어의 데이터를 찾을 수 없음.");
    }

    private void OnGetPlayerScoreFailure(PlayFabError error) => Debug.LogError($"플레이어 점수 조회 실패: {error.GenerateErrorReport()}");

    private void OnUpdateSuccess(UpdatePlayerStatisticsResult result) => Debug.Log("점수 갱신 성공");

    private void OnUpdateFailure(PlayFabError error) => Debug.LogError(error.GenerateErrorReport());

    #endregion

}
