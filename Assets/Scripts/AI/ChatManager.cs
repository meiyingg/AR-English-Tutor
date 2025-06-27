using UnityEngine;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine.Events;

public class ChatManager : MonoBehaviour
{
    public static ChatManager Instance { get; private set; }

    private List<ChatMessage> messages = new List<ChatMessage>();

    public enum Sender { User, Tutor }

    [System.Serializable]
    public class NewMessageEvent : UnityEvent<string, Sender> { }
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
        var userMessage = new ChatMessage { role = "user", content = text };
        messages.Add(userMessage);

        // Add user message to UI immediately
        onNewMessage.Invoke(text, Sender.User);

        string response = await OpenAIManager.Instance.PostRequest(messages);

        if (!string.IsNullOrEmpty(response))
        {
            var aiMessage = new ChatMessage { role = "assistant", content = response };
            messages.Add(aiMessage);
            onNewMessage.Invoke(response, Sender.Tutor);
        }
        else
        {
            onNewMessage.Invoke("Error: Could not get a response.", Sender.Tutor);
        }
    }

    public async Task SendImageMessage(string imageBase64, Texture2D imageTexture, string additionalText = "")
    {
        // Show user's image message with any additional text
        string userDisplayMessage = string.IsNullOrEmpty(additionalText) 
            ? "? Image uploaded" 
            : $"? Image uploaded: {additionalText}";
        onNewMessage.Invoke(userDisplayMessage, Sender.User);
        
        // Show analyzing message
        onNewMessage.Invoke("Analyzing image...", Sender.Tutor);
        
        // Create prompt for image analysis
        string prompt = string.IsNullOrEmpty(additionalText) 
            ? "Describe this image in English for language learning. Be detailed and educational."
            : $"Describe this image in English for language learning. User also added: {additionalText}";
        
        // Get AI response for the image
        string response = await OpenAIManager.Instance.PostVisionRequest(imageBase64, prompt);
        
        if (!string.IsNullOrEmpty(response))
        {
            // Add the image analysis to chat history
            var aiMessage = new ChatMessage { role = "assistant", content = response };
            messages.Add(aiMessage);
            
            // Replace the "Analyzing..." message with the actual response
            onNewMessage.Invoke(response, Sender.Tutor);
        }
        else
        {
            onNewMessage.Invoke("Sorry, I couldn't analyze the image. Please try again.", Sender.Tutor);
        }
    }

    public async Task SendSceneRecognitionRequest(string imageBase64, Texture2D imageTexture, string additionalText = "")
    {
        // Show user's action message
        string userDisplayMessage = string.IsNullOrEmpty(additionalText) 
            ? "? Let's start an English lesson with this scene!" 
            : $"? Scene lesson: {additionalText}";
        onNewMessage.Invoke(userDisplayMessage, Sender.User);
        
        // Show analyzing message
        onNewMessage.Invoke("Looking at your image and preparing lesson...", Sender.Tutor);
        
        // Create scene recognition prompt for interactive learning
        string scenePrompt = @"You are an English tutor. Look at this image and start an interactive English lesson based on what you see.

Instructions:
1. Identify the scene/location in the image
2. Act as a friendly English teacher - start a conversation about this scene
3. ONLY respond in English (no Chinese translations)
4. Begin with a simple greeting and scene introduction
5. Ask ONE interactive question to engage the student
6. Keep your first response short and conversational (2-3 sentences max)

Example responses:
- ""Hello! I can see you're at a coffee shop. What would you like to order today?""
- ""Hi there! This looks like a beautiful park. Do you enjoy spending time outdoors?""
- ""Welcome! I see you're in a restaurant. Are you dining alone or with friends?""

Start the lesson now - be natural and encouraging!";

        if (!string.IsNullOrEmpty(additionalText))
        {
            scenePrompt += $"\n\nUser's additional context: {additionalText}";
        }
        
        // Get AI response for scene recognition
        string response = await OpenAIManager.Instance.PostVisionRequest(imageBase64, scenePrompt);
        
        if (!string.IsNullOrEmpty(response))
        {
            // Add the scene analysis to chat history
            var aiMessage = new ChatMessage { role = "assistant", content = response };
            messages.Add(aiMessage);
            
            // Display the interactive lesson start
            onNewMessage.Invoke(response, Sender.Tutor);
        }
        else
        {
            onNewMessage.Invoke("Sorry, I couldn't see the image clearly. Could you try uploading it again?", Sender.Tutor);
        }
    }
}
