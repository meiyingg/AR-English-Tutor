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
using System.Collections.Generic;
using System.Text; // 添加StringBuilder命名空间
using System.Linq; // 添加LINQ命名空间
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
    public Button takePhotoButton; // New: take photo button
    public Button agentButton; // New: agent button for voice recording with AR scene awareness
    public GameObject screenSpaceMessagePrefab; // The original chat bubble for screen space
    public GameObject worldSpaceMessagePrefab; // Your new ChatMessage_Realworld prefab
    public Transform chatContentPanel; // Legacy screen-space container
    
    [Header("Scene Awareness")]
    [Tooltip("Whether to auto-initialize the background scene monitoring")]
    public bool enableSceneAwareness = true;
    [Tooltip("Whether to capture a scene when starting voice recording via agent button")]
    public bool captureSceneOnAgentRecord = true;

    [Header("Chat Panel Control")]
    public GameObject chatPanel; // Chat panel root GameObject
    public Button toggleChatButton; // Button to control show/hide
    private bool isChatPanelVisible = false; // Panel visibility state - hidden by default

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
        
        // 配置所有文本组件以支持富文本和表情符号
        ConfigureTextComponents();
        
        // Initialize background scene monitoring if enabled
        if (enableSceneAwareness && BackgroundSceneMonitor.Instance != null)
        {
            BackgroundSceneMonitor.Instance.StartMonitoring();
            Debug.Log("? Scene awareness active - AI can now 'see' the environment");
        }
        
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
        
        // Connect agent button (smart voice assistant with AR awareness)
        if (agentButton != null)
        {
            agentButton.onClick.AddListener(OnAgentButtonClick);
            Debug.Log("? Agent button connected");
        }
        else
        {
            Debug.LogWarning("?? Agent button not assigned in Inspector. Please drag AgentButton to the AgentButton field in ChatTestUI component.");
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
        
        // 自动开始场景监控（如果启用）
        if (enableSceneAwareness)
        {
            StartBackgroundSceneMonitoring();
        }
        
        // Configure TextMeshPro components for rich text and emoji support
        ConfigureTextComponents();
        
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
    
    private async void OnTranscriptionReceived(string transcription)
    {
        Debug.Log($"Transcription received: {transcription}");
        
        // Put transcription in input field
        if (inputField != null)
        {
            inputField.text = transcription;
        }
        
        // Get the latest scene context (if available and AgentButton was used)
        bool useSceneContext = false;
        BackgroundSceneMonitor.SceneMemory latestScene = null;
        
        if (agentButton?.gameObject.activeInHierarchy == true && 
            captureSceneOnAgentRecord && 
            BackgroundSceneMonitor.Instance != null)
        {
            latestScene = BackgroundSceneMonitor.Instance.GetLatestScene();
            useSceneContext = (latestScene != null && !string.IsNullOrEmpty(latestScene.base64Image));
        }
        
        // Disable UI during processing
        SetUIInteractable(false);
        
        // Use scene-aware message or regular message
        if (useSceneContext && ChatManager.Instance != null)
        {
            // Send with scene context
            await ChatManager.Instance.SendSceneRecognitionRequest(
                latestScene.base64Image,
                latestScene.texture, 
                transcription);
        }
        else
        {
            // Regular text message
            if (ChatManager.Instance != null)
            {
                await ChatManager.Instance.SendMessage(transcription);
            }
        }
        
        // Clear input field
        if (inputField != null)
        {
            inputField.text = "";
        }
        
        // Reset buttons based on which one was active
        if (isRecording)
        {
            if (agentButton?.gameObject.activeInHierarchy == true)
            {
                ResetAgentButton();
            }
            else
            {
                ResetRecordButton();
            }
        }
        
        // Re-enable UI
        SetUIInteractable(true);
        if (inputField != null)
        {
            inputField.ActivateInputField();
        }
    }
    
    private void OnSpeechError(string error)
    {
        Debug.LogError($"Speech error: {error}");
        UpdateChatHistory($"Speech recognition error: {error}", ChatManager.Sender.Tutor);
        
        // Reset buttons based on which one was active
        if (isRecording)
        {
            if (agentButton?.gameObject.activeInHierarchy == true)
            {
                ResetAgentButton();
            }
            else
            {
                ResetRecordButton();
            }
        }
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
        if (agentButton != null)
            agentButton.interactable = interactable;
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
        
        // Update level display
        if (levelText != null)
        {
            levelText.text = $"Level {profile.level}";
            levelText.color = profile.GetLevelColor();
        }
        
        // Update title display
        if (titleText != null)
        {
            titleText.text = profile.GetLevelTitle();
            titleText.color = profile.GetLevelColor();
        }
        
        // Update EXP display using the corrected logic
        if (expText != null)
        {
            int currentExpInLevel = profile.GetCurrentExpInLevel();
            int expForLevel = profile.GetExpForCurrentLevel();
            expText.text = $"{currentExpInLevel}/{expForLevel} EXP";
        }
        
        // Update EXP progress bar
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
            systemNotificationText.richText = true;
            systemNotificationText.parseCtrlCharacters = true;
            
            UserProfile profile = LearningProgressManager.Instance.userProfile;
            Color levelColor = profile.GetLevelColor();
            string colorHex = ColorUtility.ToHtmlStringRGB(levelColor);
            
            // 重新设计的升级通知，更简洁大方
            StringBuilder levelUpText = new StringBuilder();
            levelUpText.AppendLine("<size=30><color=#FFD700><b>LEVEL UP!</b></color></size>");
            
            // 创建一个简单的分隔线
            levelUpText.AppendLine("<size=26>-----------</size>");
            
            // 突出显示新等级和头衔
            levelUpText.AppendLine($"<size=34><color=#{colorHex}><b>{newLevel}</b></color></size>");
            levelUpText.AppendLine($"<size=26><i>\"{profile.GetLevelTitle()}\"</i></size>");
            
            // 添加一些鼓励性的消息
            string[] encouragements = new string[] {
                "Great progress! Keep going!",
                "You're getting better every day!",
                "Keep up the good work!",
                "Your skills are growing!",
                "Well done! Continue learning!"
            };
            
            // 随机选择一条鼓励消息
            string encouragement = encouragements[UnityEngine.Random.Range(0, encouragements.Length)];
            levelUpText.AppendLine($"<size=22><color=#E0E0E0>{encouragement}</color></size>");
            
            systemNotificationText.text = levelUpText.ToString();
            systemNotificationText.color = Color.white;
            systemNotificationPanel.SetActive(true);
            
            // 4秒后自动关闭
            Invoke(nameof(CloseSystemNotificationPanel), 4f);
            
            // 聊天窗口显示简洁的升级消息
            UpdateChatHistory($"Level Up! You are now Level {newLevel}: {profile.GetLevelTitle()}", ChatManager.Sender.Tutor, true);
        }
    }
    
    private void CloseSystemNotificationPanel()
    {
        if (systemNotificationPanel != null)
        {
            systemNotificationPanel.SetActive(false);
        }
    }
    
    // 重新设计的成就展示界面，更简洁美观
    public void ShowAchievements()
    {
        if (systemNotificationPanel != null && systemNotificationText != null)
        {
            if (systemNotificationPanel.activeInHierarchy)
            {
                systemNotificationPanel.SetActive(false);
                return;
            }

            systemNotificationText.richText = true;
            systemNotificationText.parseCtrlCharacters = true;

            StringBuilder achievementText = new StringBuilder();
            achievementText.AppendLine("<size=30><color=#FFD700><b>= ACHIEVEMENTS =</b></color></size>");
            
            int total = 0, unlocked = 0;
            if (AchievementManager.Instance != null && AchievementManager.Instance.achievements != null)
            {
                var achievements = AchievementManager.Instance.achievements;
                total = achievements.Count;
                unlocked = achievements.Count(a => a.isUnlocked);
                
                // 顶部显示解锁进度
                achievementText.AppendLine($"<size=20><color=#E0E0E0>Unlocked: <color=#32CD32>{unlocked}</color> / <color=#FFD700>{total}</color></color></size>");
                achievementText.AppendLine("<size=22>---------------------</size>");
                
                // 按类型分组
                foreach (AchievementType type in System.Enum.GetValues(typeof(AchievementType)))
                {
                    var typeAchievements = achievements.Where(a => a.type == type).ToList();
                    if (typeAchievements.Count > 0)
                    {
                        // 添加类别标题
                        achievementText.AppendLine($"<size=24><color=#00BFFF><b>[ {type} ]</b></color></size>");
                        
                        // 添加该类型的成就，使用表格布局
                        foreach (var a in typeAchievements)
                        {
                            string status = a.isUnlocked 
                                ? "<color=#32CD32><b>[?]</b></color>" 
                                : "<color=#A0A0A0>[·]</color>";
                                
                            string titleColor = a.isUnlocked ? "#FFFFFF" : "#A0A0A0";
                            
                            // 成就标题行（带状态和经验值）
                            achievementText.AppendLine($"{status} <color={titleColor}><b>{a.title}</b></color> <color=#00FF00>+{a.rewardExp}xp</color>");
                            
                            // 只为已解锁的成就显示描述
                            if (a.isUnlocked)
                            {
                                achievementText.AppendLine($"    <size=18><i>{a.description}</i></size>");
                            }
                        }
                        
                        achievementText.AppendLine();
                    }
                }
                
                // 底部显示总结信息
                if (unlocked == total && total > 0)
                {
                    achievementText.AppendLine("<color=#FFD700><b>All achievements unlocked!</b></color>");
                }
                else if (unlocked == 0)
                {
                    achievementText.AppendLine("<i>Start chatting to unlock achievements</i>");
                }
            }
            else
            {
                achievementText.AppendLine("<i>Achievement system is initializing...</i>");
            }
            
            systemNotificationText.text = achievementText.ToString();
            systemNotificationText.color = Color.white;
            systemNotificationPanel.SetActive(true);
            UpdateChatHistory("Achievements panel opened.", ChatManager.Sender.Tutor, true);
        }
    }

    // 重新设计的成就解锁通知，更简洁清晰
    public void ShowAchievementUnlocked(Achievement achievement)
    {
        if (systemNotificationPanel != null && systemNotificationText != null)
        {
            systemNotificationText.richText = true;
            systemNotificationText.parseCtrlCharacters = true;
            
            // 根据成就类型选择颜色
            string typeColor;
            switch (achievement.type)
            {
                case AchievementType.Conversation: typeColor = "#6495ED"; break; // 蓝色
                case AchievementType.Learning: typeColor = "#9370DB"; break;     // 紫色
                case AchievementType.Experience: typeColor = "#FFD700"; break;   // 金色
                case AchievementType.Level: typeColor = "#20B2AA"; break;        // 青色
                case AchievementType.Special: typeColor = "#FF6347"; break;      // 红色
                default: typeColor = "#FFD700"; break;                           // 默认金色
            }
            
            StringBuilder notificationText = new StringBuilder();
            
            // 简洁大标题
            notificationText.AppendLine("<size=26><color=#FFD700><b>ACHIEVEMENT</b></color></size>");
            
            // 突出显示成就标题，使用成就类型的颜色
            notificationText.AppendLine($"<size=30><color={typeColor}><b>{achievement.title}</b></color></size>");
            
            // 成就类型和描述
            notificationText.AppendLine($"<size=18><i><color=#C0C0C0>{achievement.type}</color></i></size>");
            notificationText.AppendLine($"<size=22>{achievement.description}</size>");
            
            // 醒目显示奖励
            notificationText.AppendLine($"<size=24><color=#00FF00><b>+{achievement.rewardExp} EXP</b></color></size>");
            
            systemNotificationText.text = notificationText.ToString();
            systemNotificationText.color = Color.white;
            systemNotificationPanel.SetActive(true);
            Invoke(nameof(CloseSystemNotificationPanel), 4f); // 缩短显示时间为4秒
            
            Debug.Log($"Achievement notification shown: {achievement.title}");
            
            // 聊天窗口消息也简化
            UpdateChatHistory($"Achievement: {achievement.title} (+{achievement.rewardExp} EXP)", ChatManager.Sender.Tutor, true);
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
        
        // Stop background scene monitoring if we started it
        if (enableSceneAwareness && BackgroundSceneMonitor.Instance != null)
        {
            BackgroundSceneMonitor.Instance.StopMonitoring();
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

    private void OnAgentButtonClick()
    {
        if (AudioManager.Instance == null)
        {
            Debug.LogWarning("AudioManager not found! Please add AudioManager to the scene.");
            UpdateChatHistory("Speech feature unavailable. Please check AudioManager setup.", ChatManager.Sender.Tutor);
            return;
        }

        if (!isRecording)
        {
            StartAgentRecording();
        }
        else
        {
            StopAgentRecording();
        }
    }
    
    private void StartAgentRecording()
    {
        Debug.Log("Starting AI agent voice recording...");
        isRecording = true;
        
        // Change button color to indicate recording
        Image agentButtonImage = agentButton?.GetComponent<Image>();
        if (agentButtonImage != null)
        {
            agentButtonImage.color = recordingColor;
        }
        
        // Capture current scene if enabled (gives AI context of what user is looking at)
        if (captureSceneOnAgentRecord && BackgroundSceneMonitor.Instance != null)
        {
            BackgroundSceneMonitor.Instance.CaptureScene();
            Debug.Log("Scene captured for AI context");
        }
        
        // Start recording via AudioManager
        AudioManager.Instance.StartRecording();
        
        // Disable other UI during recording
        SetUIInteractable(false);
        if (agentButton != null)
        {
            agentButton.interactable = true; // Keep agent button active to stop recording
        }
    }
    
    private void StopAgentRecording()
    {
        Debug.Log("Stopping AI agent voice recording...");
        
        // Stop recording via AudioManager
        AudioManager.Instance.StopRecording();
        
        ResetAgentButton();
    }
    
    private void ResetAgentButton()
    {
        isRecording = false;
        
        // Reset button color
        Image agentButtonImage = agentButton?.GetComponent<Image>();
        if (agentButtonImage != null)
        {
            agentButtonImage.color = normalRecordButtonColor;
        }
        
        // Re-enable UI
        SetUIInteractable(true);
    }

    private void StartBackgroundSceneMonitoring()
    {
        Debug.Log("Starting background scene monitoring...");
        // Here you would typically start any background services or monitoring needed
        // For example, starting a coroutine that checks the scene at intervals
        StartCoroutine(BackgroundSceneMonitoringCoroutine());
    }

    private IEnumerator BackgroundSceneMonitoringCoroutine()
    {
        while (true)
        {
            // Check if the AR session is running
            if (ARSession.state == ARSessionState.SessionTracking)
            {
                // Perform scene monitoring tasks
                Debug.Log("Monitoring scene...");
                
                // Example: You could check for specific objects in the scene, 
                // or monitor the camera's position/rotation for adjustments
            }
            
            // Wait for a short duration before the next check
            yield return new WaitForSeconds(5f);
        }
    }

    /// <summary>
    /// 配置所有TextMeshPro组件以支持富文本和表情符号
    /// </summary>
    private void ConfigureTextComponents()
    {
        // 确保系统通知文本支持富文本和表情符号
        if (systemNotificationText != null)
        {
            systemNotificationText.richText = true;
            systemNotificationText.parseCtrlCharacters = true;
            Debug.Log("System notification text configured for rich text and emoji support");
        }
        
        // 其他可能需要配置的TextMeshPro组件
        if (levelText != null) levelText.richText = true;
        if (expText != null) expText.richText = true;
        
        // 查找画布中的其他TextMeshPro组件并配置它们
        TextMeshProUGUI[] allTexts = FindObjectsOfType<TextMeshProUGUI>();
        foreach (var text in allTexts)
        {
            if (text != null)
            {
                text.richText = true;
                text.parseCtrlCharacters = true;
            }
        }
        
        Debug.Log($"Configured {(allTexts != null ? allTexts.Length : 0)} TextMeshPro components for rich text and emoji support");
    }
}
