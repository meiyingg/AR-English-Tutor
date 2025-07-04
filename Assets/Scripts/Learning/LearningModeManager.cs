using UnityEngine;
using System.Collections;
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
        
        // ��ʼ����ť��ɫ�Է�ӳĬ�ϵ�Normalģʽ
        UpdateModeButtonColors();
    }
    
    /// <summary>
    /// �л�ѧϰģʽ
    /// </summary>
    public void SwitchMode(LearningMode newMode)
    {
        LearningMode previousMode = currentMode;
        currentMode = newMode;
        
        Debug.Log($"Learning mode switched from {previousMode} to {currentMode}");
        
        // ���°�ť��ɫ�Է�ӳ��ǰģʽ
        UpdateModeButtonColors();
        
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
            LearningMode.Scene => "<color=#9C27B0><b>Scene Learning Mode</b></color> activated! Please let me see the nearby environment to start our lesson.",
            LearningMode.Word => "<color=#F44336><b>Word Learning Mode</b></color> activated! Please let me see the nearby environment to start our lesson.",
            LearningMode.Normal => "<color=#4CAF50><b>Normal Chat Mode</b></color> activated! Feel free to chat about anything you'd like to learn.",
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
    
    /// <summary>
    /// ����ģʽ��ť��ɫ�Է�ӳ��ǰ�����ģʽ
    /// </summary>
    private void UpdateModeButtonColors()
    {
        if (ChatTestUI.Instance == null) return;
        
        // Ĭ����ɫ����ɫ/δ���
        Color defaultColor = Color.white;
        // ������ɫ����ɫ��
        Color activeColor = Color.green;
        
        // �������а�ťΪĬ����ɫ
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
        
        // ���õ�ǰ����ģʽ�İ�ťΪ��ɫ
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
    
    /// <summary>
    /// �Զ���AI�ظ�����ȡ����¼ѧϰ����
    /// </summary>
    public void ProcessLearningContent(string aiResponse)
    {
        if (string.IsNullOrEmpty(aiResponse))
            return;

        // Find ReviewManager in scene if not already referenced
        if (ReviewManager.Instance == null)
        {
            Debug.LogWarning("ReviewManager instance not found");
            return;
        }

        // Extract and record content based on current mode
        switch (currentMode)
        {
            case LearningMode.Word:
                ExtractAndRecordWords(aiResponse);
                break;
            case LearningMode.Scene:
                ExtractAndRecordTopics(aiResponse);
                break;
            // Normal mode doesn't auto-record content
        }
    }

    /// <summary>
    /// Extract words from AI response and record them for review
    /// </summary>
    private void ExtractAndRecordWords(string response)
    {
        // Simple word extraction - look for patterns like vocabulary teaching
        string lowerResponse = response.ToLower();
        
        // Look for common vocabulary teaching patterns
        if (lowerResponse.Contains("word") || lowerResponse.Contains("vocabulary"))
        {
            // Extract words that appear to be vocabulary items
            // For simplicity, look for words that are emphasized or defined
            string[] sentences = response.Split('.', '!', '?');
            
            foreach (string sentence in sentences)
            {
                // Look for definition patterns like "X means Y" or "X is Y"
                if (sentence.Contains(" means ") || sentence.Contains(" is "))
                {
                    string[] parts = sentence.Split(new string[] { " means ", " is " }, System.StringSplitOptions.None);
                    if (parts.Length >= 2)
                    {
                        string word = ExtractWord(parts[0]);
                        if (!string.IsNullOrEmpty(word))
                        {
                            ReviewManager.Instance.AddLearnedWord(word);
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// Extract topic from AI response and record it for review
    /// </summary>
    private void ExtractAndRecordTopics(string response)
    {
        // Simple topic extraction based on scene learning context
        string topic = "";
        
        // Look for common scene/location keywords
        string lowerResponse = response.ToLower();
        
        if (lowerResponse.Contains("restaurant") || lowerResponse.Contains("dining"))
            topic = "Restaurant Scene";
        else if (lowerResponse.Contains("shopping") || lowerResponse.Contains("store"))
            topic = "Shopping Scene";
        else if (lowerResponse.Contains("airport") || lowerResponse.Contains("travel"))
            topic = "Airport Scene";
        else if (lowerResponse.Contains("school") || lowerResponse.Contains("classroom"))
            topic = "School Scene";
        else if (lowerResponse.Contains("park") || lowerResponse.Contains("outdoor"))
            topic = "Park Scene";
        else if (lowerResponse.Contains("office") || lowerResponse.Contains("work"))
            topic = "Office Scene";
        else if (lowerResponse.Contains("hospital") || lowerResponse.Contains("medical"))
            topic = "Hospital Scene";
        else if (lowerResponse.Contains("home") || lowerResponse.Contains("house"))
            topic = "Home Scene";
        
        if (!string.IsNullOrEmpty(topic))
        {
            ReviewManager.Instance.AddLearnedTopic(topic);
        }
    }

    /// <summary>
    /// Extract clean word from text
    /// </summary>
    private string ExtractWord(string text)
    {
        if (string.IsNullOrEmpty(text))
            return "";
        
        // Clean up the text to extract just the word
        text = text.Trim();
        text = text.Replace("\"", "").Replace("'", "").Replace(",", "");
        
        // Take only the first word if multiple words
        string[] words = text.Split(' ');
        if (words.Length > 0)
        {
            string word = words[words.Length - 1]; // Take the last word (likely the vocabulary word)
            
            // Remove common prefixes
            if (word.StartsWith("the "))
                word = word.Substring(4);
            
            return word.Trim();
        }
        
        return "";
    }

    /// <summary>
    /// Show review panel
    /// </summary>
    public void ShowReview()
    {
        if (ReviewManager.Instance != null)
        {
            ReviewManager.Instance.ShowReviewPanel();
        }
    }

    /// <summary>
    /// Process AI response to extract learning content for review
    /// Called when AI responds with educational content
    /// </summary>
    public void ProcessAIResponseForReview(string aiResponse)
    {
        if (ReviewManager.Instance == null || string.IsNullOrEmpty(aiResponse))
            return;

        switch (currentMode)
        {
            case LearningMode.Word:
                ExtractWordsFromResponse(aiResponse);
                break;
            case LearningMode.Scene:
                ExtractTopicsFromResponse(aiResponse);
                break;
            // Normal mode doesn't auto-extract for review
        }
    }

    /// <summary>
    /// Extract vocabulary words from AI response
    /// Simple keyword detection for Word mode
    /// </summary>
    private void ExtractWordsFromResponse(string response)
    {
        if (ReviewManager.Instance == null) return;

        // Simple keyword extraction - look for common vocabulary teaching patterns
        string[] wordIndicators = { "word:", "vocabulary:", "learn the word", "new word", "key word" };
        string lowerResponse = response.ToLower();

        foreach (string indicator in wordIndicators)
        {
            if (lowerResponse.Contains(indicator))
            {
                // Try to extract the word that follows the indicator
                int index = lowerResponse.IndexOf(indicator);
                if (index >= 0)
                {
                    string remaining = response.Substring(index + indicator.Length).Trim();
                    string[] words = remaining.Split(new char[] { ' ', '.', ',', ':', ';', '!', '?' }, System.StringSplitOptions.RemoveEmptyEntries);
                    
                    if (words.Length > 0)
                    {
                        string word = words[0].Trim('\"', '\'', '(', ')');
                        if (word.Length > 1 && word.Length < 20) // Reasonable word length
                        {
                            ReviewManager.Instance.AddLearnedWord(word);
                            Debug.Log($"Added word to review: {word}");
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// Extract topics from AI response
    /// Simple topic detection for Scene mode
    /// </summary>
    private void ExtractTopicsFromResponse(string response)
    {
        if (ReviewManager.Instance == null) return;

        // Common scene topics to look for
        string[] topicKeywords = {
            "restaurant", "dining", "ordering food", "menu",
            "shopping", "store", "buying", "cashier",
            "airport", "flight", "travel", "hotel",
            "office", "meeting", "workplace", "business",
            "school", "classroom", "studying", "homework",
            "hospital", "doctor", "health", "medicine",
            "park", "nature", "outdoor", "exercise"
        };

        string lowerResponse = response.ToLower();
        
        foreach (string topic in topicKeywords)
        {
            if (lowerResponse.Contains(topic))
            {
                ReviewManager.Instance.AddLearnedTopic(topic);
                Debug.Log($"Added topic to review: {topic}");
                return; // Only add one topic per response to avoid duplicates
            }
        }
        
        // If no predefined topic found, try to extract from context
        if (lowerResponse.Contains("scene") || lowerResponse.Contains("location") || lowerResponse.Contains("place"))
        {
            // Extract a general topic based on current mode
            string generalTopic = $"scene_conversation_{System.DateTime.Now:yyyyMMdd}";
            ReviewManager.Instance.AddLearnedTopic(generalTopic);
        }
    }

    /// <summary>
    /// Manual method to add learned content
    /// Can be called from UI or other systems
    /// </summary>
    public void AddToReview(string content, bool isWord = true)
    {
        if (ReviewManager.Instance == null || string.IsNullOrEmpty(content))
            return;

        if (isWord)
        {
            ReviewManager.Instance.AddLearnedWord(content);
        }
        else
        {
            ReviewManager.Instance.AddLearnedTopic(content);
        }
    }
}
