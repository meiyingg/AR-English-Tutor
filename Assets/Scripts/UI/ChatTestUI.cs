using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ChatTestUI : MonoBehaviour
{
    public TMP_InputField inputField;
    public Button sendButton;
    public Button imageButton; // New image upload button
    public GameObject chatMessagePrefab; // ���ǽ�ʵ��������ϢԤ����
    public GameObject imageMessagePrefab; // New prefab for image messages
    public Transform chatContentPanel; // ScrollView��Content����

    [Header("Chat Panel Control")]
    public GameObject chatPanel; // Chat���ĸ�GameObject
    public Button toggleChatButton; // ������ʾ/���صİ�ť
    private bool isChatPanelVisible = true; // �����ʾ״̬

    [Header("Chat Bubble Colors")]
    public Color userBubbleColor = new Color(0.0f, 0.5f, 1.0f); // ��ɫ
    public Color tutorBubbleColor = new Color(0.8f, 0.8f, 0.8f); // ��ɫ

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
        // ���� �����޸���ʹ�� SetParent(parent, false) ��ȷ����ȷ��UI���źͶ�λ ����
        // 1. ��������ռ��д���Ԥ���壬��ָ��������
        GameObject messageRow = Instantiate(chatMessagePrefab);

        // 2. ʹ�� SetParent ������������ false
        //    ��� false ���� (worldPositionStays) �ǽ������Ĺؼ�
        //    ������Unity������Ҫ��ͼ�����������ԭ��������λ�ã�
        //    ����������λ�á���ת��������ȫ�����µĸ����������¼��㡣��
        messageRow.transform.SetParent(chatContentPanel, false);

        // ʹ��ChatMessageUI�ű���������Ϣ�Ͷ���
        ChatMessageUI messageUI = messageRow.GetComponent<ChatMessageUI>();
        if (messageUI != null)
        {
            // ������Ϣ���ݺͶ��뷽ʽ��true��ʾ�û���Ϣ���Ҷ��룩
            messageUI.SetMessage(message, sender == ChatManager.Sender.User);
        }
        
        // ����������ɫ
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
}