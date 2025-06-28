/*
 * MainUIManager / ChatTestUI.cs
 * 
 * This script serves as the PRIMARY UI MANAGER for the AR English Learning application.
 * It manages all major UI components including:
 * - Chat interface and conversation flow
 * - Learning progress display and updates
 * - Achievement system notifications and popups
 * - AR scene interaction UI
 * - Image upload and AI response handling
 * 
 * Note: Despite the name "ChatTestUI", this is the MAIN UI CONTROLLER
 * for the entire learning application.
 */

using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Main UI Manager for AR English Learning App
/// Handles chat, progress, achievements, and all primary user interactions
/// </summary>
public class ChatTestUI : MonoBehaviour
{
    public TMP_InputField inputField;
    public Button sendButton;
    public Button imageButton; // New image upload button
    public GameObject chatMessagePrefab; // 我们将实例化的消息预制体
    public GameObject imageMessagePrefab; // New prefab for image messages
    public Transform chatContentPanel; // ScrollView的Content对象

    [Header("Chat Panel Control")]
    public GameObject chatPanel; // Chat面板的根GameObject
    public Button toggleChatButton; // 控制显示/隐藏的按钮
    private bool isChatPanelVisible = true; // 面板显示状态

    [Header("Chat Bubble Colors")]
    public Color userBubbleColor = new Color(0.0f, 0.5f, 1.0f); // 蓝色
    public Color tutorBubbleColor = new Color(0.8f, 0.8f, 0.8f); // 灰色

    [Header("Learning Progress UI")]
    public TextMeshProUGUI levelText;           // 显示等级：Level 5
    public TextMeshProUGUI titleText;           // 显示称号：Beginner
    public TextMeshProUGUI expText;             // 显示经验：120/300 EXP
    public Slider expProgressBar;               // 经验值进度条
    public GameObject levelUpPanel;             // 升级通知面板
    public TextMeshProUGUI levelUpText;         // 升级通知文本
    public Button levelUpCloseButton;           // 关闭升级通知按钮
    public Button achievementButton;            // 成就按钮

    [Header("Progress Animation Settings")]
    public float expBarAnimationSpeed = 2f;     // 经验条动画速度
    public AnimationCurve expBarCurve = AnimationCurve.EaseInOut(0,0,1,1);
    
    private float targetExpProgress = 0f;
    private bool isAnimatingExp = false;

    void Start()
    {
        Debug.Log("? ChatTestUI: Starting...");
        
        // Connect send button
        if (sendButton != null)
        {
            sendButton.onClick.AddListener(OnSendButtonClick);
            Debug.Log("? Send button connected");
        }
        else
        {
            Debug.LogError("? Send button not assigned in Inspector!");
        }
        
        // Connect image button (only if dragged in Inspector)
        if (imageButton != null)
        {
            imageButton.onClick.AddListener(OnImageButtonClick);
            Debug.Log("? Image button connected via Inspector");
        }
        else
        {
            Debug.LogWarning("?? Image button not assigned in Inspector. Please drag ImageButton to the 'Image Button' field in ChatTestUI component.");
        }
        
        // Connect toggle chat button
        if (toggleChatButton != null)
        {
            toggleChatButton.onClick.AddListener(ToggleChatPanel);
            Debug.Log("? Toggle chat button connected");
        }
        else
        {
            Debug.LogWarning("?? Toggle chat button not assigned in Inspector.");
        }
        
        // Initialize chat panel state
        if (chatPanel != null)
        {
            chatPanel.SetActive(isChatPanelVisible);
            Debug.Log($"? Chat panel initialized - Visible: {isChatPanelVisible}");
        }
        else
        {
            Debug.LogWarning("?? Chat panel not assigned in Inspector.");
        }

        // Check required prefabs
        if (chatMessagePrefab != null)
        {
            Debug.Log("? Chat message prefab assigned");
        }
        else
        {
            Debug.LogError("? Chat message prefab not assigned in Inspector!");
        }
        
        // imageMessagePrefab is optional for now
        if (imageMessagePrefab != null)
        {
            Debug.Log("? Image message prefab assigned (future feature)");
        }
        else
        {
            Debug.Log("?? Image message prefab not assigned - using text fallback");
        }
        
        // Connect chat events
        if (ChatManager.Instance != null)
        {
            ChatManager.Instance.onNewMessage.AddListener(UpdateChatHistory);
            // Note: No longer subscribing to onNewImageMessage since we handle image messages as text messages
            Debug.Log("? Chat events connected");
        }
        else
        {
            Debug.LogError("? ChatManager.Instance is null!");
        }
        
        // Initialize Learning Progress UI
        InitializeLearningProgressUI();
        
        Debug.Log("? ChatTestUI: Setup complete");
    }

    private async void OnSendButtonClick()
    {
        string message = inputField.text;
        if (!string.IsNullOrEmpty(message))
        {
            // 禁用输入框和按钮，防止在等待AI响应时重复发送
            SetUIInteractable(false);

            await ChatManager.Instance.SendMessage(message);

            inputField.text = "";
            // 重新激活输入框和按钮
            SetUIInteractable(true);
            inputField.ActivateInputField(); // 重新聚焦到输入框
        }
    }
    
    private void OnImageButtonClick()
    {
        Debug.Log("Image button clicked - starting scene learning!");
        
        // Directly start scene learning without popup
        TestSceneRecognition();
    }
    
    private void TestSceneRecognition()
    {
#if UNITY_EDITOR
        string path = UnityEditor.EditorUtility.OpenFilePanel("Select Image for Scene Learning", "", "png,jpg,jpeg");
        if (!string.IsNullOrEmpty(path))
        {
            Debug.Log($"Selected image for scene learning: {path}");
            ProcessSceneRecognitionFile(path);
        }
        else
        {
            Debug.Log("No image selected for scene learning");
        }
#else
        Debug.Log("Scene learning would work on mobile device");
        UpdateChatHistory("? Scene learning feature clicked (mobile version needs NativeGallery plugin)", ChatManager.Sender.Tutor);
#endif
    }
    
    private async void ProcessSceneRecognitionFile(string imagePath)
    {
        try
        {
            // Read image file
            byte[] imageData = System.IO.File.ReadAllBytes(imagePath);
            
            // Create texture
            Texture2D texture = new Texture2D(2, 2);
            if (texture.LoadImage(imageData))
            {
                // Convert to base64
                string base64 = System.Convert.ToBase64String(texture.EncodeToPNG());
                
                // Get user's text input (if any)
                string userText = inputField.text.Trim();
                
                // Clear input field
                inputField.text = "";
                
                // Disable UI during processing
                SetUIInteractable(false);
                
                // Send to AI for scene recognition with user's text
                await ChatManager.Instance.SendSceneRecognitionRequest(base64, texture, userText);
                
                // Re-enable UI
                SetUIInteractable(true);
            }
            else
            {
                Debug.LogError("Failed to load image");
                UpdateChatHistory("Failed to load image. Please try a different image.", ChatManager.Sender.Tutor);
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error processing scene recognition: {e.Message}");
            UpdateChatHistory("Error processing scene recognition. Please try again.", ChatManager.Sender.Tutor);
        }
    }
    
    private void SetUIInteractable(bool interactable)
    {
        inputField.interactable = interactable;
        sendButton.interactable = interactable;
        if (imageButton != null)
            imageButton.interactable = interactable;
    }
    
    private void UpdateChatHistory(string message, ChatManager.Sender sender)
    {
        // ★★★ 最终修复：使用 SetParent(parent, false) 来确保正确的UI缩放和定位 ★★★
        // 1. 先在世界空间中创建预制体，不指定父对象
        GameObject messageRow = Instantiate(chatMessagePrefab);

        // 2. 使用 SetParent 方法，并传入 false
        //    这个 false 参数 (worldPositionStays) 是解决问题的关键
        //    它告诉Unity：“不要试图保持这个物体原来的世界位置，
        //    而是让它的位置、旋转、缩放完全根据新的父对象来重新计算。”
        messageRow.transform.SetParent(chatContentPanel, false);

        // 使用ChatMessageUI脚本来设置消息和对齐
        ChatMessageUI messageUI = messageRow.GetComponent<ChatMessageUI>();
        if (messageUI != null)
        {
            // 设置消息内容和对齐方式（true表示用户消息，右对齐）
            messageUI.SetMessage(message, sender == ChatManager.Sender.User);
        }
        
        // 设置气泡颜色
        Image bubbleImage = messageRow.GetComponentInChildren<Image>();
        if (bubbleImage != null)
        {
            bubbleImage.color = (sender == ChatManager.Sender.User) ? userBubbleColor : tutorBubbleColor;
        }
    }
    
    private void ToggleChatPanel()
    {
        isChatPanelVisible = !isChatPanelVisible;
        chatPanel.SetActive(isChatPanelVisible);
        
        // Optionally, you can change the button text or icon here
        // For example, if you have a Text component as a child of the button:
        /*
        Text buttonText = toggleChatButton.GetComponentInChildren<Text>();
        if (buttonText != null)
        {
            buttonText.text = isChatPanelVisible ? "Hide Chat" : "Show Chat";
        }
        */
        
        Debug.Log($"Chat panel visibility toggled: {isChatPanelVisible}");
    }

    // Public methods for external control
    public void ShowChatPanel()
    {
        if (chatPanel != null)
        {
            isChatPanelVisible = true;
            chatPanel.SetActive(true);
            Debug.Log("Chat panel shown");
        }
    }

    public void HideChatPanel()
    {
        if (chatPanel != null)
        {
            isChatPanelVisible = false;
            chatPanel.SetActive(false);
            Debug.Log("Chat panel hidden");
        }
    }

    public bool IsChatPanelVisible()
    {
        return isChatPanelVisible;
    }
    
    #region Learning Progress UI Methods
    
    private void InitializeLearningProgressUI()
    {
        // 订阅学习进度事件
        if (LearningProgressManager.Instance != null)
        {
            LearningProgressManager.Instance.OnExpGained.AddListener(OnExpGained);
            LearningProgressManager.Instance.OnLevelUp.AddListener(OnLevelUp);
            LearningProgressManager.Instance.OnProfileUpdated.AddListener(UpdateProgressDisplay);
            
            // 初始化显示
            UpdateProgressDisplay(LearningProgressManager.Instance.userProfile);
            Debug.Log("? Learning Progress UI initialized");
        }
        
        // 设置升级面板关闭按钮
        if (levelUpCloseButton != null)
        {
            levelUpCloseButton.onClick.AddListener(CloseLevelUpPanel);
        }
        
        // 设置成就按钮
        if (achievementButton != null)
        {
            achievementButton.onClick.AddListener(ShowAchievements);
        }
        
        // 初始隐藏升级面板
        if (levelUpPanel != null)
        {
            levelUpPanel.SetActive(false);
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
            int expToNextLevel = profile.GetExpToNextLevel();
            int currentLevelExp = profile.totalExp;
            int nextLevelTotalExp = currentLevelExp + expToNextLevel;
            
            // 显示当前级别的经验进度
            if (profile.level == 1)
            {
                expText.text = $"{profile.totalExp}/{profile.GetExpRequiredForLevel(2)} EXP";
            }
            else
            {
                int currentLevelStartExp = profile.GetExpRequiredForLevel(profile.level);
                int nextLevelStartExp = profile.GetExpRequiredForLevel(profile.level + 1);
                int expInCurrentLevel = profile.totalExp - currentLevelStartExp;
                int expNeededForLevel = nextLevelStartExp - currentLevelStartExp;
                expText.text = $"{expInCurrentLevel}/{expNeededForLevel} EXP";
            }
        }
        
        // 更新经验值进度条
        if (expProgressBar != null)
        {
            float progress = profile.GetLevelProgress();
            targetExpProgress = progress;
            
            if (!isAnimatingExp)
            {
                expProgressBar.value = progress;
            }
        }
    }
    
    private void OnExpGained(int amount)
    {
        // 开始经验值动画
        isAnimatingExp = true;
        
        // 显示经验值获得效果
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
    
    private void ShowLevelUpNotification(int newLevel)
    {
        if (levelUpPanel != null && levelUpText != null)
        {
            UserProfile profile = LearningProgressManager.Instance.userProfile;
            
            levelUpText.text = $"*** LEVEL UP! ***\n\nYou are now Level {newLevel}\n({profile.GetLevelTitle()})";
            levelUpText.color = profile.GetLevelColor();
            
            levelUpPanel.SetActive(true);
            
            // 3秒后自动关闭（可选）
            Invoke(nameof(CloseLevelUpPanel), 3f);
        }
    }
    
    private void CloseLevelUpPanel()
    {
        if (levelUpPanel != null)
        {
            levelUpPanel.SetActive(false);
        }
    }
    
    // 显示成就解锁通知
    public void ShowAchievementUnlocked(Achievement achievement)
    {
        if (levelUpPanel != null && levelUpText != null)
        {
            levelUpText.text = $"*** ACHIEVEMENT UNLOCKED! ***\n\n{achievement.title}\n\n{achievement.description}\n\nReward: +{achievement.rewardExp} EXP";
            levelUpText.color = Color.yellow; // 成就通知用黄色
            
            levelUpPanel.SetActive(true);
            
            // 5秒后自动关闭
            Invoke(nameof(CloseLevelUpPanel), 5f);
            
            Debug.Log($"Achievement notification shown: {achievement.title}");
        }
    }
    
    // 显示成就列表（切换功能）
    public void ShowAchievements()
    {
        if (levelUpPanel != null && levelUpText != null)
        {
            // 如果面板已经显示，则关闭它
            if (levelUpPanel.activeInHierarchy)
            {
                levelUpPanel.SetActive(false);
                return;
            }
            
            // TODO: Fix compilation issue with AchievementManager reference
            // var achievementManager = FindObjectOfType<AchievementManager>();
            // if (achievementManager != null)
            // {
            //     string achievementText = achievementManager.GetAchievementDisplayText();
            //     levelUpText.text = achievementText;
            // }
            // else
            // {
                levelUpText.text = "*** ACHIEVEMENTS ***\n\nClick any message to unlock achievements!\n\nAVAILABLE GOALS:\n[LOCK] Ice Breaker - Send first message\n[LOCK] Chat Master - Have 10 conversations\n[LOCK] EXP Collector - Earn 100 experience\n[LOCK] Rising Star - Reach level 3\n\nStart chatting to unlock achievements!\n\nPROGRESS: 0/15 achievements unlocked";
            // }
            
            levelUpText.color = Color.white;
            levelUpPanel.SetActive(true);
        }
    }
    
    // 公共方法：手动刷新显示
    public void RefreshProgressDisplay()
    {
        if (LearningProgressManager.Instance != null)
        {
            UpdateProgressDisplay(LearningProgressManager.Instance.userProfile);
        }
    }
    
    #endregion
}