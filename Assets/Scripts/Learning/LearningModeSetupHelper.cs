using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 学习模式快速设置助手 - 帮助快速配置学习模式按钮
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
        // 查找或创建ChatTestUI引用
        if (chatTestUI == null)
        {
            chatTestUI = FindObjectOfType<ChatTestUI>();
        }
        
        if (chatTestUI == null)
        {
            Debug.LogError("ChatTestUI not found! Please assign it manually.");
            return;
        }
        
        // 查找或创建主Canvas
        if (mainCanvas == null)
        {
            mainCanvas = FindObjectOfType<Canvas>();
        }
        
        if (mainCanvas == null)
        {
            Debug.LogError("Main Canvas not found! Please assign it manually.");
            return;
        }
        
        // 创建或查找场景模式按钮
        Button sceneButton = FindOrCreateButton(sceneModeButtonName, sceneModeButtonText);
        if (sceneButton != null)
        {
            // 这里您需要手动在Inspector中将按钮拖拽到ChatTestUI的sceneModeButton字段
            Debug.Log($"Scene Mode Button created: {sceneButton.name}. Please drag it to ChatTestUI.sceneModeButton field in Inspector.");
        }
        
        // 创建或查找单词模式按钮
        Button wordButton = FindOrCreateButton(wordModeButtonName, wordModeButtonText);
        if (wordButton != null)
        {
            // 这里您需要手动在Inspector中将按钮拖拽到ChatTestUI的wordModeButton字段
            Debug.Log($"Word Mode Button created: {wordButton.name}. Please drag it to ChatTestUI.wordModeButton field in Inspector.");
        }
        
        // 创建或查找普通模式按钮
        Button normalButton = FindOrCreateButton(normalModeButtonName, normalModeButtonText);
        if (normalButton != null)
        {
            // 这里您需要手动在Inspector中将按钮拖拽到ChatTestUI的normalModeButton字段
            Debug.Log($"Normal Mode Button created: {normalButton.name}. Please drag it to ChatTestUI.normalModeButton field in Inspector.");
        }
        
        // 确保学习模式管理器存在
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
        // 首先尝试查找现有按钮
        Button existingButton = GameObject.Find(buttonName)?.GetComponent<Button>();
        if (existingButton != null)
        {
            Debug.Log($"Found existing button: {buttonName}");
            return existingButton;
        }
        
        // 创建新按钮
        GameObject buttonObj = new GameObject(buttonName);
        buttonObj.transform.SetParent(mainCanvas.transform, false);
        
        // 添加Button组件
        Button button = buttonObj.AddComponent<Button>();
        
        // 添加Image组件（Button需要）
        Image image = buttonObj.AddComponent<Image>();
        image.color = Color.white;
        
        // 创建文本子对象
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(buttonObj.transform, false);
        
        // 添加TextMeshPro组件
        TextMeshProUGUI text = textObj.AddComponent<TextMeshProUGUI>();
        text.text = buttonText;
        text.fontSize = 14;
        text.alignment = TextAlignmentOptions.Center;
        text.color = Color.black;
        
        // 设置RectTransform
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
