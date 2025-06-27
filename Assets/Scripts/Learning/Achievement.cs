using System;
using UnityEngine;

[System.Serializable]
public class Achievement
{
    [Header("Basic Info")]
    public string id;                    // Unique achievement ID
    public string title;                 // Achievement title
    public string description;           // Achievement description
    public AchievementType type;         // Achievement category
    
    [Header("Progress")]
    public int targetValue;              // Target value to unlock
    public int currentValue;             // Current progress value
    public bool isUnlocked;              // Is achievement unlocked
    public string unlockedDate;          // Date when unlocked
    
    [Header("Reward")]
    public int rewardExp;                // EXP reward when unlocked
    
    // Get progress percentage (0.0 to 1.0)
    public float GetProgress()
    {
        if (targetValue <= 0) return 1.0f;
        return Mathf.Clamp01((float)currentValue / targetValue);
    }
    
    // Check if achievement is completed but not yet unlocked
    public bool IsReadyToUnlock()
    {
        return !isUnlocked && currentValue >= targetValue;
    }
    
    // Get formatted progress string
    public string GetProgressString()
    {
        if (isUnlocked)
            return "Unlocked";
        else
            return $"{currentValue}/{targetValue}";
    }
    
    // Get display emoji based on unlock status
    public string GetStatusEmoji()
    {
        return isUnlocked ? "?" : "?";
    }
}

public enum AchievementType
{
    Conversation,    // Chat/message related
    Learning,        // Learning sessions/scene learning
    Experience,      // EXP related achievements
    Level,           // Level related achievements
    Special          // Special conditions (time-based, streaks, etc.)
}
