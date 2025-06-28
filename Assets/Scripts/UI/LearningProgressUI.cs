using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LearningProgressUI : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI levelText;           // ��ʾ�ȼ���Level 5
    public TextMeshProUGUI titleText;           // ��ʾ�ƺţ�Beginner
    public TextMeshProUGUI expText;             // ��ʾ���飺120/300 EXP
    public Slider expProgressBar;               // ����ֵ������
    public GameObject systemNotificationPanel;   // ϵͳ֪ͨ��� - ������ʾ�������ɾͽ�����ϵͳ��Ϣ
    public TextMeshProUGUI systemNotificationText; // ϵͳ֪ͨ�ı� - ��ʾ�������ɾ͵���Ϣ����
    public Button levelUpCloseButton;           // �ر�ϵͳ֪ͨ��尴ť
    public Button achievementButton;            // �ɾͰ�ť
    
    [Header("Animation Settings")]
    public float expBarAnimationSpeed = 2f;     // �����������ٶ�
    public AnimationCurve expBarCurve = AnimationCurve.EaseInOut(0,0,1,1);
    
    private float targetExpProgress = 0f;
    private bool isAnimatingExp = false;
    
    private void Start()
    {
        // ����ѧϰ�����¼�
        if (LearningProgressManager.Instance != null)
        {
            LearningProgressManager.Instance.OnExpGained.AddListener(OnExpGained);
            LearningProgressManager.Instance.OnLevelUp.AddListener(OnLevelUp);
            LearningProgressManager.Instance.OnProfileUpdated.AddListener(UpdateProgressDisplay);
            
            // ��ʼ����ʾ
            UpdateProgressDisplay(LearningProgressManager.Instance.userProfile);
        }
        
        // �����������رհ�ť
        if (levelUpCloseButton != null)
        {
            levelUpCloseButton.onClick.AddListener(CloseSystemNotificationPanel);
        }
        
        // ���óɾͰ�ť
        if (achievementButton != null)
        {
            achievementButton.onClick.AddListener(ShowAchievements);
        }
        
        // ��ʼ����ϵͳ֪ͨ���
        if (systemNotificationPanel != null)
        {
            systemNotificationPanel.SetActive(false);
        }
    }
    
    private void Update()
    {
        // �������¾�����
        if (isAnimatingExp && expProgressBar != null)
        {
            float currentProgress = expProgressBar.value;
            float newProgress = Mathf.Lerp(currentProgress, targetExpProgress, 
                expBarAnimationSpeed * Time.deltaTime);
            
            expProgressBar.value = newProgress;
            
            // ��鶯���Ƿ����
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
        
        // ���µȼ���ʾ
        if (levelText != null)
        {
            levelText.text = $"Level {profile.level}";
            levelText.color = profile.GetLevelColor();
        }
        
        // ���³ƺ���ʾ
        if (titleText != null)
        {
            titleText.text = profile.GetLevelTitle();
            titleText.color = profile.GetLevelColor();
        }
        
        // ���¾���ֵ��ʾ
        if (expText != null)
        {
            int expToNext = profile.GetExpToNextLevel();
            expText.text = $"{profile.totalExp} EXP | Next: {expToNext}";
        }
        
        // ���¾���ֵ������
        if (expProgressBar != null)
        {
            targetExpProgress = profile.GetLevelProgress();
            isAnimatingExp = true;
            
            // ���ý�������ɫ
            var fillImage = expProgressBar.fillRect.GetComponent<Image>();
            if (fillImage != null)
            {
                fillImage.color = profile.GetLevelColor();
            }
        }
    }
    
    private void OnExpGained(int amount)
    {
        // ���������Ӿ���ֵ��õĶ���Ч��
        Debug.Log($"? UI: Gained {amount} EXP!");
        
        // �������������Ʈ��Ч���������Ӿ�����
        ShowExpGainedEffect(amount);
    }
    
    private void OnLevelUp(int newLevel)
    {
        // ��ʾ����֪ͨ
        ShowLevelUpNotification(newLevel);
    }
    
    private void ShowExpGainedEffect(int amount)
    {
        // �������ʵ�־���ֵ��õ��Ӿ�Ч��
        // ����Ʈ�֡�����Ч����
        // ��ʱ����־���
        Debug.Log($"? +{amount} EXP animation should play here!");
    }
    
    // ========================================
    // ϵͳ֪ͨ��������
    // ������ʾ����ϵͳ��Ϣ������֪ͨ���ɾͽ����������¼���
    // ========================================
    
    /// <summary>
    /// ��ʾ����֪ͨ
    /// </summary>
    /// <param name="newLevel">�´ﵽ�ĵȼ�</param>
    private void ShowLevelUpNotification(int newLevel)
    {
        if (systemNotificationPanel != null && systemNotificationText != null)
        {
            UserProfile profile = LearningProgressManager.Instance.userProfile;
            
            systemNotificationText.text = $"? LEVEL UP!\n\nYou are now Level {newLevel}\n({profile.GetLevelTitle()})";
            systemNotificationText.color = profile.GetLevelColor();
            
            systemNotificationPanel.SetActive(true);
            
            // 3����Զ��رգ���ѡ��
            Invoke(nameof(CloseSystemNotificationPanel), 3f);
        }
    }
    
    /// <summary>
    /// �ر�ϵͳ֪ͨ���
    /// </summary>
    private void CloseSystemNotificationPanel()
    {
        if (systemNotificationPanel != null)
        {
            systemNotificationPanel.SetActive(false);
        }
    }
    
    /// <summary>
    /// ��ʾ�ɾ��б� - ����ϵͳ֪ͨ�����ʾ�ɾ���Ϣ
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
    
    // �����������ֶ�ˢ����ʾ
    public void RefreshDisplay()
    {
        if (LearningProgressManager.Instance != null)
        {
            UpdateProgressDisplay(LearningProgressManager.Instance.userProfile);
        }
    }
    
    // �����ã��ֶ���Ӿ���ֵ
    [ContextMenu("Add 10 EXP (Debug)")]
    public void DebugAddExp()
    {
        if (LearningProgressManager.Instance != null)
        {
            LearningProgressManager.Instance.AddExperience(10);
        }
    }
    
    // �����ã����ý���
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
