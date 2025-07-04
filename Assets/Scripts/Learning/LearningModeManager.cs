using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// ѧϰģʽ������ - ����ͬ��ѧϰģʽ
/// ���ɵ����е�����ϵͳ�У�ͨ��AI�Ի����չʾѧϰ����
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
        Normal,      // ��ͨ�Ի�ģʽ
        Scene,       // ����ѧϰģʽ
        Word         // ����ѧϰģʽ
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
    }
    
    /// <summary>
    /// �л�ѧϰģʽ
    /// </summary>
    public void SwitchMode(LearningMode newMode)
    {
        LearningMode previousMode = currentMode;
        currentMode = newMode;
        
        Debug.Log($"Learning mode switched from {previousMode} to {currentMode}");
        
        // �����촰����ʾģʽ�л���ʾ
        string modeMessage = GetModeMessage(newMode);
        if (ChatManager.Instance != null)
        {
            ChatManager.Instance.onNewMessage?.Invoke(modeMessage, ChatManager.Sender.Tutor, false);
        }
    }
    
    /// <summary>
    /// ��ȡ��ǰģʽ��AI��ʾģ��
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
    /// ����û��ı��Ƿ����ģʽ�л�ָ��
    /// </summary>
    public bool CheckForModeCommand(string userText)
    {
        string lowerText = userText.ToLower();
        
        // ��鳡��ģʽָ��
        if (lowerText.Contains("scene mode") || lowerText.Contains("scene learning"))
        {
            SwitchMode(LearningMode.Scene);
            return true;
        }
        
        // ��鵥��ģʽָ��
        if (lowerText.Contains("word mode") || lowerText.Contains("word learning") || 
            lowerText.Contains("vocabulary"))
        {
            SwitchMode(LearningMode.Word);
            return true;
        }
        
        // �����ͨģʽָ��
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
    /// ��ȡģʽ�л���ʾ��Ϣ
    /// </summary>
    private string GetModeMessage(LearningMode mode)
    {
        return mode switch
        {
            LearningMode.Scene => "Scene Learning Mode activated! Please let me see the nearby environment to start our lesson.",
            LearningMode.Word => "Word Learning Mode activated! Please let me see the nearby environment to start our lesson.",
            LearningMode.Normal => "Normal Chat Mode activated! Feel free to chat about anything you'd like to learn.",
            _ => "Mode switched successfully!"
        };
    }
    
    /// <summary>
    /// ��ȡ��ǰģʽ����ʾ����
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
    /// ���ݵ�ǰģʽ����AI��ʾ��
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
}
