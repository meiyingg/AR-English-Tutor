using System;
using UnityEngine;

[System.Serializable]
public class UserProfile
{
    [Header("Basic Info")]
    public string playerName = "English Learner";
    public int level = 1;
    public int currentExp = 0;
    public int totalExp = 0;
    
    [Header("Learning Stats")]
    public int totalSessions = 0;           // 总学习次数
    public int conversationTurns = 0;       // 对话轮数
    public int wordsLearned = 0;           // 学到的单词数
    public int consecutiveDays = 0;         // 连续学习天数
    public string lastLearningDate = "";    // 最后学习日期
    
    [Header("Achievements")]
    public bool hasUpgradedToday = false;   // 今天是否升级了
    public bool isDailyBonusReceived = false; // 今日奖励是否已领取
    
    // 获取当前等级需要的总经验值
    public int GetExpRequiredForLevel(int targetLevel)
    {
        return targetLevel * 50;
    }
    
    // 获取升级到下一级还需要的经验值
    public int GetExpToNextLevel()
    {
        int nextLevelTotalExp = GetExpRequiredForLevel(level + 1);
        return nextLevelTotalExp - totalExp;
    }
    
    // 获取当前等级的进度百分比
    public float GetLevelProgress()
    {
        if (level == 1)
        {
            return (float)totalExp / GetExpRequiredForLevel(2);
        }
        
        int currentLevelExp = GetExpRequiredForLevel(level);
        int nextLevelExp = GetExpRequiredForLevel(level + 1);
        int expInCurrentLevel = totalExp - currentLevelExp;
        int expNeededForLevel = nextLevelExp - currentLevelExp;
        
        return Mathf.Clamp01((float)expInCurrentLevel / expNeededForLevel);
    }
    
    // 获取等级称号
    public string GetLevelTitle()
    {
        return level switch
        {
            >= 1 and <= 5 => "Beginner",
            >= 6 and <= 10 => "Intermediate", 
            >= 11 and <= 15 => "Advanced",
            >= 16 and <= 20 => "Expert",
            >= 21 => "Master",
            _ => "Beginner"
        };
    }
    
    // 获取等级颜色
    public Color GetLevelColor()
    {
        return level switch
        {
            >= 1 and <= 5 => new Color(0.5f, 0.8f, 0.5f),    // 绿色 - Beginner
            >= 6 and <= 10 => new Color(0.5f, 0.7f, 1f),     // 蓝色 - Intermediate
            >= 11 and <= 15 => new Color(0.8f, 0.5f, 1f),    // 紫色 - Advanced
            >= 16 and <= 20 => new Color(1f, 0.7f, 0.3f),    // 橙色 - Expert
            >= 21 => new Color(1f, 0.8f, 0.2f),              // 金色 - Master
            _ => Color.white
        };
    }
}
