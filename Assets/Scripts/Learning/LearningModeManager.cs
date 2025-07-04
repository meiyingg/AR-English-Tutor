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
    /// 只响应明确的模式切换命令，避免复习内容触发误切换
    /// </summary>
    public bool CheckForModeCommand(string userText)
    {
        string lowerText = userText.ToLower().Trim();
        
        // 如果复习系统正在活跃，不处理模式切换命令
        if (ReviewManager.Instance != null && ReviewManager.Instance.IsReviewActive())
        {
            return false;
        }
        
        // 检查场景模式指令 - 需要明确的模式切换意图
        if (lowerText.Contains("scene mode") || 
            lowerText.Contains("scene learning") ||
            (lowerText.Contains("switch to scene") || lowerText.Contains("activate scene")))
        {
            SwitchMode(LearningMode.Scene);
            return true;
        }
        
        // 检查单词模式指令 - 需要明确的模式切换意图
        if (lowerText.Contains("word mode") || 
            lowerText.Contains("word learning") ||
            (lowerText.Contains("switch to word") || lowerText.Contains("activate word")) ||
            (lowerText.Contains("vocabulary mode") || lowerText.Contains("vocabulary learning")))
        {
            SwitchMode(LearningMode.Word);
            return true;
        }
        
        // 检查普通模式指令
        if (lowerText.Contains("normal mode") || lowerText.Contains("chat mode") || 
            lowerText.Contains("exit mode") || lowerText.Contains("back to normal") || 
            lowerText.Contains("regular chat") ||
            (lowerText.Contains("switch to normal") || lowerText.Contains("activate normal")))
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
    
    /// <summary>
    /// 自动从AI回复中提取并记录学习内容
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
    public async void ProcessAIResponseForReview(string aiResponse)
    {
        if (ReviewManager.Instance == null || string.IsNullOrEmpty(aiResponse))
            return;

        switch (currentMode)
        {
            case LearningMode.Word:
                await ExtractWordsFromResponseWithAI(aiResponse);
                break;
            case LearningMode.Scene:
                await ExtractTopicsFromResponseWithAI(aiResponse);
                break;
            case LearningMode.Normal:
                await ExtractMixedContentFromResponseWithAI(aiResponse);
                break;
        }
    }

    /// <summary>
    /// Use AI to extract vocabulary words from AI response
    /// Enhanced version with context tracking
    /// </summary>
    private async System.Threading.Tasks.Task ExtractWordsFromResponseWithAI(string response)
    {
        if (ReviewManager.Instance == null || OpenAIManager.Instance == null) return;

        string extractionPrompt = $@"Analyze this English tutoring response and extract vocabulary being taught.
Return in this format (one per line):
WORD: [word] | MEANING: [simple definition] | CONTEXT: [usage example]

Only include words being actively taught or explained. Maximum 2 entries.
If no vocabulary is being taught, return 'NONE'.

Response to analyze: {response}

Extraction:";

        try
        {
            var messages = new System.Collections.Generic.List<ChatMessage>
            {
                new ChatMessage { role = "user", content = extractionPrompt }
            };
            
            string aiResult = await OpenAIManager.Instance.PostRequest(messages);
            
            if (!string.IsNullOrEmpty(aiResult) && aiResult.Trim().ToUpper() != "NONE")
            {
                ParseEnhancedWordExtraction(aiResult, response);
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error extracting words with AI: {e.Message}");
            ExtractWordsFromResponseSimple(response);
        }
    }

    /// <summary>
    /// Parse enhanced word extraction result
    /// </summary>
    private void ParseEnhancedWordExtraction(string aiResult, string originalResponse)
    {
        string[] lines = aiResult.Split(new char[] { '\n', '\r' }, System.StringSplitOptions.RemoveEmptyEntries);
        
        foreach (string line in lines)
        {
            if (line.Contains("WORD:") && line.Contains("MEANING:"))
            {
                try
                {
                    string[] parts = line.Split('|');
                    string word = parts[0].Replace("WORD:", "").Trim().ToLower();
                    string meaning = parts.Length > 1 ? parts[1].Replace("MEANING:", "").Trim() : "";
                    string context = parts.Length > 2 ? parts[2].Replace("CONTEXT:", "").Trim() : 
                                   originalResponse.Substring(0, Mathf.Min(100, originalResponse.Length));

                    if (word.Length > 1 && word.Length < 20)
                    {
                        ReviewManager.Instance.AddLearnedWord(word);
                        Debug.Log($"Enhanced extraction - Word: {word}, Meaning: {meaning}");
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogWarning($"Failed to parse word extraction line: {line}, Error: {e.Message}");
                }
            }
        }
    }

    /// <summary>
    /// Use AI to extract topics from AI response
    /// </summary>
    private async System.Threading.Tasks.Task ExtractTopicsFromResponseWithAI(string response)
    {
        if (ReviewManager.Instance == null || OpenAIManager.Instance == null) return;

        string extractionPrompt = $@"Analyze this English scene learning response and identify the main topic or situation being taught.
Return only ONE main topic/situation (like 'restaurant', 'shopping', 'airport', 'office', etc.).
Use simple, common words. If no clear scene/topic, return 'NONE'.

Response to analyze: {response}

Main topic:";

        try
        {
            var messages = new System.Collections.Generic.List<ChatMessage>
            {
                new ChatMessage { role = "user", content = extractionPrompt }
            };
            
            string aiResult = await OpenAIManager.Instance.PostRequest(messages);
            
            if (!string.IsNullOrEmpty(aiResult) && aiResult.Trim().ToUpper() != "NONE")
            {
                string topic = aiResult.Trim().ToLower();
                if (topic.Length > 1 && topic.Length < 30)
                {
                    ReviewManager.Instance.AddLearnedTopic(topic);
                    Debug.Log($"AI extracted topic for review: {topic}");
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error extracting topics with AI: {e.Message}");
            // Fallback to simple extraction
            ExtractTopicsFromResponseSimple(response);
        }
    }

    /// <summary>
    /// Use AI to extract mixed learning content from Normal mode responses
    /// Intelligently detects and categorizes vocabulary words and topics
    /// </summary>
    private async System.Threading.Tasks.Task ExtractMixedContentFromResponseWithAI(string response)
    {
        if (ReviewManager.Instance == null || OpenAIManager.Instance == null) return;

        string extractionPrompt = $@"Analyze this English tutoring response and extract any learning content.
Identify:
1. Vocabulary words being taught or explained (nouns, verbs, adjectives)
2. Topics/themes being discussed (like 'restaurant', 'travel', 'work', etc.)

Return your analysis in this exact format:
WORDS: word1, word2, word3 (or NONE if no vocabulary)
TOPICS: topic1, topic2 (or NONE if no topics)

Only include content that would be valuable for language learning review.
Maximum 3 words and 2 topics.

Response to analyze: {response}

Analysis:";

        try
        {
            var messages = new System.Collections.Generic.List<ChatMessage>
            {
                new ChatMessage { role = "user", content = extractionPrompt }
            };
            
            string aiResult = await OpenAIManager.Instance.PostRequest(messages);
            
            if (!string.IsNullOrEmpty(aiResult))
            {
                ParseMixedExtractionResult(aiResult);
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error extracting mixed content with AI: {e.Message}");
            // Fallback: try both simple extractions
            ExtractWordsFromResponseSimple(response);
            ExtractTopicsFromResponseSimple(response);
        }
    }

    /// <summary>
    /// Parse the mixed extraction result from AI
    /// </summary>
    private void ParseMixedExtractionResult(string aiResult)
    {
        if (ReviewManager.Instance == null) return;

        string[] lines = aiResult.Split(new char[] { '\n', '\r' }, System.StringSplitOptions.RemoveEmptyEntries);
        
        foreach (string line in lines)
        {
            string trimmedLine = line.Trim();
            
            // Parse words
            if (trimmedLine.StartsWith("WORDS:", System.StringComparison.OrdinalIgnoreCase))
            {
                string wordsSection = trimmedLine.Substring(6).Trim();
                if (!wordsSection.Equals("NONE", System.StringComparison.OrdinalIgnoreCase))
                {
                    string[] words = wordsSection.Split(',');
                    foreach (string word in words)
                    {
                        string cleanWord = word.Trim().ToLower();
                        if (cleanWord.Length > 1 && cleanWord.Length < 20 && !string.IsNullOrWhiteSpace(cleanWord))
                        {
                            ReviewManager.Instance.AddLearnedWord(cleanWord);
                            Debug.Log($"Normal mode AI extracted word: {cleanWord}");
                        }
                    }
                }
            }
            
            // Parse topics
            else if (trimmedLine.StartsWith("TOPICS:", System.StringComparison.OrdinalIgnoreCase))
            {
                string topicsSection = trimmedLine.Substring(7).Trim();
                if (!topicsSection.Equals("NONE", System.StringComparison.OrdinalIgnoreCase))
                {
                    string[] topics = topicsSection.Split(',');
                    foreach (string topic in topics)
                    {
                        string cleanTopic = topic.Trim().ToLower();
                        if (cleanTopic.Length > 1 && cleanTopic.Length < 30 && !string.IsNullOrWhiteSpace(cleanTopic))
                        {
                            ReviewManager.Instance.AddLearnedTopic(cleanTopic);
                            Debug.Log($"Normal mode AI extracted topic: {cleanTopic}");
                        }
                    }
                }
            }
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

    /// <summary>
    /// Fallback: Simple keyword extraction for words
    /// </summary>
    private void ExtractWordsFromResponseSimple(string response)
    {
        if (ReviewManager.Instance == null) return;

        // Simple fallback - look for quoted words or capitalized words
        var matches = System.Text.RegularExpressions.Regex.Matches(response, @"""(\w+)""|'(\w+)'|\b[A-Z][a-z]+\b");
        foreach (System.Text.RegularExpressions.Match match in matches)
        {
            string word = match.Value.Trim('"', '\'').ToLower();
            if (word.Length > 2 && word.Length < 15)
            {
                ReviewManager.Instance.AddLearnedWord(word);
                Debug.Log($"Simple extracted word: {word}");
                break; // Only add one word to avoid spam
            }
        }
    }

    /// <summary>
    /// Fallback: Simple keyword extraction for topics
    /// </summary>
    private void ExtractTopicsFromResponseSimple(string response)
    {
        if (ReviewManager.Instance == null) return;

        // Simple fallback - common scene keywords
        string[] topicKeywords = {
            "restaurant", "dining", "food", "menu", "shopping", "store", 
            "airport", "travel", "hotel", "office", "school", "hospital", "park"
        };

        string lowerResponse = response.ToLower();
        foreach (string topic in topicKeywords)
        {
            if (lowerResponse.Contains(topic))
            {
                ReviewManager.Instance.AddLearnedTopic(topic);
                Debug.Log($"Simple extracted topic: {topic}");
                return;
            }
        }
    }
}
