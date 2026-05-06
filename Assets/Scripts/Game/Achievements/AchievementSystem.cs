using PlayFab.ClientModels;
using PlayFab;
using System.Collections.Generic;
using UnityEngine;

public class AchievementSystem : MonoBehaviour
{
    public Dictionary<string, AchievementData> achievementDatas = new Dictionary<string, AchievementData>();

    public void LoadAchievementDatas()
    {
        var request = new GetTitleDataRequest { Keys = new List<string> { "AchievementList" } };
    }



    private void OnError(PlayFabError error) => Debug.LogError(error.GenerateErrorReport());
}
