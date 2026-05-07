using System.Collections.Generic;

[System.Serializable]
public class AchievementData 
{
    public string StatName;    // 이 업적이 체크할 PlayFab 통계 이름
    public int Target;         // 달성 목표 수치
    public int Reward;         // 보상 금액
    public string Title;       // 지급할 칭호
    public string Description;
}

[System.Serializable]
public class AchievementContainer
{
    public Dictionary<string, AchievementData> Achievements;
}