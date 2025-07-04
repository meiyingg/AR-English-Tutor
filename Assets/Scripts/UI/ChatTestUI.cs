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
using System.Text; // StringBuilder�����ռ�
using System.Linq; // LINQ�����ռ�
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using Unity.Collections;
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
    
    [Header("Learning Mode Buttons")]
    public Button sceneModeButton; // ����ѧϰģʽ��ť
    public Button wordModeButton; // ����ѧϰģʽ��ť
    public Button normalModeButton; // ��ͨ����ģʽ��ť
    public Button reviewButton; // ��ϰģʽ��ť
    
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

    private Transform worldSpaceChatContainer; // New world-space container

    [Header("Scene Interaction Tracking")]
    private float lastSceneInteractionTime = 0f; // ���һ�γ���������ʱ��
    private const float SCENE_INTERACTION_COOLDOWN = 60f; // 60���ڲ��ظ������������ӳ�ʱ�䣩
    private int conversationTurnsInCurrentScene = 0; // ��ǰ�����Ի�����
    private const int MAX_CONVERSATION_TURNS_BEFORE_RESET = 8; // ����8�ֶԻ������ó���
    private int vocabularyPracticeRounds = 0; // ��ǰ�ʻ���ϰ����
    private const int MAX_VOCABULARY_ROUNDS_BEFORE_NEW_WORDS = 6; // ����6�ֺ�������ѧϰ�´ʻ�
    
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
        
        // ���������ı������֧�ָ��ı��ͱ������
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
        
        // Connect learning mode buttons
        if (sceneModeButton != null)
        {
            sceneModeButton.onClick.AddListener(OnSceneModeButtonClick);
            Debug.Log("? Scene Mode button connected");
        }
        else
        {
            Debug.LogWarning("?? Scene Mode button not assigned in Inspector. Please drag Scene Mode Button to ChatTestUI component.");
        }
        
        if (wordModeButton != null)
        {
            wordModeButton.onClick.AddListener(OnWordModeButtonClick);
            Debug.Log("? Word Mode button connected");
        }
        else
        {
            Debug.LogWarning("?? Word Mode button not assigned in Inspector. Please drag Word Mode Button to ChatTestUI component.");
        }
        
        if (normalModeButton != null)
        {
            normalModeButton.onClick.AddListener(OnNormalModeButtonClick);
            Debug.Log("? Normal Mode button connected");
        }
        else
        {
            Debug.LogWarning("?? Normal Mode button not assigned in Inspector. Please drag Normal Mode Button to ChatTestUI component.");
        }
        
        if (reviewButton != null)
        {
            reviewButton.onClick.AddListener(OnReviewButtonClick);
            Debug.Log("? Review button connected");
        }
        else
        {
            Debug.LogWarning("?? Review button not assigned in Inspector. Please drag Review Button to ChatTestUI component.");
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
        
        // ��ʼ��AgentButton��ʾ״̬����ChatPanel���⣩
        if (agentButton != null)
        {
            agentButton.gameObject.SetActive(!isChatPanelVisible); // ChatPanel����ʱ��ʾAgentButton
            Debug.Log($"? AgentButton initialized - Visible: {!isChatPanelVisible}");
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
        
        // �Զ���ʼ������أ�������ã�
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
        
        // Get current learning mode to determine behavior
        LearningModeManager.LearningMode currentMode = LearningModeManager.LearningMode.Normal;
        if (LearningModeManager.Instance != null)
        {
            currentMode = LearningModeManager.Instance.currentMode;
        }
        
        // AgentButton should primarily send text, not force scene analysis
        // Only use scene context in specific conditions based on mode and user intent
        bool useSceneContext = false;
        BackgroundSceneMonitor.SceneMemory latestScene = null;
        
        // Only consider scene context if BackgroundSceneMonitor is available
        if (BackgroundSceneMonitor.Instance != null)
        {
            latestScene = BackgroundSceneMonitor.Instance.GetLatestScene();
            
            // Check if we have valid scene data
            bool hasValidScene = (latestScene != null && !string.IsNullOrEmpty(latestScene.base64Image));
            
            if (hasValidScene)
            {
                string lowerTranscription = transcription.ToLower();
                
                // Determine scene context usage based on mode and user intent
                if (currentMode == LearningModeManager.LearningMode.Normal)
                {
                    // Normal mode: Only use scene context if user explicitly asks about the scene
                    bool explicitlyAsksAboutScene = lowerTranscription.Contains("what") && (lowerTranscription.Contains("see") || lowerTranscription.Contains("this")) ||
                                                   lowerTranscription.Contains("describe") ||
                                                   lowerTranscription.Contains("look at") ||
                                                   lowerTranscription.Contains("analyze") ||
                                                   lowerTranscription.Contains("tell me about this") ||
                                                   lowerTranscription.Contains("what's in the") ||
                                                   lowerTranscription.Contains("picture") ||
                                                   lowerTranscription.Contains("image");
                    
                    useSceneContext = explicitlyAsksAboutScene;
                }
                else if (currentMode == LearningModeManager.LearningMode.Scene)
                {
                    // Scene mode: Use intelligent context detection
                    // (keeping the existing smart logic for Scene mode)
                    bool explicitlyAskingAboutScene = 
                        (lowerTranscription.Contains("what") && (lowerTranscription.Contains("see") || lowerTranscription.Contains("this"))) ||
                        lowerTranscription.Contains("describe") ||
                        lowerTranscription.Contains("look at") ||
                        lowerTranscription.Contains("analyze") ||
                        lowerTranscription.Contains("tell me about this") ||
                        lowerTranscription.Contains("what's in the") ||
                        lowerTranscription.Contains("what is in") ||
                        lowerTranscription.Contains("show me") ||
                        lowerTranscription.Contains("new scene") ||
                        lowerTranscription.Contains("different scene") ||
                        lowerTranscription.Contains("change scene") ||
                        lowerTranscription.Contains("another scene") ||
                        lowerTranscription.Contains("restart") ||
                        lowerTranscription.Contains("start over") ||
                        lowerTranscription.Contains("let's talk about") ||
                        lowerTranscription.Contains("what about") ||
                        lowerTranscription.Contains("how about");
                    
                    bool indicatesContinuation = 
                        lowerTranscription.Equals("yes") || lowerTranscription.Equals("no") ||
                        lowerTranscription.Equals("ok") || lowerTranscription.Equals("okay") ||
                        lowerTranscription.Equals("sure") || lowerTranscription.Equals("good") ||
                        lowerTranscription.StartsWith("i ") || lowerTranscription.StartsWith("my ") ||
                        lowerTranscription.StartsWith("i'm ") || lowerTranscription.StartsWith("i am ") ||
                        lowerTranscription.Contains("i like") || lowerTranscription.Contains("i don't like") ||
                        lowerTranscription.Contains("i think") || lowerTranscription.Contains("i believe") ||
                        lowerTranscription.Contains("because") || lowerTranscription.Contains("so") ||
                        lowerTranscription.Contains("what do you") || lowerTranscription.Contains("do you") ||
                        lowerTranscription.Contains("can you") || lowerTranscription.Contains("will you");
                    
                    bool isFirstSceneInteraction = !HasRecentSceneInteraction();
                    bool tooManyConversationTurns = conversationTurnsInCurrentScene >= MAX_CONVERSATION_TURNS_BEFORE_RESET;
                    
                    useSceneContext = isFirstSceneInteraction ||
                                     explicitlyAskingAboutScene ||
                                     (tooManyConversationTurns && !indicatesContinuation);
                }
                else if (currentMode == LearningModeManager.LearningMode.Word)
                {
                    // Word mode: Use intelligent vocabulary detection
                    bool explicitlyAskingAboutWords = 
                        lowerTranscription.Contains("what") && (lowerTranscription.Contains("word") || lowerTranscription.Contains("vocabulary")) ||
                        lowerTranscription.Contains("teach me") && (lowerTranscription.Contains("word") || lowerTranscription.Contains("vocabulary")) ||
                        lowerTranscription.Contains("what is this") ||
                        lowerTranscription.Contains("what are these") ||
                        lowerTranscription.Contains("what's this called") ||
                        lowerTranscription.Contains("how do you say") ||
                        lowerTranscription.Contains("new words") ||
                        lowerTranscription.Contains("more words") ||
                        lowerTranscription.Contains("other words") ||
                        lowerTranscription.Contains("different words") ||
                        lowerTranscription.Contains("another word") ||
                        lowerTranscription.Contains("more vocabulary") ||
                        lowerTranscription.Contains("learn vocabulary");
                    
                    bool indicatesVocabularyPractice = 
                        lowerTranscription.Contains("i use") || lowerTranscription.Contains("i have") ||
                        lowerTranscription.Contains("my sentence is") || lowerTranscription.Contains("here is my sentence") ||
                        lowerTranscription.Contains("the word") || lowerTranscription.Contains("this word") ||
                        lowerTranscription.Contains("that word") || lowerTranscription.Contains("these words") ||
                        lowerTranscription.Contains("how do i use") || lowerTranscription.Contains("can i say") ||
                        lowerTranscription.Contains("is this correct") || lowerTranscription.Contains("did i use") ||
                        lowerTranscription.Equals("yes") || lowerTranscription.Equals("no") ||
                        lowerTranscription.StartsWith("i ") || lowerTranscription.StartsWith("my ") ||
                        lowerTranscription.Contains("because") || lowerTranscription.Contains("so");
                    
                    bool isFirstWordInteraction = !HasRecentSceneInteraction();
                    bool tooManyPracticeRounds = vocabularyPracticeRounds >= MAX_VOCABULARY_ROUNDS_BEFORE_NEW_WORDS;
                    
                    useSceneContext = isFirstWordInteraction ||
                                     explicitlyAskingAboutWords ||
                                     (tooManyPracticeRounds && !indicatesVocabularyPractice);
                }
            }
        }
        
        // Disable UI during processing
        SetUIInteractable(false);
        
        // Use scene-aware message or regular message
        if (useSceneContext && ChatManager.Instance != null)
        {
            // Determine if this is first scene interaction
            bool isFirstSceneInteraction = !HasRecentSceneInteraction();
            
            // Record that we're doing a scene interaction
            RecordSceneInteraction();
            
            // Reset conversation/practice counters for new vocabulary/scene analysis
            if (currentMode == LearningModeManager.LearningMode.Scene)
            {
                conversationTurnsInCurrentScene = 0;
            }
            else if (currentMode == LearningModeManager.LearningMode.Word)
            {
                vocabularyPracticeRounds = 0;
            }
            
            // Send with scene context
            await ChatManager.Instance.SendSceneRecognitionRequest(
                latestScene.base64Image,
                latestScene.texture, 
                transcription,
                isFirstSceneInteraction);
        }
        else
        {
            // Regular text message - increment conversation/practice counters
            if (currentMode == LearningModeManager.LearningMode.Scene)
            {
                conversationTurnsInCurrentScene++;
                Debug.Log($"[ChatTestUI] Scene conversation turn: {conversationTurnsInCurrentScene}");
            }
            else if (currentMode == LearningModeManager.LearningMode.Word)
            {
                vocabularyPracticeRounds++;
                Debug.Log($"[ChatTestUI] Vocabulary practice round: {vocabularyPracticeRounds}");
            }
            
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
                await ChatManager.Instance.SendSceneRecognitionRequest(base64, texture, userText, true); // Always first interaction for image upload
                
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
            // 1. �Ȱ����о�������ɫ��Ϊ��ɫ
            foreach (Transform child in worldSpaceChatContainer)
            {
                var img = child.GetComponentInChildren<Image>();
                if (img != null)
                    img.color = Color.white;
            }

            // 2. ����������
            GameObject worldMessageRow = Instantiate(worldSpaceMessagePrefab);
            worldMessageRow.transform.SetParent(worldSpaceChatContainer, false);
            SetupChatMessage(worldMessageRow, message, sender);

            // 3. ǿ��������Ϊ��ɫ
            var newImg = worldMessageRow.GetComponentInChildren<Image>();
            if (newImg != null)
                newImg.color = new Color(0.7f, 1f, 0.7f, 1f); // ǳ��ɫ
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
            // ����ռ�������ɫ��UpdateChatHistory����
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
        if (isNotification) return; // ֪ͨ����Ϣ������
        AudioManager.Instance.StopSpeaking(); // ֱ���ж���һ��TTS
        _ = AudioManager.Instance.SpeakText(text); // ������TTS
    }
    
    private void ToggleChatPanel()
    {
        isChatPanelVisible = !isChatPanelVisible;
        chatPanel.SetActive(isChatPanelVisible);
        
        // �����������ʾʱ������AgentButton����֮��Ȼ
        if (agentButton != null)
        {
            agentButton.gameObject.SetActive(!isChatPanelVisible);
            Debug.Log($"AgentButton visibility toggled: {!isChatPanelVisible}");
        }
        
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
            
            // ͬʱ����AgentButton
            if (agentButton != null)
            {
                agentButton.gameObject.SetActive(false);
            }
            
            Debug.Log("Chat panel shown, AgentButton hidden");
        }
    }

    public void HideChatPanel()
    {
        if (chatPanel != null)
        {
            isChatPanelVisible = false;
            chatPanel.SetActive(false);
            
            // ͬʱ��ʾAgentButton
            if (agentButton != null)
            {
                agentButton.gameObject.SetActive(true);
            }
            
            Debug.Log("Chat panel hidden, AgentButton shown");
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
            systemNotificationText.richText = true;
            systemNotificationText.parseCtrlCharacters = true;
            
            UserProfile profile = LearningProgressManager.Instance.userProfile;
            Color levelColor = profile.GetLevelColor();
            string colorHex = ColorUtility.ToHtmlStringRGB(levelColor);
            
            // ������Ƶ�����֪ͨ��������
            StringBuilder levelUpText = new StringBuilder();
            levelUpText.AppendLine("<size=30><color=#FFD700><b>LEVEL UP!</b></color></size>");
            
            // ����һ���򵥵ķָ���
            levelUpText.AppendLine("<size=26>-----------</size>");
            
            // ͻ����ʾ�µȼ���ͷ��
            levelUpText.AppendLine($"<size=34><color=#{colorHex}><b>{newLevel}</b></color></size>");
            levelUpText.AppendLine($"<size=26><i>\"{profile.GetLevelTitle()}\"</i></size>");
            
            // ���һЩ�����Ե���Ϣ
            string[] encouragements = new string[] {
                "Great progress! Keep going!",
                "You're getting better every day!",
                "Keep up the good work!",
                "Your skills are growing!",
                "Well done! Continue learning!"
            };
            
            // ���ѡ��һ��������Ϣ
            string encouragement = encouragements[UnityEngine.Random.Range(0, encouragements.Length)];
            levelUpText.AppendLine($"<size=22><color=#E0E0E0>{encouragement}</color></size>");
            
            systemNotificationText.text = levelUpText.ToString();
            systemNotificationText.color = Color.white;
            systemNotificationPanel.SetActive(true);
            
            // 4����Զ��ر�
            Invoke(nameof(CloseSystemNotificationPanel), 4f);
            
            // ���촰����ʾ����������Ϣ
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
    
    // ������Ƶĳɾ�չʾ���棬���������
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
                
                // ������ʾ��������
                achievementText.AppendLine($"<size=20><color=#E0E0E0>Unlocked: <color=#32CD32>{unlocked}</color> / <color=#FFD700>{total}</color></color></size>");
                achievementText.AppendLine("<size=22>---------------------</size>");
                
                // �����ͷ���
                foreach (AchievementType type in System.Enum.GetValues(typeof(AchievementType)))
                {
                    var typeAchievements = achievements.Where(a => a.type == type).ToList();
                    if (typeAchievements.Count > 0)
                    {
                        // ���������
                        achievementText.AppendLine($"<size=24><color=#00BFFF><b>[ {type} ]</b></color></size>");
                        
                        // ��Ӹ����͵ĳɾͣ�ʹ�ñ�񲼾�
                        foreach (var a in typeAchievements)
                        {
                            string status = a.isUnlocked 
                                ? "<color=#32CD32><b>[?]</b></color>" 
                                : "<color=#A0A0A0>[��]</color>";
                                
                            string titleColor = a.isUnlocked ? "#FFFFFF" : "#A0A0A0";
                            
                            // �ɾͱ����У���״̬�;���ֵ��
                            achievementText.AppendLine($"{status} <color={titleColor}><b>{a.title}</b></color> <color=#00FF00>+{a.rewardExp}xp</color>");
                            
                            // ֻΪ�ѽ����ĳɾ���ʾ����
                            if (a.isUnlocked)
                            {
                                achievementText.AppendLine($"    <size=18><i>{a.description}</i></size>");
                            }
                        }
                        
                        achievementText.AppendLine();
                    }
                }
                
                // �ײ���ʾ�ܽ���Ϣ
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

    // ������Ƶĳɾͽ���֪ͨ�����������
    public void ShowAchievementUnlocked(Achievement achievement)
    {
        if (systemNotificationPanel != null && systemNotificationText != null)
        {
            systemNotificationText.richText = true;
            systemNotificationText.parseCtrlCharacters = true;
            
            // ���ݳɾ�����ѡ����ɫ
            string typeColor;
            switch (achievement.type)
            {
                case AchievementType.Conversation: typeColor = "#6495ED"; break; // ��ɫ
                case AchievementType.Learning: typeColor = "#9370DB"; break;     // ��ɫ
                case AchievementType.Experience: typeColor = "#FFD700"; break;   // ��ɫ
                case AchievementType.Level: typeColor = "#20B2AA"; break;        // ��ɫ
                case AchievementType.Special: typeColor = "#FF6347"; break;      // ��ɫ
                default: typeColor = "#FFD700"; break;                           // Ĭ�Ͻ�ɫ
            }
            
            StringBuilder notificationText = new StringBuilder();
            
            // �������
            notificationText.AppendLine("<size=26><color=#FFD700><b>ACHIEVEMENT</b></color></size>");
            
            // ͻ����ʾ�ɾͱ��⣬ʹ�óɾ����͵���ɫ
            notificationText.AppendLine($"<size=30><color={typeColor}><b>{achievement.title}</b></color></size>");
            
            // �ɾ����ͺ�����
            notificationText.AppendLine($"<size=18><i><color=#C0C0C0>{achievement.type}</color></i></size>");
            notificationText.AppendLine($"<size=22>{achievement.description}</size>");
            
            // ��Ŀ��ʾ����
            notificationText.AppendLine($"<size=24><color=#00FF00><b>+{achievement.rewardExp} EXP</b></color></size>");
            
            systemNotificationText.text = notificationText.ToString();
            systemNotificationText.color = Color.white;
            systemNotificationPanel.SetActive(true);
            Invoke(nameof(CloseSystemNotificationPanel), 4f); // ������ʾʱ��Ϊ4��
            
            Debug.Log($"Achievement notification shown: {achievement.title}");
            
            // ���촰����ϢҲ��
            UpdateChatHistory($"Achievement: {achievement.title} (+{achievement.rewardExp} EXP)", ChatManager.Sender.Tutor, true);
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
        // ֱ�ӳ��Ի�ȡAR����ͷ���棬ʹ��ARCameraManagerʵ������
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
            yield return ChatManager.Instance.SendSceneRecognitionRequest(System.Convert.ToBase64String(texture.EncodeToPNG()), texture, userText, true); // Always first interaction for camera capture
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

        // �ڵ��Agent��ťʱ�������������
        if (chatPanel != null)
        {
            chatPanel.SetActive(false);
            isChatPanelVisible = false;
            Debug.Log("Agent clicked: Chat panel hidden");
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
        Debug.Log("Starting agent voice recording...");
        isRecording = true;
        
        // Change button color to indicate recording
        Image agentButtonImage = agentButton.GetComponent<Image>();
        if (agentButtonImage != null)
        {
            agentButtonImage.color = recordingColor;
        }
        
        // Start recording via AudioManager
        AudioManager.Instance.StartRecording();
        
        // Disable other UI during recording
        SetUIInteractable(false);
        agentButton.interactable = true; // Keep agent button active to stop recording
    }
    
    private void StopAgentRecording()
    {
        Debug.Log("Stopping agent voice recording...");
        
        // Stop recording via AudioManager
        AudioManager.Instance.StopRecording();
        
        ResetAgentButton();
    }
    
    private void ResetAgentButton()
    {
        isRecording = false;
        
        // Reset button color
        Image agentButtonImage = agentButton.GetComponent<Image>();
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
    /// ��������TextMeshPro�����֧�ָ��ı��ͱ������
    /// </summary>
    private void ConfigureTextComponents()
    {
        // ȷ��ϵͳ֪ͨ�ı�֧�ָ��ı��ͱ������
        if (systemNotificationText != null)
        {
            systemNotificationText.richText = true;
            systemNotificationText.parseCtrlCharacters = true;
            Debug.Log("System notification text configured for rich text and emoji support");
        }
        
        // ����������Ҫ���õ�TextMeshPro���
        if (levelText != null) levelText.richText = true;
        if (expText != null) expText.richText = true;
        
        // ���һ����е�����TextMeshPro�������������
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

    // Learning Mode Button Handlers
    private void OnSceneModeButtonClick()
    {
        Debug.Log("Scene Mode button clicked");
        
        // Switch to scene learning mode
        if (LearningModeManager.Instance != null)
        {
            LearningModeManager.Instance.SwitchMode(LearningModeManager.LearningMode.Scene);
        }
        else
        {
            Debug.LogWarning("LearningModeManager not found! Please add LearningModeManager to the scene.");
            UpdateChatHistory("Scene learning mode unavailable. Please check LearningModeManager setup.", ChatManager.Sender.Tutor);
        }
    }
    
    private void OnWordModeButtonClick()
    {
        Debug.Log("Word Mode button clicked");
        
        // Switch to word learning mode
        if (LearningModeManager.Instance != null)
        {
            LearningModeManager.Instance.SwitchMode(LearningModeManager.LearningMode.Word);
        }
        else
        {
            Debug.LogWarning("LearningModeManager not found! Please add LearningModeManager to the scene.");
            UpdateChatHistory("Word learning mode unavailable. Please check LearningModeManager setup.", ChatManager.Sender.Tutor);
        }
    }
    
    private void OnNormalModeButtonClick()
    {
        Debug.Log("Normal Mode button clicked");
        
        // Switch to normal chat mode
        if (LearningModeManager.Instance != null)
        {
            LearningModeManager.Instance.SwitchMode(LearningModeManager.LearningMode.Normal);
        }
        else
        {
            Debug.LogWarning("LearningModeManager not found! Please add LearningModeManager to the scene.");
            UpdateChatHistory("Normal chat mode unavailable. Please check LearningModeManager setup.", ChatManager.Sender.Tutor);
        }
    }
    
    private void OnReviewButtonClick()
    {
        Debug.Log("Review button clicked");
        
        // Toggle review panel visibility
        if (ReviewManager.Instance != null)
        {
            ReviewManager.Instance.ToggleReviewPanel();
        }
        else
        {
            Debug.LogWarning("ReviewManager not found! Please add ReviewManager to the scene.");
            UpdateChatHistory("Review function unavailable. Please check ReviewManager setup.", ChatManager.Sender.Tutor);
        }
    }
    
    /// <summary>
    /// ����Ƿ������ʱ�����г�������
    /// </summary>
    private bool HasRecentSceneInteraction()
    {
        return (Time.time - lastSceneInteractionTime) < SCENE_INTERACTION_COOLDOWN;
    }
    
    /// <summary>
    /// ��¼��������ʱ��
    /// </summary>
    private void RecordSceneInteraction()
    {
        lastSceneInteractionTime = Time.time;
        Debug.Log($"[ChatTestUI] Scene interaction recorded at time: {lastSceneInteractionTime}");
    }
}
