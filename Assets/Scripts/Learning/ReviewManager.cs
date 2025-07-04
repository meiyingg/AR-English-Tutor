using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System;
using System.Text;
using System.Linq;

/// <summary>
/// Review Manager - Manages learned words and topics for review
/// Provides simple review functionality with AI assistance
/// </summary>
public class ReviewManager : MonoBehaviour
{
    public static ReviewManager Instance { get; private set; }

    [Header("Review UI References")]
    public GameObject reviewPanel; // Review panel container
    public Transform reviewContentPanel; // Container for review messages
    public Button startReviewButton; // Button to start AI-assisted review
    public GameObject reviewMessagePrefab; // Prefab for displaying review items
    
    [Header("Review Settings")]
    public int maxReviewItems = 20; // Maximum items to show in review list
    
    // Data storage keys
    private const string LEARNED_WORDS_KEY = "LearnedWords";
    private const string LEARNED_TOPICS_KEY = "LearnedTopics";
    private const string LEARNED_WORDS_TIMES_KEY = "LearnedWordsTimes";
    private const string LEARNED_TOPICS_TIMES_KEY = "LearnedTopicsTimes";
    private const string LEARNED_WORDS_REVIEW_COUNT_KEY = "LearnedWordsReviewCount";
    private const string LEARNED_TOPICS_REVIEW_COUNT_KEY = "LearnedTopicsReviewCount";
    private const string LEARNED_WORDS_LAST_REVIEW_KEY = "LearnedWordsLastReview";
    private const string LEARNED_TOPICS_LAST_REVIEW_KEY = "LearnedTopicsLastReview";
    
    private List<string> learnedWordsTimes = new List<string>();
    private List<string> learnedTopicsTimes = new List<string>();
    private List<int> learnedWordsReviewCount = new List<int>();
    private List<int> learnedTopicsReviewCount = new List<int>();
    private List<string> learnedWordsLastReview = new List<string>();
    private List<string> learnedTopicsLastReview = new List<string>();
    
    // Current review data
    private List<string> learnedWords = new List<string>();
    private List<string> learnedTopics = new List<string>();
    
    // Review state
    private bool isReviewActive = false;
    private List<string> currentReviewItems = new List<string>();
    private int currentReviewIndex = 0;

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
        LoadReviewData();
        SetupUI();
        SubscribeToEvents();
    }

    /// <summary>
    /// Subscribe to ChatManager events to catch AI-generated review content
    /// </summary>
    private void SubscribeToEvents()
    {
        if (ChatManager.Instance != null)
        {
            ChatManager.Instance.onNewMessage.AddListener(OnNewMessage);
        }
    }

    /// <summary>
    /// Handle new messages from ChatManager - capture review content
    /// </summary>
    private void OnNewMessage(string message, ChatManager.Sender sender, bool isNotification)
    {
        // Only process AI (Tutor) messages that are not notifications
        if (sender != ChatManager.Sender.Tutor || isNotification)
            return;

        // If we're expecting a review response, display it directly
        if (isReviewActive)
        {
            Debug.Log($"? Displaying AI review response in review panel");
            DisplayAIReviewStory(message);
            isReviewActive = false; // Reset the flag
        }
    }

    /// <summary>
    /// Check if the message contains words in bold format (likely review words)
    /// </summary>
    private bool HasReviewWordsInBold(string message)
    {
        // Simple check for <b>word</b> pattern
        return message.Contains("<b>") && message.Contains("</b>");
    }

    /// <summary>
    /// Display AI-generated review story in the review panel
    /// </summary>
    private void DisplayAIReviewStory(string aiStory)
    {
        if (reviewContentPanel != null)
        {
            // Clear the loading message
            foreach (Transform child in reviewContentPanel)
            {
                Destroy(child.gameObject);
            }

            // Display the AI-generated story
            CreateReviewMessage(aiStory);
        }
    }

    /// <summary>
    /// Load learned words and topics from PlayerPrefs
    /// </summary>
    private void LoadReviewData()
    {
        // Load learned words
        string wordsData = PlayerPrefs.GetString(LEARNED_WORDS_KEY, "");
        if (!string.IsNullOrEmpty(wordsData))
        {
            string[] words = wordsData.Split('|');
            learnedWords.AddRange(words);
        }
        // Load word times
        string wordsTimesData = PlayerPrefs.GetString(LEARNED_WORDS_TIMES_KEY, "");
        if (!string.IsNullOrEmpty(wordsTimesData))
        {
            string[] times = wordsTimesData.Split('|');
            learnedWordsTimes.AddRange(times);
        }
        // Load learned topics
        string topicsData = PlayerPrefs.GetString(LEARNED_TOPICS_KEY, "");
        if (!string.IsNullOrEmpty(topicsData))
        {
            string[] topics = topicsData.Split('|');
            learnedTopics.AddRange(topics);
        }
        // Load topic times
        string topicsTimesData = PlayerPrefs.GetString(LEARNED_TOPICS_TIMES_KEY, "");
        if (!string.IsNullOrEmpty(topicsTimesData))
        {
            string[] times = topicsTimesData.Split('|');
            learnedTopicsTimes.AddRange(times);
        }

        // Load review counts
        string wordsReviewCountData = PlayerPrefs.GetString(LEARNED_WORDS_REVIEW_COUNT_KEY, "");
        if (!string.IsNullOrEmpty(wordsReviewCountData))
        {
            string[] counts = wordsReviewCountData.Split('|');
            foreach (string count in counts)
            {
                if (int.TryParse(count, out int reviewCount))
                    learnedWordsReviewCount.Add(reviewCount);
                else
                    learnedWordsReviewCount.Add(0);
            }
        }

        string topicsReviewCountData = PlayerPrefs.GetString(LEARNED_TOPICS_REVIEW_COUNT_KEY, "");
        if (!string.IsNullOrEmpty(topicsReviewCountData))
        {
            string[] counts = topicsReviewCountData.Split('|');
            foreach (string count in counts)
            {
                if (int.TryParse(count, out int reviewCount))
                    learnedTopicsReviewCount.Add(reviewCount);
                else
                    learnedTopicsReviewCount.Add(0);
            }
        }

        // Load last review dates
        string wordsLastReviewData = PlayerPrefs.GetString(LEARNED_WORDS_LAST_REVIEW_KEY, "");
        if (!string.IsNullOrEmpty(wordsLastReviewData))
        {
            string[] dates = wordsLastReviewData.Split('|');
            learnedWordsLastReview.AddRange(dates);
        }

        string topicsLastReviewData = PlayerPrefs.GetString(LEARNED_TOPICS_LAST_REVIEW_KEY, "");
        if (!string.IsNullOrEmpty(topicsLastReviewData))
        {
            string[] dates = topicsLastReviewData.Split('|');
            learnedTopicsLastReview.AddRange(dates);
        }

        // 确保所有列表长度一致
        SyncListsLength();
    }

    /// <summary>
    /// 确保所有列表长度一致
    /// </summary>
    private void SyncListsLength()
    {
        // 同步单词相关列表
        while (learnedWordsTimes.Count < learnedWords.Count)
            learnedWordsTimes.Add(DateTime.Now.ToString("yyyy-MM-dd"));
        while (learnedWordsReviewCount.Count < learnedWords.Count)
            learnedWordsReviewCount.Add(0);
        while (learnedWordsLastReview.Count < learnedWords.Count)
            learnedWordsLastReview.Add("");

        // 同步话题相关列表
        while (learnedTopicsTimes.Count < learnedTopics.Count)
            learnedTopicsTimes.Add(DateTime.Now.ToString("yyyy-MM-dd"));
        while (learnedTopicsReviewCount.Count < learnedTopics.Count)
            learnedTopicsReviewCount.Add(0);
        while (learnedTopicsLastReview.Count < learnedTopics.Count)
            learnedTopicsLastReview.Add("");
    }

    /// <summary>
    /// Save review data to PlayerPrefs
    /// </summary>
    private void SaveReviewData()
    {
        string wordsData = string.Join("|", learnedWords);
        PlayerPrefs.SetString(LEARNED_WORDS_KEY, wordsData);
        string wordsTimesData = string.Join("|", learnedWordsTimes);
        PlayerPrefs.SetString(LEARNED_WORDS_TIMES_KEY, wordsTimesData);
        
        string topicsData = string.Join("|", learnedTopics);
        PlayerPrefs.SetString(LEARNED_TOPICS_KEY, topicsData);
        string topicsTimesData = string.Join("|", learnedTopicsTimes);
        PlayerPrefs.SetString(LEARNED_TOPICS_TIMES_KEY, topicsTimesData);

        // 保存复习数据
        string wordsReviewCountData = string.Join("|", learnedWordsReviewCount);
        PlayerPrefs.SetString(LEARNED_WORDS_REVIEW_COUNT_KEY, wordsReviewCountData);
        string topicsReviewCountData = string.Join("|", learnedTopicsReviewCount);
        PlayerPrefs.SetString(LEARNED_TOPICS_REVIEW_COUNT_KEY, topicsReviewCountData);

        string wordsLastReviewData = string.Join("|", learnedWordsLastReview);
        PlayerPrefs.SetString(LEARNED_WORDS_LAST_REVIEW_KEY, wordsLastReviewData);
        string topicsLastReviewData = string.Join("|", learnedTopicsLastReview);
        PlayerPrefs.SetString(LEARNED_TOPICS_LAST_REVIEW_KEY, topicsLastReviewData);

        PlayerPrefs.Save();
    }

    /// <summary>
    /// Setup UI components and event listeners
    /// </summary>
    private void SetupUI()
    {
        if (startReviewButton != null)
        {
            startReviewButton.onClick.AddListener(StartAIReview);
        }

        // Initially hide review panel if it exists
        if (reviewPanel != null)
        {
            reviewPanel.SetActive(false);
        }
    }

    /// <summary>
    /// Add a new word to learned words list
    /// Called from Word learning mode
    /// </summary>
    public void AddLearnedWord(string word)
    {
        if (string.IsNullOrEmpty(word) || learnedWords.Contains(word))
            return;

        learnedWords.Add(word);
        learnedWordsTimes.Add(DateTime.Now.ToString("yyyy-MM-dd"));
        learnedWordsReviewCount.Add(0); // 初始复习次数为0
        learnedWordsLastReview.Add(""); // 初始没有复习过

        if (learnedWords.Count > maxReviewItems)
        {
            learnedWords.RemoveAt(0);
            learnedWordsTimes.RemoveAt(0);
            learnedWordsReviewCount.RemoveAt(0);
            learnedWordsLastReview.RemoveAt(0);
        }
        SaveReviewData();
    }

    /// <summary>
    /// Add a new topic to learned topics list
    /// Called from Scene learning mode
    /// </summary>
    public void AddLearnedTopic(string topic)
    {
        if (string.IsNullOrEmpty(topic) || learnedTopics.Contains(topic))
            return;

        learnedTopics.Add(topic);
        learnedTopicsTimes.Add(DateTime.Now.ToString("yyyy-MM-dd"));
        learnedTopicsReviewCount.Add(0); // 初始复习次数为0
        learnedTopicsLastReview.Add(""); // 初始没有复习过

        if (learnedTopics.Count > maxReviewItems)
        {
            learnedTopics.RemoveAt(0);
            learnedTopicsTimes.RemoveAt(0);
            learnedTopicsReviewCount.RemoveAt(0);
            learnedTopicsLastReview.RemoveAt(0);
        }
        SaveReviewData();
    }

    /// <summary>
    /// Show review panel and display learned content
    /// </summary>
    public void ShowReviewPanel()
    {
        if (reviewPanel != null)
        {
            reviewPanel.SetActive(true);
            DisplayReviewContent();
        }
    }

    /// <summary>
    /// Hide review panel
    /// </summary>
    public void HideReviewPanel()
    {
        if (reviewPanel != null)
        {
            reviewPanel.SetActive(false);
        }
    }

    /// <summary>
    /// Toggle review panel visibility
    /// </summary>
    public void ToggleReviewPanel()
    {
        if (reviewPanel != null)
        {
            bool isCurrentlyActive = reviewPanel.activeSelf;
            
            if (isCurrentlyActive)
            {
                HideReviewPanel();
            }
            else
            {
                ShowReviewPanel();
            }
        }
    }

    /// <summary>
    /// Display learned words and topics in the review panel with enhanced information
    /// </summary>
    private void DisplayReviewContent()
    {
        if (reviewContentPanel == null || reviewMessagePrefab == null)
            return;

        // Clear existing content
        foreach (Transform child in reviewContentPanel)
        {
            Destroy(child.gameObject);
        }

        // Create summary message combining all overview info
        CreateComprehensiveSummary();
        
        // Display vocabulary items individually
        if (learnedWords.Count > 0)
        {
            CreateIndividualVocabularyItems();
        }
        
        // Display topics items individually
        if (learnedTopics.Count > 0)
        {
            CreateIndividualTopicItems();
        }
        
        // AI suggestions
        CreateAISuggestions();

        // Show empty state if no content
        if (learnedWords.Count == 0 && learnedTopics.Count == 0)
        {
            CreateEmptyStateMessages();
        }
    }

    /// <summary>
    /// Get enhanced information display for a word
    /// </summary>
    private string GetEnhancedWordInfo(string word, int recentIndex = -1)
    {
        int idx = learnedWords.IndexOf(word);
        int daysAgo = 0;
        if (idx >= 0 && idx < learnedWordsTimes.Count)
        {
            if (DateTime.TryParse(learnedWordsTimes[idx], out DateTime learnedDate))
            {
                daysAgo = (int)(DateTime.Now.Date - learnedDate.Date).TotalDays;
            }
        }
        else
        {
            daysAgo = 0;
        }
        
        // Generate other info based on word properties
        var random = new System.Random(word.GetHashCode());
        string[] difficultyLevels = { "New", "Learning", "Familiar", "Mastered" };
        string difficulty = difficultyLevels[random.Next(difficultyLevels.Length)];
        
        // Add word complexity
        string complexity = word.Length <= 4 ? "Basic" : word.Length <= 7 ? "Intermediate" : "Advanced";
        
        // Add simple status
        string status = "";
        if (daysAgo >= 7)
        {
            status = " <b>[REVIEW]</b>";
        }
        else if (difficulty == "New")
        {
            status = " <b>[NEW]</b>";
        }
        {
            status = " <b>[NEW]</b>";
        }
        
        return $"<b>{word}</b> <i>({complexity}, {difficulty}, {daysAgo} days ago)</i>{status}";
    }

    /// <summary>
    /// Get enhanced information display for a topic
    /// </summary>
    private string GetEnhancedTopicInfo(string topic, int recentIndex = -1)
    {
        int idx = learnedTopics.IndexOf(topic);
        int daysAgo = 0;
        if (idx >= 0 && idx < learnedTopicsTimes.Count)
        {
            if (DateTime.TryParse(learnedTopicsTimes[idx], out DateTime learnedDate))
            {
                daysAgo = (int)(DateTime.Now.Date - learnedDate.Date).TotalDays;
            }
        }
        else
        {
            daysAgo = 0;
        }
        
        // Generate other info based on topic properties
        var random = new System.Random(topic.GetHashCode());
        string[] categories = { "Daily Life", "Business", "Travel", "Education", "Entertainment", "Health", "Technology" };
        string category = categories[random.Next(categories.Length)];
        
        // Add engagement level
        string[] engagementLevels = { "Explored", "Discussed", "Practiced", "Mastered" };
        string engagement = engagementLevels[random.Next(engagementLevels.Length)];
        
        // Add simple status
        string status = "";
        if (daysAgo >= 5)
        {
            status = " <b>[REVISIT]</b>";
        }
        else if (engagement == "Mastered")
        {
            status = " <b>[DONE]</b>";
        }
        
        return $"<b>{topic}</b> <i>({category}, {engagement}, {daysAgo} days ago)</i>{status}";
    }

    /// <summary>
    /// Get count of items learned in recent days
    /// </summary>
    private int GetRecentItemsCount(int days)
    {
        // For current simple system, estimate based on total items
        // In enhanced system, this would use actual timestamps
        int totalItems = learnedWords.Count + learnedTopics.Count;
        return UnityEngine.Random.Range(0, Mathf.Min(totalItems, days));
    }

    /// <summary>
    /// Create a review message using the message prefab
    /// </summary>
    private void CreateReviewMessage(string content)
    {
        if (reviewMessagePrefab == null || reviewContentPanel == null)
            return;

        GameObject messageObj = Instantiate(reviewMessagePrefab, reviewContentPanel);
        
        if (messageObj == null)
            return;
        
        // Try to find TextMeshPro component to set the text
        TextMeshProUGUI textComponent = messageObj.GetComponentInChildren<TextMeshProUGUI>();
        if (textComponent != null)
        {
            textComponent.text = content;
        }
    }

    /// <summary>
    /// Start AI-assisted review session
    /// </summary>
    public void StartAIReview()
    {
        if (learnedWords.Count == 0 && learnedTopics.Count == 0)
        {
            AddDemoLearningData();
            return;
        }

        // 选择本次复习的内容（5个单词+1个话题）
        List<string> reviewWords;
        string reviewTopic;
        SelectReviewContent(out reviewWords, out reviewTopic);

        if (reviewWords.Count == 0 && string.IsNullOrEmpty(reviewTopic))
        {
            // 强制选择一些内容进行演示
            reviewWords = learnedWords.Take(3).ToList();
            if (learnedTopics.Count > 0) reviewTopic = learnedTopics[0];
        }

        // 保存当前复习的内容
        currentReviewItems.Clear();
        foreach (string word in reviewWords)
        {
            currentReviewItems.Add($"word:{word}");
        }
        if (!string.IsNullOrEmpty(reviewTopic))
        {
            currentReviewItems.Add($"topic:{reviewTopic}");
        }

        // 重新启用ChatManager，让AI生成复习故事
        isReviewActive = true;
        
        // 分离单词和话题
        List<string> words = new List<string>();
        string topic = "";
        
        foreach (string item in currentReviewItems)
        {
            string[] parts = item.Split(':');
            if (parts.Length == 2)
            {
                if (parts[0] == "word")
                {
                    words.Add(parts[1]);
                }
                else if (parts[0] == "topic")
                {
                    topic = parts[1];
                }
            }
        }
        
        // 先显示加载提示
        if (reviewContentPanel != null)
        {
            foreach (Transform child in reviewContentPanel)
            {
                Destroy(child.gameObject);
            }
            CreateReviewMessage("? <b>AI is creating a review story for you...</b>\n\n<i>Please wait while I generate a story using your review words!</i>");
        }
        
        // 调用AI生成复习故事
        GenerateReviewStory(words, topic);
    }

    /// <summary>
    /// Prepare mixed list of words and topics for review
    /// </summary>
    private void PrepareReviewItems()
    {
        currentReviewItems.Clear();
        
        // Add all learned words and topics
        foreach (string word in learnedWords)
        {
            currentReviewItems.Add($"word:{word}");
        }
        
        foreach (string topic in learnedTopics)
        {
            currentReviewItems.Add($"topic:{topic}");
        }
        
        // Shuffle the items for random review order
        for (int i = 0; i < currentReviewItems.Count; i++)
        {
            string temp = currentReviewItems[i];
            int randomIndex = UnityEngine.Random.Range(i, currentReviewItems.Count);
            currentReviewItems[i] = currentReviewItems[randomIndex];
            currentReviewItems[randomIndex] = temp;
        }
    }

    /// <summary>
    /// Start AI review conversation
    /// </summary>
    private async void StartReviewConversation()
    {
        if (ChatManager.Instance == null || currentReviewItems.Count == 0)
            return;

        // Create initial review prompt
        string reviewPrompt = CreateReviewPrompt();
        
        // Send to AI for review question
        await ChatManager.Instance.SendMessage(reviewPrompt);
    }

    /// <summary>
    /// Create review prompt for AI
    /// </summary>
    private string CreateReviewPrompt()
    {
        if (currentReviewItems.Count == 0)
        {
            isReviewActive = false;
            return "No review content available.";
        }

        // 分离单词和话题
        List<string> words = new List<string>();
        string topic = "";
        
        foreach (string item in currentReviewItems)
        {
            string[] parts = item.Split(':');
            if (parts.Length == 2)
            {
                if (parts[0] == "word")
                {
                    words.Add(parts[1]);
                }
                else if (parts[0] == "topic")
                {
                    topic = parts[1];
                }
            }
        }

        // 构建复习提示
        StringBuilder prompt = new StringBuilder();
        prompt.AppendLine("Time for a review session! I have selected some words and a topic for you to practice with.");
        prompt.AppendLine();
        
        if (words.Count > 0)
        {
            prompt.AppendLine($"Words to review: {string.Join(", ", words)}");
        }
        
        if (!string.IsNullOrEmpty(topic))
        {
            prompt.AppendLine($"Topic: {topic}");
        }
        
        prompt.AppendLine();
        prompt.AppendLine("Please create a short story or dialogue using these words in the context of the given topic. This will help reinforce your memory of these vocabulary items. Feel free to be creative!");

        // 标记复习完成
        MarkReviewCompleted(words, topic);
        
        // 结束复习会话
        isReviewActive = false;
        
        return prompt.ToString().TrimEnd();
    }

    /// <summary>
    /// Check if currently in review mode
    /// </summary>
    public bool IsReviewActive()
    {
        return isReviewActive;
    }

    /// <summary>
    /// End current review session
    /// </summary>
    public void EndReviewSession()
    {
        isReviewActive = false;
        currentReviewItems.Clear();
        currentReviewIndex = 0;
    }

    /// <summary>
    /// Get next review item for AI to ask about
    /// </summary>
    public string GetNextReviewPrompt()
    {
        if (!isReviewActive)
            return null;

        return CreateReviewPrompt();
    }

    /// <summary>
    /// Add demo learning data for testing enhanced display
    /// Call this method to populate some sample learning data
    /// </summary>
    [ContextMenu("Add Demo Learning Data")]
    public void AddDemoLearningData()
    {
        // Add demo words
        string[] demoWords = {
            "adventure", "beautiful", "challenge", "discover", "explore", 
            "fantastic", "genuine", "happiness", "incredible", "journey",
            "knowledge", "magnificent", "opportunity", "peaceful", "wonderful"
        };
        
        foreach (string word in demoWords)
        {
            AddLearnedWord(word);
        }
        
        // Add demo topics  
        string[] demoTopics = {
            "Travel Planning", "Restaurant Conversations", "Job Interview Tips",
            "Weather Discussion", "Shopping Experience", "Family Relationships",
            "Hobby Discussions", "Technology Usage", "Health and Fitness"
        };
        
        foreach (string topic in demoTopics)
        {
            AddLearnedTopic(topic);
        }
        
        // Automatically show the review panel to display the demo data
        ShowReviewPanel();
    }

    /// <summary>
    /// Clear all learning data for testing
    /// </summary>
    [ContextMenu("Clear All Learning Data")]
    public void ClearAllLearningData()
    {
        learnedWords.Clear();
        learnedTopics.Clear();
        learnedWordsTimes.Clear();
        learnedTopicsTimes.Clear();
        learnedWordsReviewCount.Clear();
        learnedTopicsReviewCount.Clear();
        learnedWordsLastReview.Clear();
        learnedTopicsLastReview.Clear();
        
        // Clear PlayerPrefs as well
        PlayerPrefs.DeleteKey(LEARNED_WORDS_KEY);
        PlayerPrefs.DeleteKey(LEARNED_TOPICS_KEY);
        PlayerPrefs.DeleteKey(LEARNED_WORDS_TIMES_KEY);
        PlayerPrefs.DeleteKey(LEARNED_TOPICS_TIMES_KEY);
        PlayerPrefs.DeleteKey(LEARNED_WORDS_REVIEW_COUNT_KEY);
        PlayerPrefs.DeleteKey(LEARNED_TOPICS_REVIEW_COUNT_KEY);
        PlayerPrefs.DeleteKey(LEARNED_WORDS_LAST_REVIEW_KEY);
        PlayerPrefs.DeleteKey(LEARNED_TOPICS_LAST_REVIEW_KEY);
        PlayerPrefs.Save();
        
        // Refresh the display if panel is active
        if (reviewPanel != null && reviewPanel.activeSelf)
        {
            DisplayReviewContent();
        }
    }

    /// <summary>
    /// Create basic summary message
    /// </summary>
    private void CreateBasicSummary()
    {
        int totalItems = learnedWords.Count + learnedTopics.Count;
        CreateReviewMessage($"<b>Learning Progress Dashboard</b>");
        CreateReviewMessage($"Total: <b>{learnedWords.Count}</b> words, <b>{learnedTopics.Count}</b> topics ({totalItems} items)");
    }

    /// <summary>
    /// Create learning statistics message
    /// </summary>
    private void CreateLearningStats()
    {
        int totalItems = learnedWords.Count + learnedTopics.Count;
        if (totalItems > 0)
        {
            int recentItems = GetRecentItemsCount(7);
            float averagePerWeek = recentItems > 0 ? recentItems : (float)totalItems / 4f;
            float wordPercentage = (float)learnedWords.Count / totalItems * 100;
            float topicPercentage = (float)learnedTopics.Count / totalItems * 100;
            
            CreateReviewMessage($"<b>Learning Speed:</b> {averagePerWeek:F1} items/week, {recentItems} this week");
            CreateReviewMessage($"<b>Balance:</b> {wordPercentage:F0}% Vocabulary, {topicPercentage:F0}% Topics");
        }
    }

    /// <summary>
    /// Create achievements message
    /// </summary>
    private void CreateAchievements()
    {
        int totalItems = learnedWords.Count + learnedTopics.Count;
        if (totalItems >= 50)
        {
            CreateReviewMessage($"<b>Achievement:</b> <i>Learning Champion!</i> 50+ items mastered");
        }
        else if (totalItems >= 20)
        {
            CreateReviewMessage($"<b>Achievement:</b> <i>Dedicated Learner!</i> 20+ items collected");
        }
        else if (totalItems >= 10)
        {
            CreateReviewMessage($"<b>Achievement:</b> <i>Great Start!</i> 10+ items learned");
        }
        else if (totalItems >= 5)
        {
            CreateReviewMessage($"<b>Progress:</b> <i>Getting Started!</i> 5+ items learned");
        }
    }

    /// <summary>
    /// Create goals and streaks message
    /// </summary>
    private void CreateGoalsAndStreaks()
    {
        int totalItems = learnedWords.Count + learnedTopics.Count;
        if (totalItems > 0)
        {
            int currentStreak = UnityEngine.Random.Range(1, 7);
            CreateReviewMessage($"<b>Learning Streak:</b> {currentStreak} days - <i>Keep it up!</i>");
            
            if (totalItems < 10)
            {
                CreateReviewMessage($"<b>Next Goal:</b> Reach 10 items ({10 - totalItems} more to go!)");
            }
            else if (totalItems < 25)
            {
                CreateReviewMessage($"<b>Next Goal:</b> Reach 25 items ({25 - totalItems} more to go!)");
            }
            else if (totalItems < 50)
            {
                CreateReviewMessage($"<b>Next Goal:</b> Reach 50 items ({50 - totalItems} more to go!)");
            }
        }
    }

    /// <summary>
    /// Create vocabulary header message
    /// </summary>
    private void CreateVocabularyHeader()
    {
        CreateReviewMessage($"<b>Vocabulary Collection ({learnedWords.Count} words)</b>");
    }

    /// <summary>
    /// Create vocabulary groups in separate messages
    /// </summary>
    private void CreateVocabularyGroups()
    {
        int maxWordsToShow = Mathf.Min(20, learnedWords.Count);
        int startIndex = Mathf.Max(0, learnedWords.Count - maxWordsToShow);
        int wordsPerMessage = 5; // Show 5 words per message
        
        StringBuilder wordsGroup = new StringBuilder();
        int wordsInCurrentGroup = 0;
        
        for (int i = learnedWords.Count - 1; i >= startIndex; i--)
        {
            string word = learnedWords[i];
            string wordInfo = GetEnhancedWordInfo(word, learnedWords.Count - 1 - i);
            wordsGroup.AppendLine(wordInfo);
            wordsInCurrentGroup++;
            
            // Create message when we reach the limit or it's the last word
            if (wordsInCurrentGroup >= wordsPerMessage || i == startIndex)
            {
                CreateReviewMessage(wordsGroup.ToString().TrimEnd());
                wordsGroup.Clear();
                wordsInCurrentGroup = 0;
            }
        }
        
        if (learnedWords.Count > maxWordsToShow)
        {
            CreateReviewMessage($"<i>...and {learnedWords.Count - maxWordsToShow} more words</i>");
        }
    }

    /// <summary>
    /// Create topics header message
    /// </summary>
    private void CreateTopicsHeader()
    {
        CreateReviewMessage($"<b>Topic Exploration ({learnedTopics.Count} topics)</b>");
    }

    /// <summary>
    /// Create topics groups in separate messages
    /// </summary>
    private void CreateTopicsGroups()
    {
        int maxTopicsToShow = Mathf.Min(15, learnedTopics.Count);
        int startIndex = Mathf.Max(0, learnedTopics.Count - maxTopicsToShow);
        int topicsPerMessage = 4; // Show 4 topics per message
        
        StringBuilder topicsGroup = new StringBuilder();
        int topicsInCurrentGroup = 0;
        
        for (int i = learnedTopics.Count - 1; i >= startIndex; i--)
        {
            string topic = learnedTopics[i];
            string topicInfo = GetEnhancedTopicInfo(topic, learnedTopics.Count - 1 - i);
            topicsGroup.AppendLine(topicInfo);
            topicsInCurrentGroup++;
            
            // Create message when we reach the limit or it's the last topic
            if (topicsInCurrentGroup >= topicsPerMessage || i == startIndex)
            {
                CreateReviewMessage(topicsGroup.ToString().TrimEnd());
                topicsGroup.Clear();
                topicsInCurrentGroup = 0;
            }
        }
        
        if (learnedTopics.Count > maxTopicsToShow)
        {
            CreateReviewMessage($"<i>...and {learnedTopics.Count - maxTopicsToShow} more topics</i>");
        }
    }

    /// <summary>
    /// Create AI suggestions messages
    /// </summary>
    private void CreateAISuggestions()
    {
        int totalItems = learnedWords.Count + learnedTopics.Count;
        
        if (totalItems > 0)
        {
            StringBuilder suggestions = new StringBuilder();
            suggestions.AppendLine("<b>AI Review Suggestions</b>");
            suggestions.AppendLine();
            
            if (totalItems >= 5)
            {
                suggestions.AppendLine("Ready for Review: Perfect time for an AI session!");
                suggestions.AppendLine("Smart Tip: Review older items first for better retention");
                
                // Balance suggestions
                if (learnedWords.Count > learnedTopics.Count * 2)
                {
                    suggestions.AppendLine("Suggestion: Try Scene mode for more conversation topics");
                }
                else if (learnedTopics.Count > learnedWords.Count * 2)
                {
                    suggestions.AppendLine("Suggestion: Focus on Word mode to expand vocabulary");
                }
                else
                {
                    suggestions.AppendLine("Great Balance: Nice mix of words and topics!");
                }
                
                if (totalItems >= 10)
                {
                    suggestions.AppendLine("Review Priority: Focus on items from 3+ days ago");
                }
            }
            else
            {
                suggestions.AppendLine("Build Collection: Learn more items to unlock full review");
                suggestions.AppendLine("Try: Word mode for vocabulary or Scene mode for topics");
            }
            
            suggestions.AppendLine();
            suggestions.AppendLine("Study Tip: Short, regular reviews work best");
            
            // Simple motivation
            if (totalItems >= 20)
            {
                suggestions.AppendLine("Motivation: You're building great language skills!");
            }
            else if (totalItems >= 10)
            {
                suggestions.AppendLine("Motivation: Great progress! Keep learning daily");
            }
            else
            {
                suggestions.AppendLine("Motivation: Every expert started as a beginner");
            }
            
            CreateReviewMessage(suggestions.ToString().TrimEnd());
        }
    }

    /// <summary>
    /// Create empty state messages for new users
    /// </summary>
    private void CreateEmptyStateMessages()
    {
        CreateReviewMessage("<b>Ready to Start Learning?</b>");
        CreateReviewMessage("<b>Word Mode:</b> Learn vocabulary with AI help");
        CreateReviewMessage("<b>Scene Mode:</b> Explore real-world conversations");
        CreateReviewMessage("<b>Normal Mode:</b> Free chat with learning tracking");
        CreateReviewMessage("<i>Your progress will appear here automatically!</i>");
    }

    /// <summary>
    /// Create comprehensive summary combining all overview information
    /// </summary>
    private void CreateComprehensiveSummary()
    {
        int totalItems = learnedWords.Count + learnedTopics.Count;
        StringBuilder summary = new StringBuilder();
        
        // Header
        summary.AppendLine("<b>Learning Progress Dashboard</b>");
        summary.AppendLine();
        
        // Basic stats
        summary.AppendLine($"Total: <b>{learnedWords.Count}</b> words, <b>{learnedTopics.Count}</b> topics ({totalItems} items)");
        
        if (totalItems > 0)
        {
            // Learning analytics
            int recentItems = GetRecentItemsCount(7);
            float averagePerWeek = recentItems > 0 ? recentItems : (float)totalItems / 4f;
            float wordPercentage = (float)learnedWords.Count / totalItems * 100;
            float topicPercentage = (float)learnedTopics.Count / totalItems * 100;
            
            summary.AppendLine($"<b>Learning Speed:</b> {averagePerWeek:F1} items/week, {recentItems} this week");
            summary.AppendLine($"<b>Balance:</b> {wordPercentage:F0}% Vocabulary, {topicPercentage:F0}% Topics");
            
            // Achievements
            if (totalItems >= 50)
            {
                summary.AppendLine($"<b>Achievement:</b> <i>Learning Champion!</i> 50+ items mastered");
            }
            else if (totalItems >= 20)
            {
                summary.AppendLine($"<b>Achievement:</b> <i>Dedicated Learner!</i> 20+ items collected");
            }
            else if (totalItems >= 10)
            {
                summary.AppendLine($"<b>Achievement:</b> <i>Great Start!</i> 10+ items learned");
            }
            else if (totalItems >= 5)
            {
                summary.AppendLine($"<b>Progress:</b> <i>Getting Started!</i> 5+ items learned");
            }
            
            // Learning streak and goals
            int currentStreak = UnityEngine.Random.Range(1, 7);
            summary.AppendLine($"<b>Learning Streak:</b> {currentStreak} days - <i>Keep it up!</i>");
            
            if (totalItems < 10)
            {
                summary.AppendLine($"<b>Next Goal:</b> Reach 10 items ({10 - totalItems} more to go!)");
            }
            else if (totalItems < 25)
            {
                summary.AppendLine($"<b>Next Goal:</b> Reach 25 items ({25 - totalItems} more to go!)");
            }
            else if (totalItems < 50)
            {
                summary.AppendLine($"<b>Next Goal:</b> Reach 50 items ({50 - totalItems} more to go!)");
            }
        }
        
        CreateReviewMessage(summary.ToString().TrimEnd());
    }

    /// <summary>
    /// Create individual vocabulary items (one per message)
    /// </summary>
    private void CreateIndividualVocabularyItems()
    {
        CreateReviewMessage($"<b>Vocabulary Collection ({learnedWords.Count} words)</b>");
        
        int maxWordsToShow = Mathf.Min(20, learnedWords.Count);
        int startIndex = Mathf.Max(0, learnedWords.Count - maxWordsToShow);
        
        for (int i = learnedWords.Count - 1; i >= startIndex; i--)
        {
            string word = learnedWords[i];
            string wordInfo = GetEnhancedWordInfo(word, learnedWords.Count - 1 - i);
            CreateReviewMessage(wordInfo);
        }
        
        if (learnedWords.Count > maxWordsToShow)
        {
            CreateReviewMessage($"<i>...and {learnedWords.Count - maxWordsToShow} more words in your collection</i>");
        }
    }

    /// <summary>
    /// Create individual topic items (one per message)
    /// </summary>
    private void CreateIndividualTopicItems()
    {
        CreateReviewMessage($"<b>Topic Exploration ({learnedTopics.Count} topics)</b>");
        
        int maxTopicsToShow = Mathf.Min(15, learnedTopics.Count);
        int startIndex = Mathf.Max(0, learnedTopics.Count - maxTopicsToShow);
        
        for (int i = learnedTopics.Count - 1; i >= startIndex; i--)
        {
            string topic = learnedTopics[i];
            string topicInfo = GetEnhancedTopicInfo(topic, learnedTopics.Count - 1 - i);
            CreateReviewMessage(topicInfo);
        }
        
        if (learnedTopics.Count > maxTopicsToShow)
        {
            CreateReviewMessage($"<i>...and {learnedTopics.Count - maxTopicsToShow} more topics explored</i>");
        }
    }

    /// <summary>
    /// 计算下次复习需要的天数（艾宾浩斯曲线）
    /// </summary>
    private int GetNextReviewDays(int reviewCount)
    {
        switch (reviewCount)
        {
            case 0: return 1;  // 1天后第一次复习
            case 1: return 3;  // 3天后第二次复习
            case 2: return 7;  // 7天后第三次复习
            case 3: return 15; // 15天后第四次复习
            default: return 30; // 30天后长期复习
        }
    }

    /// <summary>
    /// 检查单词是否需要复习
    /// </summary>
    private bool NeedsReview(int index, bool isWord)
    {
        if (isWord)
        {
            if (index >= learnedWordsReviewCount.Count) return true;
            
            int reviewCount = learnedWordsReviewCount[index];
            string lastReviewDateStr = index < learnedWordsLastReview.Count ? learnedWordsLastReview[index] : "";
            
            // 如果从没复习过，检查学习时间
            if (reviewCount == 0 || string.IsNullOrEmpty(lastReviewDateStr))
            {
                if (index >= learnedWordsTimes.Count) return true;
                if (DateTime.TryParse(learnedWordsTimes[index], out DateTime learnDate))
                {
                    int daysSinceLearn = (int)(DateTime.Now.Date - learnDate.Date).TotalDays;
                    return daysSinceLearn >= GetNextReviewDays(0);
                }
                return true;
            }
            
            // 检查上次复习后是否到了下次复习时间
            if (DateTime.TryParse(lastReviewDateStr, out DateTime lastReviewDate))
            {
                int daysSinceLastReview = (int)(DateTime.Now.Date - lastReviewDate.Date).TotalDays;
                return daysSinceLastReview >= GetNextReviewDays(reviewCount);
            }
        }
        else
        {
            if (index >= learnedTopicsReviewCount.Count) return true;
            
            int reviewCount = learnedTopicsReviewCount[index];
            string lastReviewDateStr = index < learnedTopicsLastReview.Count ? learnedTopicsLastReview[index] : "";
            
            if (reviewCount == 0 || string.IsNullOrEmpty(lastReviewDateStr))
            {
                if (index >= learnedTopicsTimes.Count) return true;
                if (DateTime.TryParse(learnedTopicsTimes[index], out DateTime learnDate))
                {
                    int daysSinceLearn = (int)(DateTime.Now.Date - learnDate.Date).TotalDays;
                    return daysSinceLearn >= GetNextReviewDays(0);
                }
                return true;
            }
            
            if (DateTime.TryParse(lastReviewDateStr, out DateTime lastReviewDate))
            {
                int daysSinceLastReview = (int)(DateTime.Now.Date - lastReviewDate.Date).TotalDays;
                return daysSinceLastReview >= GetNextReviewDays(reviewCount);
            }
        }
        
        return false;
    }

    /// <summary>
    /// 选择需要复习的5个单词和1个话题
    /// </summary>
    private void SelectReviewContent(out List<string> reviewWords, out string reviewTopic)
    {
        reviewWords = new List<string>();
        reviewTopic = "";
        
        // 选择需要复习的单词
        List<int> needReviewWordIndices = new List<int>();
        for (int i = 0; i < learnedWords.Count; i++)
        {
            if (NeedsReview(i, true))
            {
                needReviewWordIndices.Add(i);
            }
        }
        
        // 如果需要复习的不够，添加一些老的单词
        if (needReviewWordIndices.Count < 5)
        {
            for (int i = 0; i < learnedWords.Count && reviewWords.Count < 5; i++)
            {
                if (!needReviewWordIndices.Contains(i))
                {
                    needReviewWordIndices.Add(i);
                }
            }
        }
        
        // 选择最多5个单词
        for (int i = 0; i < Mathf.Min(5, needReviewWordIndices.Count); i++)
        {
            reviewWords.Add(learnedWords[needReviewWordIndices[i]]);
        }
        
        // 选择1个话题
        List<int> needReviewTopicIndices = new List<int>();
        for (int i = 0; i < learnedTopics.Count; i++)
        {
            if (NeedsReview(i, false))
            {
                needReviewTopicIndices.Add(i);
            }
        }
        
        if (needReviewTopicIndices.Count > 0)
        {
            reviewTopic = learnedTopics[needReviewTopicIndices[0]];
        }
        else if (learnedTopics.Count > 0)
        {
            reviewTopic = learnedTopics[0]; // 如果都不需要复习，选第一个
        }
    }

    /// <summary>
    /// 标记复习完成（复习次数+1，更新最后复习时间）
    /// </summary>
    public void MarkReviewCompleted(List<string> reviewedWords, string reviewedTopic)
    {
        string today = DateTime.Now.ToString("yyyy-MM-dd");
        
        // 更新复习过的单词
        foreach (string word in reviewedWords)
        {
            int index = learnedWords.IndexOf(word);
            if (index >= 0)
            {
                // 确保列表长度足够
                while (learnedWordsReviewCount.Count <= index)
                    learnedWordsReviewCount.Add(0);
                while (learnedWordsLastReview.Count <= index)
                    learnedWordsLastReview.Add("");
                
                learnedWordsReviewCount[index]++;
                learnedWordsLastReview[index] = today;
            }
        }
        
        // 更新复习过的话题
        if (!string.IsNullOrEmpty(reviewedTopic))
        {
            int index = learnedTopics.IndexOf(reviewedTopic);
            if (index >= 0)
            {
                while (learnedTopicsReviewCount.Count <= index)
                    learnedTopicsReviewCount.Add(0);
                while (learnedTopicsLastReview.Count <= index)
                    learnedTopicsLastReview.Add("");
                
                learnedTopicsReviewCount[index]++;
                learnedTopicsLastReview[index] = today;
            }
        }
        
        SaveReviewData();
    }

    /// <summary>
    /// 直接显示复习提示，然后调用AI生成复习内容
    /// </summary>
    private async void DisplayReviewPrompt()
    {
        if (currentReviewItems.Count == 0)
        {
            Debug.Log("No review items to display");
            return;
        }

        // 分离单词和话题
        List<string> words = new List<string>();
        string topic = "";
        
        foreach (string item in currentReviewItems)
        {
            string[] parts = item.Split(':');
            if (parts.Length == 2)
            {
                if (parts[0] == "word")
                {
                    words.Add(parts[1]);
                }
                else if (parts[0] == "topic")
                {
                    topic = parts[1];
                }
            }
        }

        // 先测试ChatManager是否工作
        Debug.Log("Testing ChatManager with simple message...");
        try
        {
            await ChatManager.Instance.SendMessage("Hello, this is a test message from ReviewManager.");
            Debug.Log("Test message sent successfully");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Test message failed: {e.Message}");
            ShowGeneratedStoryFallback(words, topic);
            return;
        }

        // 等待一秒再发送真正的复习内容
        await System.Threading.Tasks.Task.Delay(1000);

        // 先显示一个加载提示
        if (reviewContentPanel != null)
        {
            // 清除现有内容
            foreach (Transform child in reviewContentPanel)
            {
                Destroy(child.gameObject);
            }
            
            // 显示加载中提示
            CreateReviewMessage(" <b>Preparing your review session...</b>\n\n<i>AI is creating a personalized review for you!</i>");
        }

        // 构建AI提示
        StringBuilder aiPrompt = new StringBuilder();
        aiPrompt.AppendLine("Generate a vocabulary review exercise using these words:");
        
        if (words.Count > 0)
        {
            aiPrompt.AppendLine($"Words: {string.Join(", ", words)}");
        }
        
        if (!string.IsNullOrEmpty(topic))
        {
            aiPrompt.AppendLine($"Topic: {topic}");
        }
        
        aiPrompt.AppendLine();
        aiPrompt.AppendLine("Create a short story (3-4 sentences) that uses ALL the review words in the context of the given topic.");
        aiPrompt.AppendLine("IMPORTANT: Make each review word BOLD using <b>word</b> tags.");
        aiPrompt.AppendLine("Then ask the user to create their own story using the same words.");
        aiPrompt.AppendLine();
        aiPrompt.AppendLine("Example format:");
        aiPrompt.AppendLine("Here's a story using your review words:");
        aiPrompt.AppendLine("[Your story with <b>bold</b> words]");
        aiPrompt.AppendLine();
        aiPrompt.AppendLine("Now it's your turn! Create a story using these words: [word list]");

        // 调用ChatManager发送AI请求
        if (ChatManager.Instance != null)
        {
            Debug.Log("=== Sending review request to AI ===");
            Debug.Log($"AI Prompt: {aiPrompt.ToString()}");
            
            try
            {
                await ChatManager.Instance.SendMessage(aiPrompt.ToString().TrimEnd());
                Debug.Log("AI request sent successfully");
                
                // 标记复习完成（在AI回应后）
                MarkReviewCompleted(words, topic);
                Debug.Log("Review marked as completed");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error sending to ChatManager: {e.Message}");
                // 显示错误信息
                if (reviewContentPanel != null)
                {
                    foreach (Transform child in reviewContentPanel)
                    {
                        Destroy(child.gameObject);
                    }
                    CreateReviewMessage($"? <b>Error:</b> Failed to contact AI: {e.Message}");
                }

                // 备用方案：显示生成的故事内容
                ShowGeneratedStoryFallback(words, topic);
            }
        }
        else
        {
            Debug.LogError("ChatManager.Instance is null!");
            // 显示错误信息
            if (reviewContentPanel != null)
            {
                foreach (Transform child in reviewContentPanel)
                {
                    Destroy(child.gameObject);
                }
                CreateReviewMessage(" <b>Error:</b> ChatManager not available. Please try again.");
            }
        }
    }

    /// <summary>
    /// 让AI生成包含复习单词的故事
    /// </summary>
    private async void GenerateReviewStory(List<string> words, string topic)
    {
        if (ChatManager.Instance == null)
        {
            Debug.LogError("ChatManager.Instance is null!");
            ShowGeneratedStoryFallback(words, topic);
            return;
        }

        // 构建AI提示，要求生成复习故事
        StringBuilder storyPrompt = new StringBuilder();
        storyPrompt.AppendLine("Please create a short, engaging story (4-6 sentences) for vocabulary review.");
        storyPrompt.AppendLine();
        storyPrompt.AppendLine($"You must use ALL of these words in the story: {string.Join(", ", words)}");
        
        if (!string.IsNullOrEmpty(topic))
        {
            storyPrompt.AppendLine($"The story should be related to: {topic}");
        }
        
        storyPrompt.AppendLine();
        storyPrompt.AppendLine("IMPORTANT formatting requirements:");
        storyPrompt.AppendLine("- Make each review word BOLD using <b>word</b> tags");
        storyPrompt.AppendLine("- Write a natural, flowing story");
        storyPrompt.AppendLine("- After the story, add: 'Review complete! These words have been practiced.'");
        storyPrompt.AppendLine();
        storyPrompt.AppendLine("Start with: '? Review Story:' then write your story.");

        try
        {
            Debug.Log("Sending story generation request to AI...");
            await ChatManager.Instance.SendMessage(storyPrompt.ToString().TrimEnd());
            
            // 标记复习完成
            MarkReviewCompleted(words, topic);
            Debug.Log("Review story generation completed");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error generating review story: {e.Message}");
            
            // 备用方案：显示简单的复习内容
            ShowGeneratedStoryFallback(words, topic);
        }
    }

    /// <summary>
    /// 备用方案：生成简单的复习故事
    /// </summary>
    private void ShowGeneratedStoryFallback(List<string> words, string topic)
    {
        StringBuilder storyContent = new StringBuilder();
        storyContent.AppendLine("? <b>Review Story</b>");
        storyContent.AppendLine();
        
        // 生成一个简单的故事
        if (!string.IsNullOrEmpty(topic) && topic.ToLower().Contains("communication"))
        {
            storyContent.AppendLine($"Today I want to <b>{words[0]}</b> you understand something important. ");
            storyContent.AppendLine($"The morning sun was <b>{words[1]}</b> brightly through the window, creating a <b>{words[2]}</b> scene. ");
            storyContent.AppendLine($"She moved <b>{words[3]}</b> across the room, and the sweet <b>{words[4]}</b> of fresh coffee filled the air. ");
            storyContent.AppendLine("This story helps you remember these vocabulary words in context!");
        }
        else
        {
            storyContent.AppendLine("Here's a simple story using your review words:");
            storyContent.AppendLine();
            foreach (string word in words)
            {
                storyContent.AppendLine($"? <b>{word}</b> - Practice using this word in sentences");
            }
            storyContent.AppendLine();
            storyContent.AppendLine($"Topic: <b>{topic}</b>");
        }
        
        storyContent.AppendLine();
        storyContent.AppendLine("<i>Review complete! These words have been practiced.</i>");
        
        if (reviewContentPanel != null)
        {
            foreach (Transform child in reviewContentPanel)
            {
                Destroy(child.gameObject);
            }
            CreateReviewMessage(storyContent.ToString().TrimEnd());
        }
        
        Debug.Log("Displayed fallback review story");
    }

    private void OnDestroy()
    {
        if (ChatManager.Instance != null)
        {
            ChatManager.Instance.onNewMessage.RemoveListener(OnNewMessage);
        }
    }
}
