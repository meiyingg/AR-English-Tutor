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
