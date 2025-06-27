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
}
