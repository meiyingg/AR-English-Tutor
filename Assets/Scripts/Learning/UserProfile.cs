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
    public int totalSessions = 0;           // ��ѧϰ����
    public int conversationTurns = 0;       // �Ի�����
    public int wordsLearned = 0;           // ѧ���ĵ�����
    public int consecutiveDays = 0;         // ����ѧϰ����
    public string lastLearningDate = "";    // ���ѧϰ����
    
    [Header("Achievements")]
    public bool hasUpgradedToday = false;   // �����Ƿ�������
    public bool isDailyBonusReceived = false; // ���ս����Ƿ�����ȡ
    
    // ��ȡ��ǰ�ȼ���Ҫ���ܾ���ֵ
    public int GetExpRequiredForLevel(int targetLevel)
    {
        return targetLevel * 50;
    }
    
    // ��ȡ��������һ������Ҫ�ľ���ֵ
    public int GetExpToNextLevel()
    {
        int nextLevelTotalExp = GetExpRequiredForLevel(level + 1);
        return nextLevelTotalExp - totalExp;
    }
    
    // ��ȡ��ǰ�ȼ��Ľ��Ȱٷֱ�
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
    
    // ��ȡ�ȼ��ƺ�
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
    
    // ��ȡ�ȼ���ɫ
    public Color GetLevelColor()
    {
        return level switch
        {
            >= 1 and <= 5 => new Color(0.5f, 0.8f, 0.5f),    // ��ɫ - Beginner
            >= 6 and <= 10 => new Color(0.5f, 0.7f, 1f),     // ��ɫ - Intermediate
            >= 11 and <= 15 => new Color(0.8f, 0.5f, 1f),    // ��ɫ - Advanced
            >= 16 and <= 20 => new Color(1f, 0.7f, 0.3f),    // ��ɫ - Expert
            >= 21 => new Color(1f, 0.8f, 0.2f),              // ��ɫ - Master
            _ => Color.white
        };
    }
}
