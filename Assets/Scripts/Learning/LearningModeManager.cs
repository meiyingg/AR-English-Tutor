using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 学习模式管理器 - 管理不同的学习模式
/// 集成到现有的聊天系统中，通过AI对话面板展示学习内容
/// </summary>
public class LearningModeManager : MonoBehaviour
{
    public static LearningModeManager Instance { get; private set; }
    
    [Header("Learning Mode Settings")]
    public LearningMode currentMode = LearningMode.Normal;
    
    [Header("Mode Prompt Templates")]
    [TextArea(3, 5)]
    public string scenePromptTemplate = "You are an English tutor. Look at this scene and create an interactive English lesson.\n\nInstructions:\n1. Describe the scene briefly\n2. Start a natural conversation about this location\n3. Ask engaging questions\n4. Keep responses conversational and educational\n5. Adapt to the user's level\n\nStart the lesson now!";
    
    [TextArea(3, 5)]
    public string wordPromptTemplate = "You are an English tutor. Look at this image and focus on vocabulary learning.\n\nInstructions:\n1. Identify the main objects in the image\n2. Choose 1-2 key vocabulary words\n3. Provide simple definitions and example sentences\n4. Create a short dialogue using these words\n5. Ask the user to practice with these words\n\nStart the vocabulary lesson now!";
    
    public enum LearningMode
    {
        Normal,      // 普通对话模式
        Scene,       // 场景学习模式
        Word         // 单词学习模式
    }
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private void Start()
    {
        Debug.Log("LearningModeManager initialized - Ready for mode switching");
        
        // Start in Normal mode and show initial mode message after a delay
        StartCoroutine(InitializeNormalMode());
    }
    
    private System.Collections.IEnumerator InitializeNormalMode()
    {
        // Wait for other systems to initialize
        yield return new WaitForSeconds(2f);
        
        // Normal mode is activated by default but no message is shown in chat
        // The default mode activation should not clutter the chat interface
        Debug.Log("Learning system initialized in Normal Mode - ready for natural conversation and grammar corrections");
        
        // 初始化按钮颜色以反映默认的Normal模式
        UpdateModeButtonColors();
    }
    
    /// <summary>
    /// 切换学习模式
    /// </summary>
    public void SwitchMode(LearningMode newMode)
    {
        LearningMode previousMode = currentMode;
        currentMode = newMode;
        
        Debug.Log($"Learning mode switched from {previousMode} to {currentMode}");
        
        // 更新按钮颜色以反映当前模式
        UpdateModeButtonColors();
        
        // 在聊天窗口显示模式切换提示
        string modeMessage = GetModeMessage(newMode);
        if (ChatManager.Instance != null)
        {
            ChatManager.Instance.onNewMessage?.Invoke(modeMessage, ChatManager.Sender.Tutor, false);
        }
    }
    
    /// <summary>
    /// 获取当前模式的AI提示模板
    /// </summary>
    public string GetCurrentModePrompt()
    {
        return currentMode switch
        {
            LearningMode.Scene => scenePromptTemplate,
            LearningMode.Word => wordPromptTemplate,
            _ => "" // Normal mode uses default prompts
        };
    }
    
    /// <summary>
    /// 检查用户文本是否包含模式切换指令
    /// </summary>
    public bool CheckForModeCommand(string userText)
    {
        string lowerText = userText.ToLower();
        
        // 检查场景模式指令
        if (lowerText.Contains("scene mode") || lowerText.Contains("scene learning"))
        {
            SwitchMode(LearningMode.Scene);
            return true;
        }
        
        // 检查单词模式指令
        if (lowerText.Contains("word mode") || lowerText.Contains("word learning") || 
            lowerText.Contains("vocabulary"))
        {
            SwitchMode(LearningMode.Word);
            return true;
        }
        
        // 检查普通模式指令
        if (lowerText.Contains("normal mode") || lowerText.Contains("chat mode") || 
            lowerText.Contains("normal") || lowerText.Contains("exit") || 
            lowerText.Contains("back to normal") || lowerText.Contains("regular chat"))
        {
            SwitchMode(LearningMode.Normal);
            return true;
        }
        
        return false;
    }
    
    /// <summary>
    /// 获取模式切换提示消息
    /// </summary>
    private string GetModeMessage(LearningMode mode)
    {
        return mode switch
        {
            LearningMode.Scene => "<color=#9C27B0><b>Scene Learning Mode</b></color> activated! Please let me see the nearby environment to start our lesson.",
            LearningMode.Word => "<color=#F44336><b>Word Learning Mode</b></color> activated! Please let me see the nearby environment to start our lesson.",
            LearningMode.Normal => "<color=#4CAF50><b>Normal Chat Mode</b></color> activated! Feel free to chat about anything you'd like to learn.",
            _ => "Mode switched successfully!"
        };
    }
    
    /// <summary>
    /// 获取当前模式的显示名称
    /// </summary>
    public string GetCurrentModeDisplayName()
    {
        return currentMode switch
        {
            LearningMode.Scene => "Scene Learning",
            LearningMode.Word => "Word Learning", 
            LearningMode.Normal => "Normal Chat",
            _ => "Unknown"
        };
    }
    
    /// <summary>
    /// 根据当前模式调整AI提示词
    /// </summary>
    public string AdjustPromptForMode(string basePrompt)
    {
        if (currentMode == LearningMode.Normal)
        {
            return basePrompt;
        }
        
        string modePrompt = GetCurrentModePrompt();
        if (!string.IsNullOrEmpty(modePrompt))
        {
            return modePrompt + "\n\n" + basePrompt;
        }
        
        return basePrompt;
    }
    
    /// <summary>
    /// 更新模式按钮颜色以反映当前激活的模式
    /// </summary>
    private void UpdateModeButtonColors()
    {
        if (ChatTestUI.Instance == null) return;
        
        // 默认颜色（白色/未激活）
        Color defaultColor = Color.white;
        // 激活颜色（绿色）
        Color activeColor = Color.green;
        
        // 重置所有按钮为默认颜色
        if (ChatTestUI.Instance.normalModeButton != null)
        {
            var normalColors = ChatTestUI.Instance.normalModeButton.colors;
            normalColors.normalColor = defaultColor;
            ChatTestUI.Instance.normalModeButton.colors = normalColors;
        }
        
        if (ChatTestUI.Instance.sceneModeButton != null)
        {
            var sceneColors = ChatTestUI.Instance.sceneModeButton.colors;
            sceneColors.normalColor = defaultColor;
            ChatTestUI.Instance.sceneModeButton.colors = sceneColors;
        }
        
        if (ChatTestUI.Instance.wordModeButton != null)
        {
            var wordColors = ChatTestUI.Instance.wordModeButton.colors;
            wordColors.normalColor = defaultColor;
            ChatTestUI.Instance.wordModeButton.colors = wordColors;
        }
        
        // 设置当前激活模式的按钮为绿色
        switch (currentMode)
        {
            case LearningMode.Normal:
                if (ChatTestUI.Instance.normalModeButton != null)
                {
                    var normalColors = ChatTestUI.Instance.normalModeButton.colors;
                    normalColors.normalColor = activeColor;
                    ChatTestUI.Instance.normalModeButton.colors = normalColors;
                }
                break;
                
            case LearningMode.Scene:
                if (ChatTestUI.Instance.sceneModeButton != null)
                {
                    var sceneColors = ChatTestUI.Instance.sceneModeButton.colors;
                    sceneColors.normalColor = activeColor;
                    ChatTestUI.Instance.sceneModeButton.colors = sceneColors;
                }
                break;
                
            case LearningMode.Word:
                if (ChatTestUI.Instance.wordModeButton != null)
                {
                    var wordColors = ChatTestUI.Instance.wordModeButton.colors;
                    wordColors.normalColor = activeColor;
                    ChatTestUI.Instance.wordModeButton.colors = wordColors;
                }
                break;
        }
    }
}
