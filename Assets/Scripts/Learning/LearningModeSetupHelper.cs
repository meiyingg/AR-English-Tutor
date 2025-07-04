using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// ѧϰģʽ������������ - ������������ѧϰģʽ��ť
/// </summary>
public class LearningModeSetupHelper : MonoBehaviour
{
    [Header("Auto Setup")]
    public bool autoSetupOnStart = true;
    
    [Header("Button Configuration")]
    public string sceneModeButtonName = "SceneModeButton";
    public string wordModeButtonName = "WordModeButton";
    public string normalModeButtonName = "NormalModeButton";
    public string sceneModeButtonText = "Scene Learning";
    public string wordModeButtonText = "Word Learning";
    public string normalModeButtonText = "Normal Chat";
    
    [Header("UI References")]
    public ChatTestUI chatTestUI;
    public Canvas mainCanvas;
    
    private void Start()
    {
        if (autoSetupOnStart)
        {
            SetupLearningModeButtons();
        }
    }
    
    [ContextMenu("Setup Learning Mode Buttons")]
    public void SetupLearningModeButtons()
    {
        // ���һ򴴽�ChatTestUI����
        if (chatTestUI == null)
        {
            chatTestUI = FindObjectOfType<ChatTestUI>();
        }
        
        if (chatTestUI == null)
        {
            Debug.LogError("ChatTestUI not found! Please assign it manually.");
            return;
        }
        
        // ���һ򴴽���Canvas
        if (mainCanvas == null)
        {
            mainCanvas = FindObjectOfType<Canvas>();
        }
        
        if (mainCanvas == null)
        {
            Debug.LogError("Main Canvas not found! Please assign it manually.");
            return;
        }
        
        // ��������ҳ���ģʽ��ť
        Button sceneButton = FindOrCreateButton(sceneModeButtonName, sceneModeButtonText);
        if (sceneButton != null)
        {
            // ��������Ҫ�ֶ���Inspector�н���ť��ק��ChatTestUI��sceneModeButton�ֶ�
            Debug.Log($"Scene Mode Button created: {sceneButton.name}. Please drag it to ChatTestUI.sceneModeButton field in Inspector.");
        }
        
        // ��������ҵ���ģʽ��ť
        Button wordButton = FindOrCreateButton(wordModeButtonName, wordModeButtonText);
        if (wordButton != null)
        {
            // ��������Ҫ�ֶ���Inspector�н���ť��ק��ChatTestUI��wordModeButton�ֶ�
            Debug.Log($"Word Mode Button created: {wordButton.name}. Please drag it to ChatTestUI.wordModeButton field in Inspector.");
        }
        
        // �����������ͨģʽ��ť
        Button normalButton = FindOrCreateButton(normalModeButtonName, normalModeButtonText);
        if (normalButton != null)
        {
            // ��������Ҫ�ֶ���Inspector�н���ť��ק��ChatTestUI��normalModeButton�ֶ�
            Debug.Log($"Normal Mode Button created: {normalButton.name}. Please drag it to ChatTestUI.normalModeButton field in Inspector.");
        }
        
        // ȷ��ѧϰģʽ����������
        if (LearningModeManager.Instance == null)
        {
            GameObject modeManager = new GameObject("LearningModeManager");
            modeManager.AddComponent<LearningModeManager>();
            Debug.Log("LearningModeManager created automatically.");
        }
        
        Debug.Log("Learning Mode setup complete! Don't forget to assign the buttons in ChatTestUI Inspector.");
    }
    
    private Button FindOrCreateButton(string buttonName, string buttonText)
    {
        // ���ȳ��Բ������а�ť
        Button existingButton = GameObject.Find(buttonName)?.GetComponent<Button>();
        if (existingButton != null)
        {
            Debug.Log($"Found existing button: {buttonName}");
            return existingButton;
        }
        
        // �����°�ť
        GameObject buttonObj = new GameObject(buttonName);
        buttonObj.transform.SetParent(mainCanvas.transform, false);
        
        // ���Button���
        Button button = buttonObj.AddComponent<Button>();
        
        // ���Image�����Button��Ҫ��
        Image image = buttonObj.AddComponent<Image>();
        image.color = Color.white;
        
        // �����ı��Ӷ���
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(buttonObj.transform, false);
        
        // ���TextMeshPro���
        TextMeshProUGUI text = textObj.AddComponent<TextMeshProUGUI>();
        text.text = buttonText;
        text.fontSize = 14;
        text.alignment = TextAlignmentOptions.Center;
        text.color = Color.black;
        
        // ����RectTransform
        RectTransform buttonRect = buttonObj.GetComponent<RectTransform>();
        buttonRect.sizeDelta = new Vector2(120, 40);
        
        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;
        textRect.anchoredPosition = Vector2.zero;
        
        Debug.Log($"Created new button: {buttonName}");
        return button;
    }
}
