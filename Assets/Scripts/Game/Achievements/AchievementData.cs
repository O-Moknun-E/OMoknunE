using System.Collections.Generic;

[System.Serializable]
public class AchievementData 
{
    public string ID;
    public int Reward;
    public string Title;
    public string Description;
}

[System.Serializable]
public class AchievementContainer
{
    public Dictionary<string, AchievementData> achievements;
}