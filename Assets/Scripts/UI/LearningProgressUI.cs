using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LearningProgressUI : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI levelText;           // 显示等级：Level 5
    public TextMeshProUGUI titleText;           // 显示称号：Beginner
    public TextMeshProUGUI expText;             // 显示经验：120/300 EXP
    public Slider expProgressBar;               // 经验值进度条
    public GameObject systemNotificationPanel;   // 系统通知面板 - 用于显示升级、成就解锁等系统消息
    public TextMeshProUGUI systemNotificationText; // 系统通知文本 - 显示升级、成就等消息内容
    public Button levelUpCloseButton;           // 关闭系统通知面板按钮
    public Button achievementButton;            // 成就按钮
    
    [Header("Animation Settings")]
    public float expBarAnimationSpeed = 2f;     // 经验条动画速度
    public AnimationCurve expBarCurve = AnimationCurve.EaseInOut(0,0,1,1);
    
    private float targetExpProgress = 0f;
    private bool isAnimatingExp = false;
    
    private void Start()
    {
        // 订阅学习进度事件
        if (LearningProgressManager.Instance != null)
        {
            LearningProgressManager.Instance.OnExpGained.AddListener(OnExpGained);
            LearningProgressManager.Instance.OnLevelUp.AddListener(OnLevelUp);
            LearningProgressManager.Instance.OnProfileUpdated.AddListener(UpdateProgressDisplay);
            
            // 初始化显示
            UpdateProgressDisplay(LearningProgressManager.Instance.userProfile);
        }
        
        // 设置升级面板关闭按钮
        if (levelUpCloseButton != null)
        {
            levelUpCloseButton.onClick.AddListener(CloseSystemNotificationPanel);
        }
        
        // 设置成就按钮
        if (achievementButton != null)
        {
            achievementButton.onClick.AddListener(ShowAchievements);
        }
        
        // 初始隐藏系统通知面板
        if (systemNotificationPanel != null)
        {
            systemNotificationPanel.SetActive(false);
        }
    }
    
    private void Update()
    {
        // 动画更新经验条
        if (isAnimatingExp && expProgressBar != null)
        {
            float currentProgress = expProgressBar.value;
            float newProgress = Mathf.Lerp(currentProgress, targetExpProgress, 
                expBarAnimationSpeed * Time.deltaTime);
            
            expProgressBar.value = newProgress;
            
            // 检查动画是否完成
            if (Mathf.Abs(newProgress - targetExpProgress) < 0.01f)
            {
                expProgressBar.value = targetExpProgress;
                isAnimatingExp = false;
            }
        }
    }
    
    public void UpdateProgressDisplay(UserProfile profile)
    {
        if (profile == null) return;
        
        // 更新等级显示
        if (levelText != null)
        {
            levelText.text = $"Level {profile.level}";
            levelText.color = profile.GetLevelColor();
        }
        
        // 更新称号显示
        if (titleText != null)
        {
            titleText.text = profile.GetLevelTitle();
            titleText.color = profile.GetLevelColor();
        }
        
        // 更新经验值显示
        if (expText != null)
        {
            int expToNext = profile.GetExpToNextLevel();
            expText.text = $"{profile.totalExp} EXP | Next: {expToNext}";
        }
        
        // 更新经验值进度条
        if (expProgressBar != null)
        {
            targetExpProgress = profile.GetLevelProgress();
            isAnimatingExp = true;
            
            // 设置进度条颜色
            var fillImage = expProgressBar.fillRect.GetComponent<Image>();
            if (fillImage != null)
            {
                fillImage.color = profile.GetLevelColor();
            }
        }
    }
    
    private void OnExpGained(int amount)
    {
        // 这里可以添加经验值获得的动画效果
        Debug.Log($"? UI: Gained {amount} EXP!");
        
        // 可以在这里添加飘字效果或其他视觉反馈
        ShowExpGainedEffect(amount);
    }
    
    private void OnLevelUp(int newLevel)
    {
        // 显示升级通知
        ShowLevelUpNotification(newLevel);
    }
    
    private void ShowExpGainedEffect(int amount)
    {
        // 这里可以实现经验值获得的视觉效果
        // 比如飘字、粒子效果等
        // 暂时用日志替代
        Debug.Log($"? +{amount} EXP animation should play here!");
    }
    
    // ========================================
    // 系统通知面板管理方法
    // 用于显示各种系统消息：升级通知、成就解锁、特殊事件等
    // ========================================
    
    /// <summary>
    /// 显示升级通知
    /// </summary>
    /// <param name="newLevel">新达到的等级</param>
    private void ShowLevelUpNotification(int newLevel)
    {
        if (systemNotificationPanel != null && systemNotificationText != null)
        {
            UserProfile profile = LearningProgressManager.Instance.userProfile;
            
            systemNotificationText.text = $"? LEVEL UP!\n\nYou are now Level {newLevel}\n({profile.GetLevelTitle()})";
            systemNotificationText.color = profile.GetLevelColor();
            
            systemNotificationPanel.SetActive(true);
            
            // 3秒后自动关闭（可选）
            Invoke(nameof(CloseSystemNotificationPanel), 3f);
        }
    }
    
    /// <summary>
    /// 关闭系统通知面板
    /// </summary>
    private void CloseSystemNotificationPanel()
    {
        if (systemNotificationPanel != null)
        {
            systemNotificationPanel.SetActive(false);
        }
    }
    
    /// <summary>
    /// 显示成就列表 - 复用系统通知面板显示成就信息
    /// </summary>
    public void ShowAchievements()
    {
        if (systemNotificationPanel != null && systemNotificationText != null)
        {
            // TODO: Enable after Unity setup
            // var achievementManager = FindObjectOfType<AchievementManager>();
            // if (achievementManager != null)
            // {
            //     string achievementText = achievementManager.GetAchievementDisplayText();
            //     systemNotificationText.text = achievementText;
            // }
            // else
            // {
                systemNotificationText.text = "? ACHIEVEMENTS\n\nAchievement system is ready!\nCreate AchievementManager GameObject to see achievements.";
            // }
            
            systemNotificationText.color = Color.white;
            systemNotificationPanel.SetActive(true);
        }
    }
    
    // 公共方法：手动刷新显示
    public void RefreshDisplay()
    {
        if (LearningProgressManager.Instance != null)
        {
            UpdateProgressDisplay(LearningProgressManager.Instance.userProfile);
        }
    }
    
    // 调试用：手动添加经验值
    [ContextMenu("Add 10 EXP (Debug)")]
    public void DebugAddExp()
    {
        if (LearningProgressManager.Instance != null)
        {
            LearningProgressManager.Instance.AddExperience(10);
        }
    }
    
    // 调试用：重置进度
    [ContextMenu("Reset Progress (Debug)")]
    public void DebugResetProgress()
    {
        if (LearningProgressManager.Instance != null)
        {
            LearningProgressManager.Instance.ResetProgress();
            RefreshDisplay();
        }
    }
}
