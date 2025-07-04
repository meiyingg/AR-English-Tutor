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
    public int totalSessions = 0;           // Total learning sessions
    public int conversationTurns = 0;       // Number of conversation turns
    public int wordsLearned = 0;            // Number of words learned
    public int consecutiveDays = 0;         // Consecutive learning days
    public string lastLearningDate = "";     // Last learning date
    
    [Header("Achievements")]
    public bool hasUpgradedToday = false;    // Has leveled up today
    public bool isDailyBonusReceived = false; // Daily bonus received today
    
    // Gets the total EXP required to reach a specific level.
    public int GetTotalExpForLevel(int targetLevel)
    {
        if (targetLevel <= 1) return 0;
        int requiredExp = 0;
        // Sum of EXP for all previous levels (L1 needs 50, L2 needs 100, etc.)
        for (int i = 1; i < targetLevel; i++)
        {
            requiredExp += i * 50;
        }
        return requiredExp;
    }

    // Gets the EXP needed to complete the current level.
    public int GetExpForCurrentLevel()
    {
        return level * 50;
    }

    // Gets the current EXP accumulated within the current level.
    public int GetCurrentExpInLevel()
    {
        int expForCurrentLevel = GetTotalExpForLevel(level);
        return totalExp - expForCurrentLevel;
    }
    
    // Gets the progress towards the next level as a percentage (0.0 to 1.0).
    public float GetLevelProgress()
    {
        int expNeeded = GetExpForCurrentLevel();
        if (expNeeded == 0) return 0;
        
        int currentExpInLevel = GetCurrentExpInLevel();
        return Mathf.Clamp01((float)currentExpInLevel / expNeeded);
    }

    // Gets the EXP required to level up to the next level.
    public int GetExpToNextLevel()
    {
        int expForNext = GetExpForCurrentLevel();
        int currentExpInLevel = GetCurrentExpInLevel();
        return expForNext - currentExpInLevel;
    }
    
    // Gets the level title
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
    
    // Gets the level color
    public Color GetLevelColor()
    {
        return level switch
        {
            >= 1 and <= 5 => new Color(0.5f, 0.8f, 0.5f),    // Green - Beginner
            >= 6 and <= 10 => new Color(0.5f, 0.7f, 1f),     // Blue - Intermediate
            >= 11 and <= 15 => new Color(0.8f, 0.5f, 1f),    // Purple - Advanced
            >= 16 and <= 20 => new Color(1f, 0.7f, 0.3f),    // Orange - Expert
            >= 21 => new Color(1f, 0.8f, 0.2f),              // Gold - Master
            _ => Color.white
        };
    }
}
