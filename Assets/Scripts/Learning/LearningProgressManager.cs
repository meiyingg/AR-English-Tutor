using System;
using UnityEngine;
using UnityEngine.Events;

public class LearningProgressManager : MonoBehaviour
{
    public static LearningProgressManager Instance { get; private set; }
    
    [Header("User Progress")]
    public UserProfile userProfile;
    
    [Header("Reward Settings")]
    public int baseSessionReward = 15;      // Base conversation reward
    public int continuousRewardBonus = 10;  // Continuous conversation bonus
    public int newWordBonus = 5;            // New word bonus
    public int dailyBonus = 20;             // Daily first study bonus
    
    [Header("Events")]
    public UnityEvent<int> OnExpGained;         // EXP gained event
    public UnityEvent<int> OnLevelUp;           // Level up event
    public UnityEvent<UserProfile> OnProfileUpdated; // Profile updated event
    
    private const string SAVE_KEY = "UserLearningProfile";
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadUserProfile();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private void Start()
    {
        // Check for daily bonus
        CheckDailyBonus();
        
        Debug.Log($"? Welcome back, {userProfile.playerName}!");
        Debug.Log($"? Level: {userProfile.level} ({userProfile.GetLevelTitle()})");
        Debug.Log($"? EXP: {userProfile.totalExp} | Next Level: {userProfile.GetExpToNextLevel()} EXP needed");
    }
    
    // Completes a learning session
    public void CompleteSession(bool isSceneLearning = true, int conversationTurns = 1)
    {
        int expGained = baseSessionReward;
        
        // Bonus for continuous conversation
        if (conversationTurns >= 3)
        {
            expGained += continuousRewardBonus;
            Debug.Log($"? Conversation bonus: +{continuousRewardBonus} EXP!");
        }
        
        // Daily first study bonus
        if (!userProfile.isDailyBonusReceived)
        {
            expGained += dailyBonus;
            userProfile.isDailyBonusReceived = true;
            Debug.Log($"? Daily first study bonus: +{dailyBonus} EXP!");
        }
        
        // Update stats
        userProfile.totalSessions++;
        userProfile.conversationTurns += conversationTurns;
        
        // Add experience
        AddExperience(expGained);
        
        // Update last learning date
        userProfile.lastLearningDate = DateTime.Now.ToString("yyyy-MM-dd");
        
        SaveUserProfile();
    }
    
    // Reward for learning a new word
    public void LearnNewWord()
    {
        userProfile.wordsLearned++;
        AddExperience(newWordBonus);
        
        Debug.Log($"? New word learned! +{newWordBonus} EXP!");
        SaveUserProfile();
    }
    
    // Adds experience points
    public void AddExperience(int amount)
    {
        int oldLevel = userProfile.level;
        userProfile.totalExp += amount;
        Debug.Log($"? +{amount} EXP! Total: {userProfile.totalExp}");
        // Check for level up
        CheckLevelUp(oldLevel);
        OnExpGained?.Invoke(amount);
        OnProfileUpdated?.Invoke(userProfile);
        SaveUserProfile(); // Ensure profile is saved after gaining EXP
    }
    
    // Checks for level up
    private void CheckLevelUp(int oldLevel)
    {
        int newLevel = CalculateLevel(userProfile.totalExp);
        if (newLevel > oldLevel)
        {
            userProfile.level = newLevel;
            userProfile.hasUpgradedToday = true;
            Debug.Log($"? LEVEL UP! You are now Level {newLevel} ({userProfile.GetLevelTitle()})!");
            OnLevelUp?.Invoke(newLevel);
            SaveUserProfile(); // Also save on level up
        }
    }
    
    // Calculates level based on total experience
    private int CalculateLevel(int totalExp)
    {
        int level = 1;
        int expRequired = 0;
        
        while (true)
        {
            int expForThisLevel = level * 50; // EXP needed for each level: L1=50, L2=100, L3=150...
            
            if (totalExp >= expRequired + expForThisLevel)
            {
                expRequired += expForThisLevel;
                level++;
            }
            else
            {
                break;
            }
        }
        
        return level;
    }
    
    // Checks for daily bonus
    private void CheckDailyBonus()
    {
        string today = DateTime.Now.ToString("yyyy-MM-dd");
        
        if (userProfile.lastLearningDate != today)
        {
            userProfile.isDailyBonusReceived = false;
            userProfile.hasUpgradedToday = false;
            
            // Calculate consecutive learning days
            if (!string.IsNullOrEmpty(userProfile.lastLearningDate))
            {
                DateTime lastDate = DateTime.Parse(userProfile.lastLearningDate);
                DateTime currentDate = DateTime.Parse(today);
                
                if ((currentDate - lastDate).Days == 1)
                {
                    userProfile.consecutiveDays++;
                }
                else if ((currentDate - lastDate).Days > 1)
                {
                    userProfile.consecutiveDays = 0; // Reset if streak is broken
                }
            }
        }
    }
    
    // Gets the learning difficulty level (for adjusting AI prompts)
    public string GetLearningDifficultyLevel()
    {
        return userProfile.level switch
        {
            >= 1 and <= 5 => "beginner",      // Basic vocabulary, simple sentences
            >= 6 and <= 10 => "elementary",   // Common phrases, daily expressions
            >= 11 and <= 15 => "intermediate", // Idiomatic expressions, common usage
            >= 16 and <= 20 => "upper-intermediate", // Formal situations, business terms
            >= 21 => "advanced",               // Cultural topics, in-depth conversation
            _ => "beginner"
        };
    }
    
    // Saves the user profile
    public void SaveUserProfile()
    {
        string json = JsonUtility.ToJson(userProfile);
        PlayerPrefs.SetString(SAVE_KEY, json);
        PlayerPrefs.Save();
        
        Debug.Log("? User profile saved!");
    }
    
    // Loads the user profile
    public void LoadUserProfile()
    {
        if (PlayerPrefs.HasKey(SAVE_KEY))
        {
            string json = PlayerPrefs.GetString(SAVE_KEY);
            userProfile = JsonUtility.FromJson<UserProfile>(json);
            Debug.Log("? User profile loaded!");
        }
        else
        {
            userProfile = new UserProfile();
            Debug.Log("? New user profile created!");
        }
    }
    
    // Resets progress (for debugging)
    public void ResetProgress()
    {
        userProfile = new UserProfile();
        SaveUserProfile();
        Debug.Log("? User progress reset!");
    }
}
