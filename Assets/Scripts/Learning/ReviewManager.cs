using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System;

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

        // Load learned topics
        string topicsData = PlayerPrefs.GetString(LEARNED_TOPICS_KEY, "");
        if (!string.IsNullOrEmpty(topicsData))
        {
            string[] topics = topicsData.Split('|');
            learnedTopics.AddRange(topics);
        }

        Debug.Log($"Loaded {learnedWords.Count} words and {learnedTopics.Count} topics for review");
    }

    /// <summary>
    /// Save review data to PlayerPrefs
    /// </summary>
    private void SaveReviewData()
    {
        // Save learned words
        string wordsData = string.Join("|", learnedWords);
        PlayerPrefs.SetString(LEARNED_WORDS_KEY, wordsData);

        // Save learned topics
        string topicsData = string.Join("|", learnedTopics);
        PlayerPrefs.SetString(LEARNED_TOPICS_KEY, topicsData);

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
        
        // Keep only recent items to avoid too much data
        if (learnedWords.Count > maxReviewItems)
        {
            learnedWords.RemoveAt(0);
        }

        SaveReviewData();
        Debug.Log($"Added word to review: {word}");
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
        
        // Keep only recent items to avoid too much data
        if (learnedTopics.Count > maxReviewItems)
        {
            learnedTopics.RemoveAt(0);
        }

        SaveReviewData();
        Debug.Log($"Added topic to review: {topic}");
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
            if (reviewPanel.activeSelf)
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
    /// Display learned words and topics in the review panel
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

        // Show summary
        CreateReviewMessage($"Review Summary: {learnedWords.Count} words, {learnedTopics.Count} topics learned");

        // Display learned words
        if (learnedWords.Count > 0)
        {
            CreateReviewMessage("Learned Words:");
            foreach (string word in learnedWords)
            {
                CreateReviewMessage($"- {word}");
            }
        }

        // Display learned topics
        if (learnedTopics.Count > 0)
        {
            CreateReviewMessage("Learned Topics:");
            foreach (string topic in learnedTopics)
            {
                CreateReviewMessage($"- {topic}");
            }
        }

        if (learnedWords.Count == 0 && learnedTopics.Count == 0)
        {
            CreateReviewMessage("No learning content yet. Start learning to see your progress here!");
        }
    }

    /// <summary>
    /// Create a review message using the message prefab
    /// </summary>
    private void CreateReviewMessage(string content)
    {
        if (reviewMessagePrefab == null || reviewContentPanel == null)
            return;

        GameObject messageObj = Instantiate(reviewMessagePrefab, reviewContentPanel);
        
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
            Debug.Log("No content available for review");
            return;
        }

        // Prepare review items
        PrepareReviewItems();

        // Switch to review mode and start AI interaction
        if (ChatManager.Instance != null && currentReviewItems.Count > 0)
        {
            isReviewActive = true;
            currentReviewIndex = 0;
            
            // Hide review panel and start AI review
            HideReviewPanel();
            
            // Start AI review conversation
            StartReviewConversation();
        }
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
        if (currentReviewIndex >= currentReviewItems.Count)
        {
            // Review session completed
            isReviewActive = false;
            return "Review session completed! Great job practicing your learned content.";
        }

        string currentItem = currentReviewItems[currentReviewIndex];
        string[] parts = currentItem.Split(':');
        
        if (parts.Length != 2)
            return "Review error occurred.";

        string type = parts[0];
        string content = parts[1];

        string prompt;
        if (type == "word")
        {
            prompt = $"Review time! Please ask the user about the word '{content}'. You can ask them to use it in a sentence, explain its meaning, or provide synonyms. Keep it simple and encouraging.";
        }
        else // topic
        {
            prompt = $"Review time! Please ask the user a question about the topic '{content}'. Ask them to describe a situation or use key vocabulary related to this topic. Keep it simple and encouraging.";
        }

        currentReviewIndex++;
        return prompt;
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
}
