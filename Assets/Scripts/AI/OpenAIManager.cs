using UnityEngine;
using UnityEngine.Networking;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;

public class OpenAIManager : MonoBehaviour
{
    public static OpenAIManager Instance { get; private set; }

    public APIConfig apiConfig;
    private const string ApiUrl = "https://api.openai.com/v1/chat/completions";

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

    public async Task<string> PostRequest(List<ChatMessage> messages)
    {
        var requestData = new ChatRequest
        {
            model = "gpt-3.5-turbo",
            messages = messages
        };

        string json = JsonUtility.ToJson(requestData);
        byte[] bodyRaw = Encoding.UTF8.GetBytes(json);

        using (UnityWebRequest request = new UnityWebRequest(ApiUrl, "POST"))
        {
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Authorization", "Bearer " + apiConfig.apiKey);

            var asyncOp = request.SendWebRequest();

            while (!asyncOp.isDone)
            {
                await Task.Yield();
            }

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Error: " + request.error);
                Debug.LogError("Response: " + request.downloadHandler.text);
                return null;
            }
            else
            {
                var responseJson = JsonUtility.FromJson<ChatResponse>(request.downloadHandler.text);
                return responseJson.choices[0].message.content;
            }
        }
    }

    // New method for Vision API requests
    public async Task<string> PostVisionRequest(string imageBase64, string promptText = "Describe this image in English for language learning.")
    {
        // Updated to use GPT-4.1 nano model
        string jsonPayload = $@"{{
            ""model"": ""gpt-4.1-nano"",
            ""messages"": [
                {{
                    ""role"": ""user"",
                    ""content"": [
                        {{
                            ""type"": ""text"",
                            ""text"": ""{promptText}""
                        }},
                        {{
                            ""type"": ""image_url"",
                            ""image_url"": {{
                                ""url"": ""data:image/png;base64,{imageBase64}""
                            }}
                        }}
                    ]
                }}
            ],
            ""max_tokens"": 300
        }}";

        Debug.Log("? Sending Vision API request with model: gpt-4.1-nano");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonPayload);

        using (UnityWebRequest request = new UnityWebRequest(ApiUrl, "POST"))
        {
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Authorization", "Bearer " + apiConfig.apiKey);

            var asyncOp = request.SendWebRequest();

            while (!asyncOp.isDone)
            {
                await Task.Yield();
            }

            Debug.Log($"? Vision API Response Code: {request.responseCode}");

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Vision API Error: " + request.error);
                Debug.LogError("Vision API Response: " + request.downloadHandler.text);
                
                // Try fallback approach - use regular text model with image description
                return "I can see you've uploaded an image, but I'm having trouble analyzing it right now. Please try describing the image in text, and I'll help you learn English vocabulary related to it!";
            }
            else
            {
                Debug.Log("? Vision API request successful!");
                var responseJson = JsonUtility.FromJson<ChatResponse>(request.downloadHandler.text);
                return responseJson.choices[0].message.content;
            }
        }
    }
}

[System.Serializable]
public class ChatRequest
{
    public string model;
    public List<ChatMessage> messages;
}

[System.Serializable]
public class ChatMessage
{
    public string role;
    public string content;
    public List<MessageContent> content_array; // For vision API with mixed content
}

[System.Serializable]
public class MessageContent
{
    public string type; // "text" or "image_url"
    public string text; // For text content
    public ImageUrl image_url; // For image content
}

[System.Serializable]
public class ImageUrl
{
    public string url; // base64 data URL for images
}

[System.Serializable]
public class ChatResponse
{
    public List<Choice> choices;
}

[System.Serializable]
public class Choice
{
    public ChatMessage message;
}

[System.Serializable]
public class VisionChatRequest
{
    public string model;
    public List<ChatMessage> messages;
    public int max_tokens;
}
