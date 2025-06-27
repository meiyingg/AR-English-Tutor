using System;
using UnityEngine;
using UnityEngine.Events;

public class LearningProgressManager : MonoBehaviour
{
    public static LearningProgressManager Instance { get; private set; }
    
    [Header("User Progress")]
    public UserProfile userProfile;
    
    [Header("Reward Settings")]
    public int baseSessionReward = 10;      // 基础对话奖励
    public int continuousRewardBonus = 5;   // 连续对话奖励
    public int newWordBonus = 3;            // 新单词奖励
    public int dailyBonus = 5;              // 每日首次奖励
    
    [Header("Events")]
    public UnityEvent<int> OnExpGained;         // 获得经验值事件
    public UnityEvent<int> OnLevelUp;           // 升级事件
    public UnityEvent<UserProfile> OnProfileUpdated; // 资料更新事件
    
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
        // 检查每日奖励
        CheckDailyBonus();
        
        Debug.Log($"? Welcome back, {userProfile.playerName}!");
        Debug.Log($"? Level: {userProfile.level} ({userProfile.GetLevelTitle()})");
        Debug.Log($"? EXP: {userProfile.totalExp} | Next Level: {userProfile.GetExpToNextLevel()} EXP needed");
    }
    
    // 完成一次学习会话
    public void CompleteSession(bool isSceneLearning = true, int conversationTurns = 1)
    {
        int expGained = baseSessionReward;
        
        // 连续对话奖励
        if (conversationTurns >= 3)
        {
            expGained += continuousRewardBonus;
            Debug.Log($"? Conversation bonus: +{continuousRewardBonus} EXP!");
        }
        
        // 每日首次学习奖励
        if (!userProfile.isDailyBonusReceived)
        {
            expGained += dailyBonus;
            userProfile.isDailyBonusReceived = true;
            Debug.Log($"? Daily first study bonus: +{dailyBonus} EXP!");
        }
        
        // 更新统计
        userProfile.totalSessions++;
        userProfile.conversationTurns += conversationTurns;
        
        // 添加经验值
        AddExperience(expGained);
        
        // 更新最后学习日期
        userProfile.lastLearningDate = DateTime.Now.ToString("yyyy-MM-dd");
        
        SaveUserProfile();
    }
    
    // 学习新单词奖励
    public void LearnNewWord()
    {
        userProfile.wordsLearned++;
        AddExperience(newWordBonus);
        
        Debug.Log($"? New word learned! +{newWordBonus} EXP!");
        SaveUserProfile();
    }
    
    // 添加经验值
    public void AddExperience(int amount)
    {
        int oldLevel = userProfile.level;
        userProfile.totalExp += amount;
        
        Debug.Log($"? +{amount} EXP! Total: {userProfile.totalExp}");
        
        // 检查是否升级
        CheckLevelUp(oldLevel);
        
        OnExpGained?.Invoke(amount);
        OnProfileUpdated?.Invoke(userProfile);
    }
    
    // 检查升级
    private void CheckLevelUp(int oldLevel)
    {
        int newLevel = CalculateLevel(userProfile.totalExp);
        
        if (newLevel > oldLevel)
        {
            userProfile.level = newLevel;
            userProfile.hasUpgradedToday = true;
            
            Debug.Log($"? LEVEL UP! You are now Level {newLevel} ({userProfile.GetLevelTitle()})!");
            
            OnLevelUp?.Invoke(newLevel);
        }
    }
    
    // 根据总经验值计算等级
    private int CalculateLevel(int totalExp)
    {
        int level = 1;
        int expRequired = 0;
        
        while (true)
        {
            int expForThisLevel = level * 50; // 每级需要的经验：level1=50, level2=100, level3=150...
            
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
    
    // 检查每日奖励
    private void CheckDailyBonus()
    {
        string today = DateTime.Now.ToString("yyyy-MM-dd");
        
        if (userProfile.lastLearningDate != today)
        {
            userProfile.isDailyBonusReceived = false;
            userProfile.hasUpgradedToday = false;
            
            // 计算连续学习天数
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
                    userProfile.consecutiveDays = 0; // 断签重置
                }
            }
        }
    }
    
    // 获取学习难度级别（用于AI提示词调整）
    public string GetLearningDifficultyLevel()
    {
        return userProfile.level switch
        {
            >= 1 and <= 5 => "beginner",      // 基础词汇，简单句型
            >= 6 and <= 10 => "elementary",   // 常用短语，日常表达
            >= 11 and <= 15 => "intermediate", // 地道表达，习惯用法
            >= 16 and <= 20 => "upper-intermediate", // 正式场合，商务用语
            >= 21 => "advanced",               // 文化内容，深度交流
            _ => "beginner"
        };
    }
    
    // 保存用户资料
    public void SaveUserProfile()
    {
        string json = JsonUtility.ToJson(userProfile);
        PlayerPrefs.SetString(SAVE_KEY, json);
        PlayerPrefs.Save();
        
        Debug.Log("? User profile saved!");
    }
    
    // 加载用户资料
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
    
    // 重置进度（调试用）
    public void ResetProgress()
    {
        userProfile = new UserProfile();
        SaveUserProfile();
        Debug.Log("? User progress reset!");
    }
}
