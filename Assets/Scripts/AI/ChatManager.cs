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
}
