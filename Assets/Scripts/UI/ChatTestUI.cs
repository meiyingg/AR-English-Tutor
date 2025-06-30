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

/// <summary>
/// Main UI Manager for AR English Learning App
/// Handles chat, progress, achievements, and all primary user interactions
/// </summary>
public class ChatTestUI : MonoBehaviour
{
    public TMP_InputField inputField;
    public Button sendButton;
    public Button imageButton; // New image upload button
    public Button recordButton; // New voice recording button
    public Button takePhotoButton; // ���������հ�ť
    public GameObject chatMessagePrefab; // ���ǽ�ʵ��������ϢԤ����
    public GameObject imageMessagePrefab; // New prefab for image messages
    public Transform chatContentPanel; // ScrollView��Content����

    [Header("Chat Panel Control")]
    public GameObject chatPanel; // Chat���ĸ�GameObject
    public Button toggleChatButton; // ������ʾ/���صİ�ť
    private bool isChatPanelVisible = true; // �����ʾ״̬

    [Header("Speech Features")]
    public Color recordingColor = Color.red;
    public Color normalRecordButtonColor = Color.white;
    private bool isRecording = false;

    [Header("Chat Bubble Colors")]
    public Color userBubbleColor = new Color(0.0f, 0.5f, 1.0f); // ��ɫ
    public Color tutorBubbleColor = new Color(0.8f, 0.8f, 0.8f); // ��ɫ

    [Header("Learning Progress UI")]
    public TextMeshProUGUI levelText;           // ��ʾ�ȼ���Level 5
    public TextMeshProUGUI titleText;           // ��ʾ�ƺţ�Beginner
    public TextMeshProUGUI expText;             // ��ʾ���飺120/300 EXP
    public Slider expProgressBar;               // ����ֵ������
    public GameObject systemNotificationPanel;   // ϵͳ֪ͨ��� - ������ʾ�������ɾͽ�����ϵͳ��Ϣ
    public TextMeshProUGUI systemNotificationText;         // ϵͳ֪ͨ�ı� - ��ʾ�������ɾ͵���Ϣ����
    public Button levelUpCloseButton;           // �ر�ϵͳ֪ͨ��尴ť
    public Button achievementButton;            // �ɾͰ�ť

    [Header("Progress Animation Settings")]
    public float expBarAnimationSpeed = 2f;     // �����������ٶ�
    public AnimationCurve expBarCurve = AnimationCurve.EaseInOut(0,0,1,1);
    
    private float targetExpProgress = 0f;
    private bool isAnimatingExp = false;

    public ARCameraManager arCameraManager; // Drag ARCameraManager from AR Camera here in Inspector

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
            // ���������Ͱ�ť����ֹ�ڵȴ�AI��Ӧʱ�ظ�����
            SetUIInteractable(false);

            await ChatManager.Instance.SendMessage(message);

            inputField.text = "";
            // ���¼��������Ͱ�ť
            SetUIInteractable(true);
            inputField.ActivateInputField(); // ���¾۽��������
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
        GameObject messageRow = Instantiate(chatMessagePrefab);
        messageRow.transform.SetParent(chatContentPanel, false);
        ChatMessageUI messageUI = messageRow.GetComponent<ChatMessageUI>();
        if (messageUI != null)
        {
            messageUI.SetMessage(message, sender == ChatManager.Sender.User);
        }
        Image bubbleImage = messageRow.GetComponentInChildren<Image>();
        if (bubbleImage != null)
        {
            bubbleImage.color = (sender == ChatManager.Sender.User) ? userBubbleColor : tutorBubbleColor;
        }
        // ֻ��AI��ͨ�Ի����Զ�TTS
        if (sender == ChatManager.Sender.Tutor && !isNotification)
        {
            AddTTSClickHandler(messageRow, message);
            PlayTTS(message, false);
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
        if (isNotification) return; // ֪ͨ����Ϣ������
        AudioManager.Instance.StopSpeaking(); // ֱ���ж���һ��TTS
        _ = AudioManager.Instance.SpeakText(text); // ������TTS
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
        // ����ѧϰ�����¼�
        if (LearningProgressManager.Instance != null)
        {
            LearningProgressManager.Instance.OnExpGained.AddListener(OnExpGained);
            LearningProgressManager.Instance.OnLevelUp.AddListener(OnLevelUp);
            LearningProgressManager.Instance.OnProfileUpdated.AddListener(UpdateProgressDisplay);
            
            // ��ʼ����ʾ
            UpdateProgressDisplay(LearningProgressManager.Instance.userProfile);
            Debug.Log("? Learning Progress UI initialized");
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
        
        // ��ʼ�����������
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
            int expToNextLevel = profile.GetExpToNextLevel();
            int currentLevelExp = profile.totalExp;
            int nextLevelTotalExp = currentLevelExp + expToNextLevel;
            
            // ��ʾ��ǰ����ľ������
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
                expInCurrentLevel = Mathf.Max(0, expInCurrentLevel); // ��ֹ����
                expText.text = $"{expInCurrentLevel}/{expNeededForLevel} EXP";
            }
        }
        
        // ���¾���ֵ������
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
        // ��ʼ����ֵ����
        isAnimatingExp = true;
        
        // ��ʾ����ֵ���Ч��
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
    
    private void ShowLevelUpNotification(int newLevel)
    {
        if (systemNotificationPanel != null && systemNotificationText != null)
        {
            UserProfile profile = LearningProgressManager.Instance.userProfile;
            systemNotificationText.text = $"*** LEVEL UP! ***\n\nYou are now Level {newLevel}\n({profile.GetLevelTitle()})";
            systemNotificationText.color = profile.GetLevelColor();
            systemNotificationPanel.SetActive(true);
            // 3����Զ��رգ���ѡ��
            Invoke(nameof(CloseSystemNotificationPanel), 3f);
            // ���촰��Ҳ��ʾ������Ϣ������TTS
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
    
    // ��ʾ�ɾͽ���֪ͨ
    public void ShowAchievementUnlocked(Achievement achievement)
    {
        if (systemNotificationPanel != null && systemNotificationText != null)
        {
            systemNotificationText.text = $"*** ACHIEVEMENT UNLOCKED! ***\n\n{achievement.title}\n\n{achievement.description}\n\nReward: +{achievement.rewardExp} EXP";
            systemNotificationText.color = Color.yellow;
            systemNotificationPanel.SetActive(true);
            Invoke(nameof(CloseSystemNotificationPanel), 5f);
            Debug.Log($"Achievement notification shown: {achievement.title}");
            // ���촰��Ҳ��ʾ�ɾ���Ϣ������TTS
            UpdateChatHistory($"ACHIEVEMENT UNLOCKED! {achievement.title}: {achievement.description}", ChatManager.Sender.Tutor, true);
        }
    }
    
    // ��ʾ�ɾ��б��л����ܣ�
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
            // ���촰��Ҳ��ʾ�ɾ��б�����TTS
            UpdateChatHistory("Achievements panel opened.", ChatManager.Sender.Tutor, true);
        }
    }
    
    // �����������ֶ�ˢ����ʾ
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
        if (arCameraManager == null)
        {
            Debug.LogError("ARCameraManager is not assigned!");
            UpdateChatHistory("AR camera is not configured", ChatManager.Sender.Tutor);
            yield break;
        }

        XRCpuImage cpuImage;
        if (arCameraManager.TryAcquireLatestCpuImage(out cpuImage))
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
}
