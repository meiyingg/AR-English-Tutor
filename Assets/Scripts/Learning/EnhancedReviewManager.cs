using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System;
using System.Linq;

/// <summary>
/// Enhanced Review Manager with Ebbinghaus Forgetting Curve
/// Provides intelligent spaced repetition and progress tracking
/// </summary>
public class EnhancedReviewManager : MonoBehaviour
{
    public static EnhancedReviewManager Instance { get; private set; }

    [Header("Review UI References")]
    public GameObject reviewPanel;
    public Transform reviewContentPanel;
    public Button startReviewButton;
    public GameObject reviewMessagePrefab;
    public Button settingsButton;
    public Slider difficultySlider;
    public Toggle autoReviewToggle;

    [Header("Review Settings")]
    public int maxReviewItems = 50;
    public bool enableEbbinghausCurve = true;
    public bool enableProgressTracking = true;
    public ReviewDifficulty defaultDifficulty = ReviewDifficulty.Medium;

    // Ebbinghaus intervals (in hours)
    private readonly int[] ebbinghausIntervals = { 1, 8, 24, 72, 168, 336, 720 }; // 1h, 8h, 1d, 3d, 1w, 2w, 1m

    // Data storage
    private const string LEARNED_ITEMS_KEY = "LearnedItems_v2";
    private const string REVIEW_SETTINGS_KEY = "ReviewSettings";

    // Current data
    private List<LearnedItem> learnedItems = new List<LearnedItem>();
    private List<LearnedItem> currentReviewItems = new List<LearnedItem>();
    private int currentReviewIndex = 0;
    private bool isReviewActive = false;

    public enum ReviewDifficulty { Easy, Medium, Hard }
    public enum ItemType { Word, Topic, Phrase, Grammar }
    public enum MasteryLevel { New, Learning, Familiar, Mastered, Expert }

    [Serializable]
    public class LearnedItem
    {
        public string content;
        public ItemType type;
        public string meaning;
        public DateTime learnTime;
        public DateTime lastReviewTime;
        public DateTime nextReviewTime;
        public int reviewCount;
        public MasteryLevel masteryLevel;
        public float difficultyScore; // 0-1, user's difficulty with this item
        public List<string> contextExamples = new List<string>();
        public bool isActive = true;

        public LearnedItem(string content, ItemType type, string meaning = "")
        {
            this.content = content;
            this.type = type;
            this.meaning = meaning;
            this.learnTime = DateTime.Now;
            this.lastReviewTime = DateTime.Now;
            this.reviewCount = 0;
            this.masteryLevel = MasteryLevel.New;
            this.difficultyScore = 0.5f;
            CalculateNextReviewTime();
        }

        public void CalculateNextReviewTime()
        {
            if (!EnhancedReviewManager.Instance.enableEbbinghausCurve)
            {
                nextReviewTime = DateTime.Now.AddHours(24); // Simple 24h interval
                return;
            }

            int intervalIndex = Mathf.Min(reviewCount, EnhancedReviewManager.Instance.ebbinghausIntervals.Length - 1);
            int baseHours = EnhancedReviewManager.Instance.ebbinghausIntervals[intervalIndex];
            
            // Adjust interval based on mastery and difficulty
            float masteryMultiplier = GetMasteryMultiplier();
            float difficultyMultiplier = 1.0f + difficultyScore; // Higher difficulty = longer intervals
            
            int adjustedHours = Mathf.RoundToInt(baseHours * masteryMultiplier * difficultyMultiplier);
            nextReviewTime = lastReviewTime.AddHours(adjustedHours);
        }

        private float GetMasteryMultiplier()
        {
            return masteryLevel switch
            {
                MasteryLevel.New => 0.5f,
                MasteryLevel.Learning => 0.7f,
                MasteryLevel.Familiar => 1.0f,
                MasteryLevel.Mastered => 1.5f,
                MasteryLevel.Expert => 2.0f,
                _ => 1.0f
            };
        }

        public bool IsDueForReview()
        {
            return DateTime.Now >= nextReviewTime && isActive;
        }

        public void UpdateAfterReview(bool wasCorrect, float userDifficulty)
        {
            lastReviewTime = DateTime.Now;
            reviewCount++;
            
            // Update difficulty score based on user performance
            if (wasCorrect)
            {
                difficultyScore = Mathf.Max(0, difficultyScore - 0.1f);
                // Improve mastery level
                if (reviewCount >= 3 && difficultyScore < 0.3f)
                {
                    masteryLevel = (MasteryLevel)Mathf.Min((int)masteryLevel + 1, (int)MasteryLevel.Expert);
                }
            }
            else
            {
                difficultyScore = Mathf.Min(1, difficultyScore + 0.2f);
                // Decrease mastery if struggling
                if (difficultyScore > 0.7f)
                {
                    masteryLevel = (MasteryLevel)Mathf.Max((int)masteryLevel - 1, (int)MasteryLevel.New);
                }
            }

            CalculateNextReviewTime();
        }
    }

    [Serializable]
    public class ReviewSettings
    {
        public bool enableEbbinghaus = true;
        public bool enableProgressTracking = true;
        public ReviewDifficulty difficulty = ReviewDifficulty.Medium;
        public int dailyReviewTarget = 10;
        public bool notificationsEnabled = true;
    }

    private ReviewSettings currentSettings = new ReviewSettings();

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
        LoadSettings();
        SetupUI();
        StartCoroutine(CheckForDueReviews());
    }

    /// <summary>
    /// Add learned content with enhanced tracking
    /// </summary>
    public void AddLearnedContent(string content, ItemType type, string meaning = "", string context = "")
    {
        // Check if item already exists
        var existingItem = learnedItems.FirstOrDefault(item => 
            item.content.Equals(content, StringComparison.OrdinalIgnoreCase) && item.type == type);

        if (existingItem != null)
        {
            // Add context example if new
            if (!string.IsNullOrEmpty(context) && !existingItem.contextExamples.Contains(context))
            {
                existingItem.contextExamples.Add(context);
                if (existingItem.contextExamples.Count > 5) // Limit examples
                {
                    existingItem.contextExamples.RemoveAt(0);
                }
            }
            return;
        }

        // Create new item
        var newItem = new LearnedItem(content, type, meaning);
        if (!string.IsNullOrEmpty(context))
        {
            newItem.contextExamples.Add(context);
        }

        learnedItems.Add(newItem);

        // Keep list size manageable
        if (learnedItems.Count > maxReviewItems)
        {
            // Remove oldest items that are mastered
            var itemsToRemove = learnedItems
                .Where(item => item.masteryLevel == MasteryLevel.Expert)
                .OrderBy(item => item.lastReviewTime)
                .Take(5)
                .ToList();

            foreach (var item in itemsToRemove)
            {
                learnedItems.Remove(item);
            }
        }

        SaveReviewData();
        Debug.Log($"Added {type}: {content} (Total items: {learnedItems.Count})");
    }

    /// <summary>
    /// Get items due for review based on Ebbinghaus curve
    /// </summary>
    public List<LearnedItem> GetDueItems()
    {
        return learnedItems.Where(item => item.IsDueForReview())
                          .OrderBy(item => item.nextReviewTime)
                          .ToList();
    }

    /// <summary>
    /// Start intelligent review session
    /// </summary>
    public async void StartIntelligentReview()
    {
        var dueItems = GetDueItems();
        
        if (dueItems.Count == 0)
        {
            ShowNoReviewNeededMessage();
            return;
        }

        // Prioritize by urgency and difficulty
        currentReviewItems = dueItems
            .OrderBy(item => item.nextReviewTime) // Most overdue first
            .ThenByDescending(item => item.difficultyScore) // Harder items first
            .Take(currentSettings.dailyReviewTarget)
            .ToList();

        isReviewActive = true;
        currentReviewIndex = 0;
        HideReviewPanel();

        // Start AI-assisted review
        await StartNextReviewItem();
    }

    /// <summary>
    /// Start reviewing next item
    /// </summary>
    private async System.Threading.Tasks.Task StartNextReviewItem()
    {
        if (currentReviewIndex >= currentReviewItems.Count)
        {
            CompleteReviewSession();
            return;
        }

        var item = currentReviewItems[currentReviewIndex];
        string reviewPrompt = GenerateIntelligentReviewPrompt(item);

        if (ChatManager.Instance != null)
        {
            await ChatManager.Instance.SendMessage(reviewPrompt);
        }
    }

    /// <summary>
    /// Generate context-aware review prompt
    /// </summary>
    private string GenerateIntelligentReviewPrompt(LearnedItem item)
    {
        string basePrompt = item.type switch
        {
            ItemType.Word => $"Let's review the word '{item.content}'. ",
            ItemType.Topic => $"Let's review the topic '{item.content}'. ",
            ItemType.Phrase => $"Let's review the phrase '{item.content}'. ",
            ItemType.Grammar => $"Let's review the grammar concept '{item.content}'. ",
            _ => $"Let's review '{item.content}'. "
        };

        // Adjust difficulty based on mastery level
        string difficultyInstruction = item.masteryLevel switch
        {
            MasteryLevel.New => "Start with a simple question to help them remember this new content.",
            MasteryLevel.Learning => "Ask a moderate question to test their understanding.",
            MasteryLevel.Familiar => "Challenge them with a practical application question.",
            MasteryLevel.Mastered => "Ask an advanced question or request creative usage.",
            MasteryLevel.Expert => "Challenge them to teach or explain this to others.",
            _ => "Ask an appropriate question for their level."
        };

        string contextInfo = "";
        if (item.contextExamples.Count > 0)
        {
            contextInfo = $" They previously encountered this in contexts like: {string.Join(", ", item.contextExamples.Take(2))}";
        }

        return $"{basePrompt}{difficultyInstruction}{contextInfo} Keep it engaging and educational.";
    }

    /// <summary>
    /// Handle user's review response
    /// </summary>
    public void ProcessReviewResponse(string userResponse, bool isCorrect)
    {
        if (!isReviewActive || currentReviewIndex >= currentReviewItems.Count) return;

        var currentItem = currentReviewItems[currentReviewIndex];
        
        // Rate difficulty based on response quality (simplified)
        float perceivedDifficulty = isCorrect ? 0.3f : 0.8f;
        
        currentItem.UpdateAfterReview(isCorrect, perceivedDifficulty);
        currentReviewIndex++;

        // Continue to next item
        _ = StartNextReviewItem();
    }

    /// <summary>
    /// Complete review session with summary
    /// </summary>
    private void CompleteReviewSession()
    {
        isReviewActive = false;
        int reviewedCount = currentReviewItems.Count;
        
        string summaryMessage = $"Review session completed! You reviewed {reviewedCount} items. ";
        
        // Calculate next review suggestions
        var nextDue = GetDueItems().Take(5).ToList();
        if (nextDue.Count > 0)
        {
            var nextReviewTime = nextDue.Min(item => item.nextReviewTime);
            var timeUntilNext = nextReviewTime - DateTime.Now;
            
            if (timeUntilNext.TotalHours < 24)
            {
                summaryMessage += $"Next review session available in {timeUntilNext.Hours} hours.";
            }
            else
            {
                summaryMessage += $"Next review session available tomorrow.";
            }
        }

        if (ChatManager.Instance != null)
        {
            ChatManager.Instance.onNewMessage?.Invoke(summaryMessage, ChatManager.Sender.Tutor, false);
        }

        SaveReviewData();
    }

    private void ShowNoReviewNeededMessage()
    {
        string message = "Great! No items are due for review right now. ";
        
        var nextDue = learnedItems.Where(item => item.isActive)
                                 .OrderBy(item => item.nextReviewTime)
                                 .FirstOrDefault();
        
        if (nextDue != null)
        {
            var timeUntilNext = nextDue.nextReviewTime - DateTime.Now;
            if (timeUntilNext.TotalHours < 24)
            {
                message += $"Next review in {timeUntilNext.Hours} hours.";
            }
            else
            {
                message += "Check back tomorrow for new reviews.";
            }
        }

        if (ChatManager.Instance != null)
        {
            ChatManager.Instance.onNewMessage?.Invoke(message, ChatManager.Sender.Tutor, false);
        }
    }

    /// <summary>
    /// Check for due reviews periodically
    /// </summary>
    private System.Collections.IEnumerator CheckForDueReviews()
    {
        while (true)
        {
            yield return new WaitForSeconds(3600); // Check every hour
            
            var dueCount = GetDueItems().Count;
            if (dueCount > 0 && currentSettings.notificationsEnabled)
            {
                Debug.Log($"{dueCount} items are due for review!");
                // Could trigger notification UI here
            }
        }
    }

    /// <summary>
    /// Enhanced review panel display
    /// </summary>
    public void ShowEnhancedReviewPanel()
    {
        if (reviewPanel != null)
        {
            reviewPanel.SetActive(true);
            DisplayEnhancedReviewContent();
        }
    }

    private void DisplayEnhancedReviewContent()
    {
        if (reviewContentPanel == null || reviewMessagePrefab == null) return;

        // Clear existing content
        foreach (Transform child in reviewContentPanel)
        {
            Destroy(child.gameObject);
        }

        // Show enhanced summary
        var dueItems = GetDueItems();
        var masteryStats = learnedItems.GroupBy(item => item.masteryLevel)
                                      .ToDictionary(g => g.Key, g => g.Count());

        CreateReviewMessage($"Learning Progress: {learnedItems.Count} total items");
        CreateReviewMessage($"Due for review: {dueItems.Count} items");
        
        // Show mastery breakdown
        foreach (var stat in masteryStats)
        {
            CreateReviewMessage($"{stat.Key}: {stat.Value} items");
        }

        // Show upcoming reviews
        if (dueItems.Count > 0)
        {
            CreateReviewMessage("Most urgent reviews:");
            foreach (var item in dueItems.Take(5))
            {
                var overdue = DateTime.Now - item.nextReviewTime;
                string overdueText = overdue.TotalHours > 0 ? $" (overdue {overdue.Hours}h)" : "";
                CreateReviewMessage($"- {item.content} ({item.type}){overdueText}");
            }
        }

        if (learnedItems.Count == 0)
        {
            CreateReviewMessage("Start learning to see your progress here!");
        }
    }

    public void ToggleReviewPanel()
    {
        if (reviewPanel != null)
        {
            if (reviewPanel.activeSelf)
            {
                HideReviewPanel();
            }
            else
            {
                ShowEnhancedReviewPanel();
            }
        }
    }

    public void HideReviewPanel()
    {
        if (reviewPanel != null)
        {
            reviewPanel.SetActive(false);
        }
    }

    private void CreateReviewMessage(string content)
    {
        if (reviewMessagePrefab == null || reviewContentPanel == null) return;

        GameObject messageObj = Instantiate(reviewMessagePrefab, reviewContentPanel);
        TextMeshProUGUI textComponent = messageObj.GetComponentInChildren<TextMeshProUGUI>();
        if (textComponent != null)
        {
            textComponent.text = content;
        }
    }

    private void SetupUI()
    {
        if (startReviewButton != null)
        {
            startReviewButton.onClick.AddListener(StartIntelligentReview);
        }

        if (reviewPanel != null)
        {
            reviewPanel.SetActive(false);
        }
    }

    private void SaveReviewData()
    {
        try
        {
            var dataWrapper = new ReviewDataWrapper { items = learnedItems };
            string json = JsonUtility.ToJson(dataWrapper);
            PlayerPrefs.SetString(LEARNED_ITEMS_KEY, json);
            PlayerPrefs.Save();
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to save review data: {e.Message}");
        }
    }

    private void LoadReviewData()
    {
        try
        {
            if (PlayerPrefs.HasKey(LEARNED_ITEMS_KEY))
            {
                string json = PlayerPrefs.GetString(LEARNED_ITEMS_KEY);
                var dataWrapper = JsonUtility.FromJson<ReviewDataWrapper>(json);
                if (dataWrapper?.items != null)
                {
                    learnedItems = dataWrapper.items;
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to load review data: {e.Message}");
            learnedItems = new List<LearnedItem>();
        }
    }

    private void LoadSettings()
    {
        if (PlayerPrefs.HasKey(REVIEW_SETTINGS_KEY))
        {
            try
            {
                string json = PlayerPrefs.GetString(REVIEW_SETTINGS_KEY);
                currentSettings = JsonUtility.FromJson<ReviewSettings>(json);
            }
            catch
            {
                currentSettings = new ReviewSettings();
            }
        }
    }

    // Wrapper for JSON serialization
    [Serializable]
    private class ReviewDataWrapper
    {
        public List<LearnedItem> items;
    }

    // Public API for backward compatibility
    public void AddLearnedWord(string word) => AddLearnedContent(word, ItemType.Word);
    public void AddLearnedTopic(string topic) => AddLearnedContent(topic, ItemType.Topic);
}
