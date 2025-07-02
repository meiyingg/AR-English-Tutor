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
using System.Collections;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using Unity.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;

/// <summary>
/// Main UI Manager for AR English Learning App
/// Handles chat, progress, achievements, and all primary user interactions
/// </summary>
public class ChatTestUI : MonoBehaviour
{
    public static ChatTestUI Instance { get; private set; }

    [Header("UI References")]
    public TMP_InputField inputField;
    public Button sendButton;
    public Button imageButton; // New image upload button
    public Button recordButton; // New voice recording button
    public Button takePhotoButton; // 新增：拍照按钮
    public GameObject screenSpaceMessagePrefab; // The original chat bubble for screen space
    public GameObject worldSpaceMessagePrefab; // Your new ChatMessage_Realworld prefab
    public Transform chatContentPanel; // Legacy screen-space container

    [Header("Chat Panel Control")]
    public GameObject chatPanel; // Chat面板的根GameObject
    public Button toggleChatButton; // 控制显示/隐藏的按钮
    private bool isChatPanelVisible = false; // 面板显示状态 - 默认隐藏

    [Header("Speech Features")]
    public Color recordingColor = Color.red;
    public Color normalRecordButtonColor = Color.white;
    private bool isRecording = false;

    [Header("Chat Bubble Colors")]
    public Color userBubbleColor = new Color(0.0f, 0.5f, 1.0f); // 蓝色
    public Color tutorBubbleColor = new Color(0.8f, 0.8f, 0.8f); // 灰色

    [Header("Learning Progress UI")]
    public TextMeshProUGUI levelText;           // 显示等级：Level 5
    public TextMeshProUGUI titleText;           // 显示称号：Beginner
    public TextMeshProUGUI expText;             // 显示经验：120/300 EXP
    public Slider expProgressBar;               // 经验值进度条
    public GameObject systemNotificationPanel;   // 系统通知面板 - 用于显示升级、成就解锁等系统消息
    public TextMeshProUGUI systemNotificationText;         // 系统通知文本 - 显示升级、成就等消息内容
    public Button levelUpCloseButton;           // 关闭系统通知面板按钮
    public Button achievementButton;            // 成就按钮

    [Header("Progress Animation Settings")]
    public float expBarAnimationSpeed = 2f;     // 经验条动画速度
    public AnimationCurve expBarCurve = AnimationCurve.EaseInOut(0,0,1,1);
    
    private float targetExpProgress = 0f;
    private bool isAnimatingExp = false;

    private Transform worldSpaceChatContainer; // New world-space container

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        #if UNITY_EDITOR
        // For easier testing in the editor, automatically find and initialize a pre-placed tutor.
        FindAndInitializePrePlacedTutor();
        #endif

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
        
        // Connect record button for voice input
        if (recordButton != null)
        {
            recordButton.onClick.AddListener(OnRecordButtonClick);
            Debug.Log("? Record button connected");
        }
        else
        {
            Debug.LogWarning("?? Record button not assigned in Inspector. Please drag RecordButton to the 'Record Button' field in ChatTestUI component.");
        }
        
        // Connect take photo button
        if (takePhotoButton != null)
        {
            takePhotoButton.onClick.AddListener(OnTakePhotoButtonClick);
            Debug.Log("? Take Photo button connected");
        }
        else
        {
            Debug.LogWarning("?? Take Photo button not assigned in Inspector. Please drag TakePhotoButton to ChatTestUI component.");
        }
        
        // Subscribe to AudioManager events once in Start (not in StartRecording)
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.OnRecordingStateChanged += OnRecordingStateChanged;
            AudioManager.Instance.OnSpeechToTextResult += OnTranscriptionReceived;
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
        if (screenSpaceMessagePrefab != null)
        {
            Debug.Log("? Chat message prefab assigned");
        }
        else
        {
            Debug.LogError("? Chat message prefab not assigned in Inspector!");
        }
        
        // worldSpaceMessagePrefab is optional for now
        if (worldSpaceMessagePrefab != null)
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

    #if UNITY_EDITOR
    private void FindAndInitializePrePlacedTutor()
    {
        var arManager = FindObjectOfType<ARManager>();
        if (arManager == null) return;

        var existingTutor = GameObject.Find(arManager.tutorPrefab.name);
        if (existingTutor == null)
        {
            existingTutor = GameObject.Find(arManager.tutorPrefab.name + "(Clone)");
        }

        if (existingTutor != null)
        {
            Debug.Log("Found pre-placed tutor in Editor. Initializing for testing.");
            arManager.InitializeTutor(existingTutor);
        }
    }
    #endif

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
    
    private void OnRecordButtonClick()
    {
        if (AudioManager.Instance == null)
        {
            Debug.LogWarning("AudioManager not found! Please add AudioManager to the scene.");
            UpdateChatHistory("Speech feature unavailable. Please check AudioManager setup.", ChatManager.Sender.Tutor);
            return;
        }

        if (!isRecording)
        {
            StartRecording();
        }
        else
        {
            StopRecording();
        }
    }
    
    private void StartRecording()
    {
        Debug.Log("Starting voice recording...");
        isRecording = true;
        
        // Change button color to indicate recording
        Image recordButtonImage = recordButton.GetComponent<Image>();
        if (recordButtonImage != null)
        {
            recordButtonImage.color = recordingColor;
        }
        
        // Update button text
        TextMeshProUGUI buttonText = recordButton.GetComponentInChildren<TextMeshProUGUI>();
        if (buttonText != null)
        {
            buttonText.text = "STOP";
        }
        
        // Start recording via AudioManager
        AudioManager.Instance.StartRecording();
        
        // Disable other UI during recording
        SetUIInteractable(false);
        recordButton.interactable = true; // Keep record button active to stop recording
    }
    
    private void StopRecording()
    {
        Debug.Log("Stopping voice recording...");
        
        // Stop recording via AudioManager
        AudioManager.Instance.StopRecording();
        
        ResetRecordButton();
    }
    
    private void ResetRecordButton()
    {
        isRecording = false;
        
        // Reset button color
        Image recordButtonImage = recordButton.GetComponent<Image>();
        if (recordButtonImage != null)
        {
            recordButtonImage.color = normalRecordButtonColor;
        }
        
        // Reset button text
        TextMeshProUGUI buttonText = recordButton.GetComponentInChildren<TextMeshProUGUI>();
        if (buttonText != null)
        {
            buttonText.text = "?";
        }
        
        // Re-enable UI
        SetUIInteractable(true);
    }
    
    // Audio event handlers
    private void OnRecordingStateChanged(bool isRecording)
    {
        if (isRecording)
        {
            Debug.Log("Recording started successfully");
            // Don't show recording status messages in chat to avoid clutter
        }
        else
        {
            Debug.Log("Recording finished, processing...");
            // Don't show recording status messages in chat to avoid clutter
        }
    }
    
    private void OnTranscriptionReceived(string transcription)
    {
        Debug.Log($"Transcription received: {transcription}");
        
        // Put transcription in input field
        if (inputField != null)
        {
            inputField.text = transcription;
        }
        
        // Automatically send the transcribed message (don't show duplicate message)
        OnSendButtonClick();
        
        ResetRecordButton();
    }
    
    private void OnSpeechError(string error)
    {
        Debug.LogError($"Speech error: {error}");
        UpdateChatHistory($"Speech recognition error: {error}", ChatManager.Sender.Tutor);
        ResetRecordButton();
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
#elif UNITY_ANDROID || UNITY_IOS
        NativeGallery.RequestPermissionAsync((permission) => {
            if (permission == NativeGallery.Permission.Granted)
            {
                NativeGallery.GetImageFromGallery((path) => {
                    if (!string.IsNullOrEmpty(path))
                    {
                        Debug.Log($"Selected image for scene learning: {path}");
                        ProcessSceneRecognitionFile(path);
                    }
                    else
                    {
                        Debug.Log("No image selected for scene learning");
                        UpdateChatHistory("No image selected for scene learning", ChatManager.Sender.Tutor);
                    }
                }, "Select an image", "image/*");
            }
            else
            {
                UpdateChatHistory("Permission denied or cancelled.", ChatManager.Sender.Tutor);
            }
        }, NativeGallery.PermissionType.Read, NativeGallery.MediaType.Image);
#else
        Debug.Log("Scene learning would work on mobile device");
        UpdateChatHistory("Scene learning feature clicked (unsupported platform)", ChatManager.Sender.Tutor);
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
        if (recordButton != null)
            recordButton.interactable = interactable;
    }
    
    private void UpdateChatHistory(string message, ChatManager.Sender sender, bool isNotification = false)
    {
        // --- 1. Always create the message in the screen-space chat history panel ---
        if (chatContentPanel != null && screenSpaceMessagePrefab != null)
        {
            GameObject historyMessageRow = Instantiate(screenSpaceMessagePrefab);
            historyMessageRow.transform.SetParent(chatContentPanel, false);
            SetupChatMessage(historyMessageRow, message, sender);
        }

        // --- 2. If it's the Tutor's response in AR mode, also create a world-space bubble (not for notifications) ---
        bool isTutorInAR = sender == ChatManager.Sender.Tutor && !isNotification && worldSpaceChatContainer != null && worldSpaceMessagePrefab != null;
        if (isTutorInAR)
        {
            // 1. 先把所有旧气泡颜色改为白色
            foreach (Transform child in worldSpaceChatContainer)
            {
                var img = child.GetComponentInChildren<Image>();
                if (img != null)
                    img.color = Color.white;
            }

            // 2. 生成新气泡
            GameObject worldMessageRow = Instantiate(worldSpaceMessagePrefab);
            worldMessageRow.transform.SetParent(worldSpaceChatContainer, false);
            SetupChatMessage(worldMessageRow, message, sender);

            // 3. 强制新气泡为绿色
            var newImg = worldMessageRow.GetComponentInChildren<Image>();
            if (newImg != null)
                newImg.color = new Color(0.7f, 1f, 0.7f, 1f); // 浅绿色
        }

        // --- 3. Handle TTS for non-notification Tutor messages ---
        if (sender == ChatManager.Sender.Tutor && !isNotification)
        {
            PlayTTS(message, false);
        }
    }

    /// <summary>
    /// Helper method to configure a chat message UI object.
    /// </summary>
    private void SetupChatMessage(GameObject messageInstance, string message, ChatManager.Sender sender)
    {
        ChatMessageUI messageUI = messageInstance.GetComponent<ChatMessageUI>();
        if (messageUI != null)
        {
            messageUI.SetMessage(message, sender == ChatManager.Sender.User);
        }
        else
        {
            var tmp = messageInstance.GetComponentInChildren<TMPro.TMP_Text>();
            if (tmp != null) tmp.text = message;
        }

        Image bubbleImage = messageInstance.GetComponentInChildren<Image>();
        if (bubbleImage != null)
        {
            bool isWorldSpace = messageInstance.transform.parent == worldSpaceChatContainer;
            if (!isWorldSpace)
                bubbleImage.color = (sender == ChatManager.Sender.User) ? userBubbleColor : tutorBubbleColor;
            // 世界空间气泡颜色由UpdateChatHistory控制
        }

        if (messageInstance.transform.parent == worldSpaceChatContainer)
        {
            if (messageInstance.GetComponent<FadeAndDestroy>() == null)
                messageInstance.AddComponent<FadeAndDestroy>();
        }

        if(sender == ChatManager.Sender.Tutor)
        {
            AddTTSClickHandler(messageInstance, message);
        }
    }

    private void AddTTSClickHandler(GameObject messageObject, string text)
    {
        Button clickableArea = messageObject.GetComponent<Button>();
        if (clickableArea == null)
        {
            clickableArea = messageObject.AddComponent<Button>();
        }
        
        // Make the button transparent but clickable
        clickableArea.targetGraphic = messageObject.GetComponentInChildren<Image>();
        
        // Add click event for TTS
        clickableArea.onClick.AddListener(() => {
            PlayTTS(text);
        });
        
        // Optional: Add visual feedback
        var colors = clickableArea.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = new Color(1f, 1f, 1f, 0.8f);
        colors.pressedColor = new Color(1f, 1f, 1f, 0.6f);
        clickableArea.colors = colors;
    }
    
    private void PlayTTS(string text, bool isNotification = false)
    {
        if (AudioManager.Instance == null)
        {
            Debug.LogWarning("AudioManager not found! TTS not available.");
            return;
        }
        if (isNotification) return; // 通知类消息不播报
        AudioManager.Instance.StopSpeaking(); // 直接中断上一次TTS
        _ = AudioManager.Instance.SpeakText(text); // 播放新TTS
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
            levelUpCloseButton.onClick.AddListener(CloseSystemNotificationPanel);
        }
        
        // 设置成就按钮
        if (achievementButton != null)
        {
            achievementButton.onClick.AddListener(ShowAchievements);
        }
        
        // 初始隐藏升级面板
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
                expInCurrentLevel = Mathf.Max(0, expInCurrentLevel); // 防止负数
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
        if (systemNotificationPanel != null && systemNotificationText != null)
        {
            UserProfile profile = LearningProgressManager.Instance.userProfile;
            systemNotificationText.text = $"*** LEVEL UP! ***\n\nYou are now Level {newLevel}\n({profile.GetLevelTitle()})";
            systemNotificationText.color = profile.GetLevelColor();
            systemNotificationPanel.SetActive(true);
            // 3秒后自动关闭（可选）
            Invoke(nameof(CloseSystemNotificationPanel), 3f);
            // 聊天窗口也显示升级消息，但不TTS
            UpdateChatHistory($"LEVEL UP! You are now Level {newLevel} ({profile.GetLevelTitle()})", ChatManager.Sender.Tutor, true);
        }
    }
    
    private void CloseSystemNotificationPanel()
    {
        if (systemNotificationPanel != null)
        {
            systemNotificationPanel.SetActive(false);
        }
    }
    
    // 显示成就解锁通知
    public void ShowAchievementUnlocked(Achievement achievement)
    {
        if (systemNotificationPanel != null && systemNotificationText != null)
        {
            systemNotificationText.text = $"*** ACHIEVEMENT UNLOCKED! ***\n\n{achievement.title}\n\n{achievement.description}\n\nReward: +{achievement.rewardExp} EXP";
            systemNotificationText.color = Color.yellow;
            systemNotificationPanel.SetActive(true);
            Invoke(nameof(CloseSystemNotificationPanel), 5f);
            Debug.Log($"Achievement notification shown: {achievement.title}");
            // 聊天窗口也显示成就消息，但不TTS
            UpdateChatHistory($"ACHIEVEMENT UNLOCKED! {achievement.title}: {achievement.description}", ChatManager.Sender.Tutor, true);
        }
    }
    
    // 显示成就列表（切换功能）
    public void ShowAchievements()
    {
        if (systemNotificationPanel != null && systemNotificationText != null)
        {
            if (systemNotificationPanel.activeInHierarchy)
            {
                systemNotificationPanel.SetActive(false);
                return;
            }
            systemNotificationText.text = "*** ACHIEVEMENTS ***\n\nClick any message to unlock achievements!\n\nAVAILABLE GOALS:\n[LOCK] Ice Breaker - Send first message\n[LOCK] Chat Master - Have 10 conversations\n[LOCK] EXP Collector - Earn 100 experience\n[LOCK] Rising Star - Reach level 3\n\nStart chatting to unlock achievements!\n\nPROGRESS: 0/15 achievements unlocked";
            systemNotificationText.color = Color.white;
            systemNotificationPanel.SetActive(true);
            // 聊天窗口也显示成就列表，但不TTS
            UpdateChatHistory("Achievements panel opened.", ChatManager.Sender.Tutor, true);
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

    private void OnDestroy()
    {
        // Unsubscribe from events to prevent memory leaks
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.OnRecordingStateChanged -= OnRecordingStateChanged;
            AudioManager.Instance.OnSpeechToTextResult -= OnTranscriptionReceived;
        }

        if (ChatManager.Instance != null)
        {
            ChatManager.Instance.onNewMessage.RemoveListener(UpdateChatHistory);
        }
    }
    
    private void OnTakePhotoButtonClick()
    {
        StartCoroutine(CaptureARCameraImageAndSend());
    }

    private IEnumerator CaptureARCameraImageAndSend()
    {
        // 直接尝试获取AR摄像头画面，使用ARCameraManager实例方法
        XRCpuImage cpuImage;
        var arCameraManager = FindObjectOfType<ARCameraManager>();
        if (arCameraManager != null && arCameraManager.TryAcquireLatestCpuImage(out cpuImage))
        {
            var conversionParams = new XRCpuImage.ConversionParams
            {
                inputRect = new RectInt(0, 0, cpuImage.width, cpuImage.height),
                outputDimensions = new Vector2Int(cpuImage.width, cpuImage.height),
                outputFormat = TextureFormat.RGB24,
                transformation = XRCpuImage.Transformation.MirrorY
            };

            Texture2D texture = new Texture2D(cpuImage.width, cpuImage.height, TextureFormat.RGB24, false);
            NativeArray<byte> buffer = new NativeArray<byte>(cpuImage.GetConvertedDataSize(conversionParams), Allocator.Temp);
            cpuImage.Convert(conversionParams, buffer);
            texture.LoadRawTextureData(buffer);
            texture.Apply();
            buffer.Dispose();
            cpuImage.Dispose();

            string userText = inputField.text.Trim();
            inputField.text = "";

            SetUIInteractable(false);
            yield return ChatManager.Instance.SendSceneRecognitionRequest(System.Convert.ToBase64String(texture.EncodeToPNG()), texture, userText);
            SetUIInteractable(true);
        }
        else
        {
            Debug.LogError("Failed to acquire AR camera image!");
            UpdateChatHistory("Failed to acquire AR camera image", ChatManager.Sender.Tutor, true);
        }
        yield break;
    }

    /// <summary>
    /// Called by ARManager after the tutor is placed to set the anchor for chat bubbles.
    /// </summary>
    public void SetWorldSpaceChatContainer(Transform container)
    {
        worldSpaceChatContainer = container;
        Debug.Log("[ChatTestUI] SetWorldSpaceChatContainer called, container: " + (container != null ? container.name : "null"));
    }
}
