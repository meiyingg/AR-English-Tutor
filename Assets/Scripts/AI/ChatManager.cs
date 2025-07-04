using UnityEngine;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine.Events;

public class ChatManager : MonoBehaviour
{
    public static ChatManager Instance { get; private set; }

    private List<ChatMessage> messages = new List<ChatMessage>();
    private int currentSessionTurns = 0; // 当前会话的对话轮数

    public enum Sender { User, Tutor }

    [System.Serializable]
    public class NewMessageEvent : UnityEvent<string, Sender, bool> { }
    public NewMessageEvent onNewMessage;
    
    [System.Serializable]
    public class NewImageMessageEvent : UnityEvent<Texture2D, string, Sender> { }
    public NewImageMessageEvent onNewImageMessage;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // Optional: Add a system message to set the context for the AI
        var systemMessage = new ChatMessage { role = "system", content = "You are a helpful English tutor." };
        messages.Add(systemMessage);
    }

    public new async Task SendMessage(string text)
    {
        // Check for learning mode commands first
        if (LearningModeManager.Instance != null && LearningModeManager.Instance.CheckForModeCommand(text))
        {
            // Mode command was processed, no need to send to AI
            return;
        }
        
        var userMessage = new ChatMessage { role = "user", content = text };
        messages.Add(userMessage);
        
        // Increment session turns
        currentSessionTurns++;

        // Add user message to UI immediately
        onNewMessage.Invoke(text, Sender.User, false);
        
        if (ARTutorAnimatorController.Instance != null) ARTutorAnimatorController.Instance.SetThinking(true);
        
        // TODO: Achievement integration - Enable after Unity setup
        // var achievementManager = FindObjectOfType<AchievementManager>();
        // if (achievementManager != null)
        // {
        //     achievementManager.IncrementAchievementProgress("first_message");
        //     achievementManager.IncrementAchievementProgress("chat_master");
        //     achievementManager.IncrementAchievementProgress("conversation_king");
        // }

        string response = await OpenAIManager.Instance.PostRequest(messages);
        
        if (ARTutorAnimatorController.Instance != null) ARTutorAnimatorController.Instance.SetThinking(false);

        if (!string.IsNullOrEmpty(response))
        {
            AnalyzeResponseAndPlayAnimation(response);
            var aiMessage = new ChatMessage { role = "assistant", content = response };
            messages.Add(aiMessage);
            onNewMessage.Invoke(response, Sender.Tutor, false);
            
            // In Normal mode, provide grammar correction after AI response
            if (LearningModeManager.Instance != null && 
                LearningModeManager.Instance.currentMode == LearningModeManager.LearningMode.Normal)
            {
                await ProvideGrammarCorrection(text);
            }
            
            // Award experience for each conversation turn
            if (LearningProgressManager.Instance != null)
            {
                // Give experience for each conversation (8 EXP per turn)
                LearningProgressManager.Instance.AddExperience(8);
                
                // Give bonus for continuous conversation (every 3rd turn)
                if (currentSessionTurns >= 3)
                {
                    LearningProgressManager.Instance.AddExperience(7); // Bonus EXP
                    Debug.Log($"? Conversation bonus: +7 EXP for continuous chat!");
                    currentSessionTurns = 0; // Reset counter after bonus
                }
            }
        }
        else
        {
            onNewMessage.Invoke("Error: Could not get a response.", Sender.Tutor, true);
        }
    }

    public async Task SendImageMessage(string imageBase64, Texture2D imageTexture, string additionalText = "")
    {
        // Show user's image message with any additional text
        string userDisplayMessage = string.IsNullOrEmpty(additionalText) 
            ? "? Image uploaded" 
            : $"? Image uploaded: {additionalText}";
        onNewMessage.Invoke(userDisplayMessage, Sender.User, true);
        
        // Show analyzing message
        onNewMessage.Invoke("Analyzing image...", Sender.Tutor, true);
        
        if (ARTutorAnimatorController.Instance != null) ARTutorAnimatorController.Instance.SetThinking(true);
        
        // Create prompt for image analysis
        string prompt = string.IsNullOrEmpty(additionalText) 
            ? "Describe this image in English for language learning. Be detailed and educational."
            : $"Describe this image in English for language learning. User also added: {additionalText}";
        
        // Get AI response for the image
        string response = await OpenAIManager.Instance.PostVisionRequest(imageBase64, prompt);
        
        if (ARTutorAnimatorController.Instance != null) ARTutorAnimatorController.Instance.SetThinking(false);
        
        if (!string.IsNullOrEmpty(response))
        {
            AnalyzeResponseAndPlayAnimation(response);
            // Add the image analysis to chat history
            var aiMessage = new ChatMessage { role = "assistant", content = response };
            messages.Add(aiMessage);
            
            // Replace the "Analyzing..." message with the actual response
            onNewMessage.Invoke(response, Sender.Tutor, false);
        }
        else
        {
            onNewMessage.Invoke("Sorry, I couldn't analyze the image. Please try again.", Sender.Tutor, true);
        }
    }

    public async Task SendSceneRecognitionRequest(string imageBase64, Texture2D imageTexture, string additionalText = "")
    {
        // Reset session turn counter for new scene
        currentSessionTurns = 0;
        
        // Get current learning mode
        LearningModeManager.LearningMode currentMode = LearningModeManager.LearningMode.Scene;
        if (LearningModeManager.Instance != null)
        {
            currentMode = LearningModeManager.Instance.currentMode;
        }
        
        // Show user's action message based on current mode
        string userDisplayMessage = GetModeSpecificUserMessage(currentMode, additionalText);
        onNewMessage.Invoke(userDisplayMessage, Sender.User, true);
        
        // Show analyzing message
        string analyzingMessage = GetModeSpecificAnalyzingMessage(currentMode);
        onNewMessage.Invoke(analyzingMessage, Sender.Tutor, true);
        
        if (ARTutorAnimatorController.Instance != null) ARTutorAnimatorController.Instance.SetThinking(true);
        
        // Get user's learning level to adjust difficulty
        string difficultyLevel = "beginner";
        if (LearningProgressManager.Instance != null)
        {
            difficultyLevel = LearningProgressManager.Instance.GetLearningDifficultyLevel();
        }
        
        // Create mode-specific prompt
        string scenePrompt = CreateModeSpecificPrompt(currentMode, difficultyLevel);

        if (!string.IsNullOrEmpty(additionalText))
        {
            scenePrompt += $"\n\nUser's additional context: {additionalText}";
        }
        
        // Get AI response for scene recognition
        string response = await OpenAIManager.Instance.PostVisionRequest(imageBase64, scenePrompt);
        
        if (ARTutorAnimatorController.Instance != null) ARTutorAnimatorController.Instance.SetThinking(false);
        
        if (!string.IsNullOrEmpty(response))
        {
            AnalyzeResponseAndPlayAnimation(response);
            // Add the scene analysis to chat history
            var aiMessage = new ChatMessage { role = "assistant", content = response };
            messages.Add(aiMessage);
            
            // Display the interactive lesson start
            onNewMessage.Invoke(response, Sender.Tutor, false);
            
            // Award experience for starting a scene learning session
            if (LearningProgressManager.Instance != null)
            {
                LearningProgressManager.Instance.CompleteSession(true, 1);
            }
        }
        else
        {
            onNewMessage.Invoke("Sorry, I couldn't see the image clearly. Could you try uploading it again?", Sender.Tutor, true);
        }
    }

    private void AnalyzeResponseAndPlayAnimation(string response)
    {
        if (ARTutorAnimatorController.Instance == null) return;

        string lowerResponse = response.ToLower();

        // if (lowerResponse.Contains("hello") || lowerResponse.Contains("hi") || lowerResponse.Contains("welcome"))
        // {
        //     ARTutorAnimatorController.Instance.PlayAnimation("Waving");
        // }
        // else if (lowerResponse.Contains("look") || lowerResponse.Contains("see") || lowerResponse.Contains("that"))
        // {
        //     ARTutorAnimatorController.Instance.PlayAnimation("PointingForward");
        // }
        // else if (lowerResponse.Contains("let's") || lowerResponse.Contains("ready"))
        // {
        //     ARTutorAnimatorController.Instance.PlayAnimation("BboyHipHopMove");
        // }
        // Add more animation triggers here based on keywords
        
    }
    
    // Helper methods for mode-specific messaging
    private string GetModeSpecificUserMessage(LearningModeManager.LearningMode mode, string additionalText)
    {
        string baseMessage = mode switch
        {
            LearningModeManager.LearningMode.Scene => "Let's start a scene learning session!",
            LearningModeManager.LearningMode.Word => "Let's learn vocabulary from this image!",
            _ => "Let's start an English lesson with this scene!"
        };
        
        if (!string.IsNullOrEmpty(additionalText))
        {
            baseMessage += $" {additionalText}";
        }
        
        return baseMessage;
    }
    
    private string GetModeSpecificAnalyzingMessage(LearningModeManager.LearningMode mode)
    {
        return mode switch
        {
            LearningModeManager.LearningMode.Scene => "Looking at your scene and preparing an interactive lesson...",
            LearningModeManager.LearningMode.Word => "Identifying vocabulary words in your image...",
            _ => "Looking at your image and preparing lesson..."
        };
    }
    
    private string CreateModeSpecificPrompt(LearningModeManager.LearningMode mode, string difficultyLevel)
    {
        string basePrompt = mode switch
        {
            LearningModeManager.LearningMode.Scene => CreateSceneModePrompt(difficultyLevel),
            LearningModeManager.LearningMode.Word => CreateWordModePrompt(difficultyLevel),
            _ => CreateSceneModePrompt(difficultyLevel)
        };
        
        return basePrompt;
    }
    
    private string CreateSceneModePrompt(string difficultyLevel)
    {
        return $@"You are an English tutor. Look at this image and start an interactive English lesson based on what you see.

Learning Level: {difficultyLevel}

Instructions:
1. Identify the scene/location in the image
2. Act as a friendly English teacher - start a conversation about this scene  
3. ONLY respond in English (no Chinese translations)
4. Adapt your language complexity to the {difficultyLevel} level
5. Ask ONE interactive question to engage the student
6. Keep your first response short and conversational (2-3 sentences max)

Language Guidelines for {difficultyLevel} level:
- beginner: Use simple words, present tense, basic vocabulary
- elementary: Use common phrases, simple past/future, everyday vocabulary  
- intermediate: Use natural expressions, various tenses, common idioms
- upper-intermediate: Use business/formal language, complex sentences
- advanced: Use sophisticated vocabulary, cultural references, nuanced expressions

Example responses for {difficultyLevel}:
{GetLevelSpecificExamples(difficultyLevel)}

Start the lesson now - be natural and encouraging!";
    }
    
    private string CreateWordModePrompt(string difficultyLevel)
    {
        return $@"You are an English tutor. Look at this image and focus on vocabulary learning.

Learning Level: {difficultyLevel}

Instructions:
1. Identify the main objects in the image
2. Choose 1-2 key vocabulary words based on the user's level
3. Provide simple definitions and example sentences
4. Create a short dialogue using these words
5. Ask the user to practice with these words
6. Keep your response focused and educational

Language Guidelines for {difficultyLevel} level:
- beginner: Focus on basic nouns, simple definitions
- elementary: Add adjectives, basic verbs, simple sentences
- intermediate: Include phrasal verbs, expressions, context usage
- upper-intermediate: Advanced vocabulary, idioms, formal usage
- advanced: Sophisticated vocabulary, nuanced meanings, cultural context

Example format:
'I see a [object]! Let me teach you about this word.
[Object] means [definition]. 
Example: [sentence]
Mini dialogue: A: [sentence] B: [response]
Now try making your own sentence with [object]!'

Start the vocabulary lesson now!";
    }
    
    private string GetLevelSpecificExamples(string difficultyLevel)
    {
        return difficultyLevel switch
        {
            "beginner" => @"
- ""Hello! I see a coffee shop. Do you like coffee?""
- ""Hi! This is a park. The weather looks nice today.""",
            "elementary" => @"
- ""Hello! I can see you're at a coffee shop. What would you like to order today?""
- ""Hi there! This looks like a beautiful park. Do you enjoy spending time outdoors?""",
            "intermediate" => @"
- ""Welcome! I see you're in a restaurant. Are you dining alone or with friends?""
- ""This appears to be a bustling shopping district. What brings you here today?""",
            "upper-intermediate" => @"
- ""I notice you're in what appears to be a professional environment. Are you here for business or pleasure?""
- ""This seems like an interesting cultural venue. What's caught your attention here?""",
            "advanced" => @"
- ""This establishment has quite an ambiance. I'm curious about what drew you to this particular locale.""
- ""The setting suggests this might be a place of some cultural significance. What's your take on it?""",
            _ => @"
- ""Hello! I see a coffee shop. Do you like coffee?""
- ""Hi! This is a park. The weather looks nice today."""
        };
    }
    
    /// <summary>
    /// Provides grammar correction and demonstration for user's message in Normal mode
    /// </summary>
    private async Task ProvideGrammarCorrection(string userText)
    {
        // Get user's learning level for appropriate corrections
        string difficultyLevel = "beginner";
        if (LearningProgressManager.Instance != null)
        {
            difficultyLevel = LearningProgressManager.Instance.GetLearningDifficultyLevel();
        }
        
        // Create grammar correction prompt
        string grammarPrompt = CreateGrammarCorrectionPrompt(userText, difficultyLevel);
        
        // Create a temporary message list for grammar correction (don't add to main conversation)
        var grammarMessages = new List<ChatMessage>
        {
            new ChatMessage { role = "system", content = "You are an English grammar tutor." },
            new ChatMessage { role = "user", content = grammarPrompt }
        };
        
        // Get grammar correction response
        string correctionResponse = await OpenAIManager.Instance.PostRequest(grammarMessages);
        
        if (!string.IsNullOrEmpty(correctionResponse))
        {
            // Display grammar correction as a separate tutor message
            onNewMessage.Invoke(correctionResponse, Sender.Tutor, false);
        }
    }
    
    /// <summary>
    /// Creates a grammar correction prompt based on user's message and level
    /// </summary>
    private string CreateGrammarCorrectionPrompt(string userText, string difficultyLevel)
    {
        return $@"Please analyze this English sentence and provide grammar correction and demonstration:

User's message: ""{userText}""
User's level: {difficultyLevel}

Instructions:
1. If there are grammar errors, gently correct them
2. Explain the correction in simple terms appropriate for {difficultyLevel} level
3. Provide a corrected version of the sentence
4. Give one additional example using the corrected grammar
5. Keep your feedback encouraging and educational
6. If the grammar is already correct, acknowledge this and provide a tip for improvement
7. the whole response should not exceed 4 sentences

Format your response like this:
Grammar Check:
[Your analysis here]

Corrected: [Corrected sentence if needed, or 'Your grammar is correct!']

Example: [Additional example sentence];
    }
}
