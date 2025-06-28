using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class AchievementManager : MonoBehaviour
{
    public static AchievementManager Instance { get; private set; }
    
    [Header("Achievement System")]
    public List<Achievement> achievements = new List<Achievement>();
    
    private const string SAVE_KEY = "UserAchievements";
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeAchievements();
            LoadAchievements();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private void Start()
    {
        // Subscribe to learning events
        if (LearningProgressManager.Instance != null)
        {
            LearningProgressManager.Instance.OnExpGained.AddListener(OnExpGained);
            LearningProgressManager.Instance.OnLevelUp.AddListener(OnLevelUp);
        }
        
        // Subscribe to chat events  
        if (ChatManager.Instance != null)
        {
            ChatManager.Instance.onNewMessage.AddListener(OnNewMessage);
        }
        
        Debug.Log("? Achievement System initialized!");
    }
    
    private void InitializeAchievements()
    {
        achievements = new List<Achievement>
        {
            // Conversation Achievements
            new Achievement
            {
                id = "first_message",
                title = "Ice Breaker",
                description = "Send your first message to start learning!",
                type = AchievementType.Conversation,
                targetValue = 1,
                currentValue = 0,
                isUnlocked = false,
                rewardExp = 10
            },
            new Achievement
            {
                id = "chat_master",
                title = "Chat Master",
                description = "Have 10 conversations with your AI tutor",
                type = AchievementType.Conversation,
                targetValue = 10,
                currentValue = 0,
                isUnlocked = false,
                rewardExp = 25
            },
            new Achievement
            {
                id = "conversation_king",
                title = "Conversation King",
                description = "Send 50 messages total - you're getting fluent!",
                type = AchievementType.Conversation,
                targetValue = 50,
                currentValue = 0,
                isUnlocked = false,
                rewardExp = 50
            },
            
            // Learning Achievements
            new Achievement
            {
                id = "first_session",
                title = "First Steps",
                description = "Complete your first learning session",
                type = AchievementType.Learning,
                targetValue = 1,
                currentValue = 0,
                isUnlocked = false,
                rewardExp = 15
            },
            new Achievement
            {
                id = "dedicated_learner",
                title = "Dedicated Learner",
                description = "Complete 10 learning sessions - keep it up!",
                type = AchievementType.Learning,
                targetValue = 10,
                currentValue = 0,
                isUnlocked = false,
                rewardExp = 40
            },
            new Achievement
            {
                id = "scholar",
                title = "Scholar",
                description = "Complete 50 learning sessions - you're a true scholar!",
                type = AchievementType.Learning,
                targetValue = 50,
                currentValue = 0,
                isUnlocked = false,
                rewardExp = 100
            },
            
            // Experience Achievements
            new Achievement
            {
                id = "exp_collector",
                title = "EXP Collector",
                description = "Earn 100 total experience points",
                type = AchievementType.Experience,
                targetValue = 100,
                currentValue = 0,
                isUnlocked = false,
                rewardExp = 20
            },
            new Achievement
            {
                id = "exp_master",
                title = "EXP Master",
                description = "Earn 500 total experience points - impressive!",
                type = AchievementType.Experience,
                targetValue = 500,
                currentValue = 0,
                isUnlocked = false,
                rewardExp = 75
            },
            new Achievement
            {
                id = "exp_legend",
                title = "EXP Legend",
                description = "Earn 1000 total experience points - legendary status!",
                type = AchievementType.Experience,
                targetValue = 1000,
                currentValue = 0,
                isUnlocked = false,
                rewardExp = 150
            },
            
            // Level Achievements
            new Achievement
            {
                id = "level_up",
                title = "Rising Star",
                description = "Reach level 3 - you're on your way!",
                type = AchievementType.Level,
                targetValue = 3,
                currentValue = 0,
                isUnlocked = false,
                rewardExp = 30
            },
            new Achievement
            {
                id = "intermediate",
                title = "Intermediate",
                description = "Reach level 5 - intermediate level achieved!",
                type = AchievementType.Level,
                targetValue = 5,
                currentValue = 0,
                isUnlocked = false,
                rewardExp = 60
            },
            new Achievement
            {
                id = "advanced",
                title = "Advanced Learner",
                description = "Reach level 10 - advanced English skills unlocked!",
                type = AchievementType.Level,
                targetValue = 10,
                currentValue = 0,
                isUnlocked = false,
                rewardExp = 120
            },
            
            // Special Achievements
            new Achievement
            {
                id = "early_bird",
                title = "Early Bird",
                description = "Complete a learning session before 9 AM",
                type = AchievementType.Special,
                targetValue = 1,
                currentValue = 0,
                isUnlocked = false,
                rewardExp = 25
            },
            new Achievement
            {
                id = "night_owl",
                title = "Night Owl",
                description = "Complete a learning session after 9 PM",
                type = AchievementType.Special,
                targetValue = 1,
                currentValue = 0,
                isUnlocked = false,
                rewardExp = 25
            },
            new Achievement
            {
                id = "weekend_warrior",
                title = "Weekend Warrior",
                description = "Study during the weekend - dedication!",
                type = AchievementType.Special,
                targetValue = 1,
                currentValue = 0,
                isUnlocked = false,
                rewardExp = 20
            }
        };
    }
    
    #region Event Handlers
    
    private void OnExpGained(int amount)
    {
        // Update experience-based achievements
        if (LearningProgressManager.Instance != null)
        {
            int totalExp = LearningProgressManager.Instance.userProfile.totalExp;
            UpdateAchievementProgress("exp_collector", totalExp);
            UpdateAchievementProgress("exp_master", totalExp);
            UpdateAchievementProgress("exp_legend", totalExp);
        }
    }
    
    private void OnLevelUp(int newLevel)
    {
        // Update level-based achievements
        UpdateAchievementProgress("level_up", newLevel);
        UpdateAchievementProgress("intermediate", newLevel);
        UpdateAchievementProgress("advanced", newLevel);
    }
    
    private void OnNewMessage(string message, ChatManager.Sender sender)
    {
        // Only count user messages
        if (sender == ChatManager.Sender.User)
        {
            // Update conversation achievements
            IncrementAchievementProgress("first_message");
            IncrementAchievementProgress("chat_master");
            IncrementAchievementProgress("conversation_king");
        }
    }
    
    #endregion
    
    #region Achievement Management
    
    public void UpdateAchievementProgress(string achievementId, int newValue)
    {
        var achievement = GetAchievement(achievementId);
        if (achievement != null && !achievement.isUnlocked)
        {
            achievement.currentValue = newValue;
            CheckAndUnlockAchievement(achievement);
        }
    }
    
    public void IncrementAchievementProgress(string achievementId, int increment = 1)
    {
        var achievement = GetAchievement(achievementId);
        if (achievement != null && !achievement.isUnlocked)
        {
            achievement.currentValue += increment;
            CheckAndUnlockAchievement(achievement);
        }
    }
    
    private void CheckAndUnlockAchievement(Achievement achievement)
    {
        if (!achievement.isUnlocked && achievement.currentValue >= achievement.targetValue)
        {
            UnlockAchievement(achievement);
        }
    }
    
    private void UnlockAchievement(Achievement achievement)
    {
        achievement.isUnlocked = true;
        achievement.unlockedDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm");
        
        // Award bonus EXP for unlocking achievement
        if (achievement.rewardExp > 0 && LearningProgressManager.Instance != null)
        {
            LearningProgressManager.Instance.AddExperience(achievement.rewardExp);
            Debug.Log($"? Bonus reward: +{achievement.rewardExp} EXP for achievement!");
        }
        
        // Show notification to user
        ShowAchievementNotification(achievement);
        
        // Save progress
        SaveAchievements();
    }
    
    private void ShowAchievementNotification(Achievement achievement)
    {
        // Try to show achievement using ChatTestUI's level up panel
        var chatUI = FindObjectOfType<ChatTestUI>();
        if (chatUI != null)
        {
            // Show achievement unlock notification in UI
            chatUI.ShowAchievementUnlocked(achievement);
        }
        else
        {
            // Fallback to console log
            Debug.Log($"? Achievement Unlocked: {achievement.title}!");
            Debug.Log($"? {achievement.description}");
        }
    }
    
    #endregion
    
    #region Utility Methods
    
    public Achievement GetAchievement(string id)
    {
        return achievements.FirstOrDefault(a => a.id == id);
    }
    
    public string GetAchievementDisplayText()
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("*** ACHIEVEMENTS ***");
        sb.AppendLine();
        
        var unlockedAchievements = achievements.Where(a => a.isUnlocked).ToList();
        var lockedAchievements = achievements.Where(a => !a.isUnlocked).ToList();
        
        // 显示已解锁成就
        if (unlockedAchievements.Count > 0)
        {
            sb.AppendLine("UNLOCKED:");
            foreach (var achievement in unlockedAchievements.OrderByDescending(a => a.unlockedDate))
            {
                sb.AppendLine($"[DONE] {achievement.title}");
                sb.AppendLine($"       {achievement.description}");
                if (!string.IsNullOrEmpty(achievement.unlockedDate))
                {
                    sb.AppendLine($"       Unlocked: {achievement.unlockedDate}");
                }
                sb.AppendLine();
            }
        }
        else
        {
            sb.AppendLine("UNLOCKED: None yet - start chatting to unlock your first achievement!");
            sb.AppendLine();
        }
        
        // 显示部分进度的锁定成就
        var progressAchievements = lockedAchievements.Where(a => a.currentValue > 0).ToList();
        if (progressAchievements.Count > 0)
        {
            sb.AppendLine("IN PROGRESS:");
            foreach (var achievement in progressAchievements)
            {
                sb.AppendLine($"[....] {achievement.title}");
                sb.AppendLine($"       {achievement.description}");
                sb.AppendLine($"       Progress: {achievement.currentValue}/{achievement.targetValue}");
                sb.AppendLine();
            }
        }
        
        // 显示一些锁定的成就作为目标
        var upcomingAchievements = lockedAchievements.Where(a => a.currentValue == 0).Take(3).ToList();
        if (upcomingAchievements.Count > 0)
        {
            sb.AppendLine("UPCOMING GOALS:");
            foreach (var achievement in upcomingAchievements)
            {
                sb.AppendLine($"[LOCK] {achievement.title}");
                sb.AppendLine($"       {achievement.description}");
                sb.AppendLine($"       Goal: {achievement.targetValue} | Reward: +{achievement.rewardExp} EXP");
                sb.AppendLine();
            }
        }
        
        // 显示统计
        sb.AppendLine($"PROGRESS: {unlockedAchievements.Count}/{achievements.Count} achievements unlocked");
        
        return sb.ToString();
    }
    
    public int GetUnlockedCount()
    {
        return achievements.Count(a => a.isUnlocked);
    }
    
    public int GetTotalCount()
    {
        return achievements.Count;
    }
    
    #endregion
    
    #region Save/Load System
    
    private void SaveAchievements()
    {
        try
        {
            var saveData = new AchievementSaveData { achievements = achievements };
            string json = JsonUtility.ToJson(saveData);
            PlayerPrefs.SetString(SAVE_KEY, json);
            PlayerPrefs.Save();
            Debug.Log("? Achievements saved!");
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to save achievements: {e.Message}");
        }
    }
    
    private void LoadAchievements()
    {
        try
        {
            if (PlayerPrefs.HasKey(SAVE_KEY))
            {
                string json = PlayerPrefs.GetString(SAVE_KEY);
                var saveData = JsonUtility.FromJson<AchievementSaveData>(json);
                
                if (saveData != null && saveData.achievements != null)
                {
                    // Merge saved data with initialized achievements
                    foreach (var savedAchievement in saveData.achievements)
                    {
                        var existing = GetAchievement(savedAchievement.id);
                        if (existing != null)
                        {
                            existing.currentValue = savedAchievement.currentValue;
                            existing.isUnlocked = savedAchievement.isUnlocked;
                            existing.unlockedDate = savedAchievement.unlockedDate;
                        }
                    }
                    Debug.Log("? Achievements loaded!");
                }
            }
            else
            {
                Debug.Log("? No saved achievements found, starting fresh!");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to load achievements: {e.Message}");
        }
    }
    
    #endregion
    
    #region Debug Methods
    
    [ContextMenu("Show All Achievements")]
    public void ShowAllAchievements()
    {
        Debug.Log(GetAchievementDisplayText());
    }
    
    [ContextMenu("Reset All Achievements")]
    public void ResetAllAchievements()
    {
        foreach (var achievement in achievements)
        {
            achievement.currentValue = 0;
            achievement.isUnlocked = false;
            achievement.unlockedDate = "";
        }
        SaveAchievements();
        Debug.Log("? All achievements reset!");
    }
    
    [ContextMenu("Unlock All Achievements")]
    public void UnlockAllAchievements()
    {
        foreach (var achievement in achievements)
        {
            if (!achievement.isUnlocked)
            {
                achievement.currentValue = achievement.targetValue;
                UnlockAchievement(achievement);
            }
        }
        Debug.Log("? All achievements unlocked!");
    }
    
    #endregion
}

[System.Serializable]
public class AchievementSaveData
{
    public List<Achievement> achievements;
}
