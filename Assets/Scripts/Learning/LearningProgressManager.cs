using System;
using UnityEngine;
using UnityEngine.Events;

public class LearningProgressManager : MonoBehaviour
{
    public static LearningProgressManager Instance { get; private set; }
    
    [Header("User Progress")]
    public UserProfile userProfile;
    
    [Header("Reward Settings")]
    public int baseSessionReward = 10;      // �����Ի�����
    public int continuousRewardBonus = 5;   // �����Ի�����
    public int newWordBonus = 3;            // �µ��ʽ���
    public int dailyBonus = 5;              // ÿ���״ν���
    
    [Header("Events")]
    public UnityEvent<int> OnExpGained;         // ��þ���ֵ�¼�
    public UnityEvent<int> OnLevelUp;           // �����¼�
    public UnityEvent<UserProfile> OnProfileUpdated; // ���ϸ����¼�
    
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
        // ���ÿ�ս���
        CheckDailyBonus();
        
        Debug.Log($"? Welcome back, {userProfile.playerName}!");
        Debug.Log($"? Level: {userProfile.level} ({userProfile.GetLevelTitle()})");
        Debug.Log($"? EXP: {userProfile.totalExp} | Next Level: {userProfile.GetExpToNextLevel()} EXP needed");
    }
    
    // ���һ��ѧϰ�Ự
    public void CompleteSession(bool isSceneLearning = true, int conversationTurns = 1)
    {
        int expGained = baseSessionReward;
        
        // �����Ի�����
        if (conversationTurns >= 3)
        {
            expGained += continuousRewardBonus;
            Debug.Log($"? Conversation bonus: +{continuousRewardBonus} EXP!");
        }
        
        // ÿ���״�ѧϰ����
        if (!userProfile.isDailyBonusReceived)
        {
            expGained += dailyBonus;
            userProfile.isDailyBonusReceived = true;
            Debug.Log($"? Daily first study bonus: +{dailyBonus} EXP!");
        }
        
        // ����ͳ��
        userProfile.totalSessions++;
        userProfile.conversationTurns += conversationTurns;
        
        // ��Ӿ���ֵ
        AddExperience(expGained);
        
        // �������ѧϰ����
        userProfile.lastLearningDate = DateTime.Now.ToString("yyyy-MM-dd");
        
        SaveUserProfile();
    }
    
    // ѧϰ�µ��ʽ���
    public void LearnNewWord()
    {
        userProfile.wordsLearned++;
        AddExperience(newWordBonus);
        
        Debug.Log($"? New word learned! +{newWordBonus} EXP!");
        SaveUserProfile();
    }
    
    // ��Ӿ���ֵ
    public void AddExperience(int amount)
    {
        int oldLevel = userProfile.level;
        userProfile.totalExp += amount;
        
        Debug.Log($"? +{amount} EXP! Total: {userProfile.totalExp}");
        
        // ����Ƿ�����
        CheckLevelUp(oldLevel);
        
        OnExpGained?.Invoke(amount);
        OnProfileUpdated?.Invoke(userProfile);
    }
    
    // �������
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
    
    // �����ܾ���ֵ����ȼ�
    private int CalculateLevel(int totalExp)
    {
        int level = 1;
        int expRequired = 0;
        
        while (true)
        {
            int expForThisLevel = level * 50; // ÿ����Ҫ�ľ��飺level1=50, level2=100, level3=150...
            
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
    
    // ���ÿ�ս���
    private void CheckDailyBonus()
    {
        string today = DateTime.Now.ToString("yyyy-MM-dd");
        
        if (userProfile.lastLearningDate != today)
        {
            userProfile.isDailyBonusReceived = false;
            userProfile.hasUpgradedToday = false;
            
            // ��������ѧϰ����
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
                    userProfile.consecutiveDays = 0; // ��ǩ����
                }
            }
        }
    }
    
    // ��ȡѧϰ�Ѷȼ�������AI��ʾ�ʵ�����
    public string GetLearningDifficultyLevel()
    {
        return userProfile.level switch
        {
            >= 1 and <= 5 => "beginner",      // �����ʻ㣬�򵥾���
            >= 6 and <= 10 => "elementary",   // ���ö���ճ����
            >= 11 and <= 15 => "intermediate", // �ص���ϰ���÷�
            >= 16 and <= 20 => "upper-intermediate", // ��ʽ���ϣ���������
            >= 21 => "advanced",               // �Ļ����ݣ���Ƚ���
            _ => "beginner"
        };
    }
    
    // �����û�����
    public void SaveUserProfile()
    {
        string json = JsonUtility.ToJson(userProfile);
        PlayerPrefs.SetString(SAVE_KEY, json);
        PlayerPrefs.Save();
        
        Debug.Log("? User profile saved!");
    }
    
    // �����û�����
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
    
    // ���ý��ȣ������ã�
    public void ResetProgress()
    {
        userProfile = new UserProfile();
        SaveUserProfile();
        Debug.Log("? User progress reset!");
    }
}
