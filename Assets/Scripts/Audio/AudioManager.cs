using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text;
using System;

/// <summary>
/// 音频管理器 - 处理语音转文字(STT)和文字转语音(TTS)功能
/// 使用OpenAI Whisper API和TTS API
/// </summary>
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("录音设置")]
    public int recordingDuration = 10; // 最大录音时长（秒）
    public int sampleRate = 44100; // 采样率
    public KeyCode recordHotkey = KeyCode.Space; // 录音热键
    
    [Header("TTS设置")]
    public TTSVoice selectedVoice = TTSVoice.alloy; // 默认语音
    public float speechSpeed = 1.0f; // 语速 (0.25 to 4.0)
    
    [Header("音频组件")]
    public AudioSource audioSource; // 用于播放TTS音频
    
    // OpenAI API URLs
    private const string WhisperApiUrl = "https://api.openai.com/v1/audio/transcriptions";
    private const string TTSApiUrl = "https://api.openai.com/v1/audio/speech";
    
    // 录音相关
    private AudioClip recordedClip;
    private string microphoneName;
    private bool isRecording = false;
    private bool isProcessing = false;
    
    // 事件
    public System.Action<string> OnSpeechToTextResult; // STT结果回调
    public System.Action<bool> OnRecordingStateChanged; // 录音状态变化回调
    public System.Action<bool> OnTTSStateChanged; // TTS播放状态变化回调

    public enum TTSVoice
    {
        alloy,
        echo,
        fable,
        onyx,
        nova,
        shimmer
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeAudio();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializeAudio()
    {
        // 检查和请求麦克风权限（移动设备需要）
        CheckMicrophonePermission();
        
        // 设置AudioSource
        if (audioSource == null)
        {
            audioSource = gameObject.GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
        }
        
        // 配置AudioSource
        audioSource.playOnAwake = false;
        audioSource.volume = 0.8f;
        
        Debug.Log("? Audio Manager initialized successfully!");
    }
    
    private void CheckMicrophonePermission()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        // 在Android设备上检查麦克风权限
        if (!UnityEngine.Android.Permission.HasUserAuthorizedPermission(UnityEngine.Android.Permission.Microphone))
        {
            Debug.Log("Requesting microphone permission...");
            UnityEngine.Android.Permission.RequestUserPermission(UnityEngine.Android.Permission.Microphone);
        }
#endif

        // 检查可用的麦克风设备
        if (Microphone.devices.Length > 0)
        {
            microphoneName = Microphone.devices[0];
            Debug.Log($"? Audio Manager initialized with microphone: {microphoneName}");
        }
        else
        {
            Debug.LogWarning("?? No microphone devices found!");
        }
    }

    #region 语音转文字 (Speech-to-Text)

    /// <summary>
    /// 开始录音
    /// </summary>
    public void StartRecording()
    {
        if (isRecording || isProcessing)
        {
            Debug.LogWarning("Already recording or processing audio");
            return;
        }

#if UNITY_ANDROID && !UNITY_EDITOR
        // 在Android设备上再次检查权限
        if (!UnityEngine.Android.Permission.HasUserAuthorizedPermission(UnityEngine.Android.Permission.Microphone))
        {
            Debug.LogError("Microphone permission not granted!");
            OnSpeechToTextResult?.Invoke("Microphone permission required. Please grant permission in settings.");
            return;
        }
#endif

        // 检查麦克风设备
        if (string.IsNullOrEmpty(microphoneName))
        {
            // 重新检查设备
            if (Microphone.devices.Length > 0)
            {
                microphoneName = Microphone.devices[0];
                Debug.Log($"? Found microphone: {microphoneName}");
            }
            else
            {
                Debug.LogError("No microphone available!");
                OnSpeechToTextResult?.Invoke("No microphone device found.");
                return;
            }
        }

        try
        {
            isRecording = true;
            recordedClip = Microphone.Start(microphoneName, false, recordingDuration, sampleRate);
            
            if (recordedClip == null)
            {
                Debug.LogError("Failed to start microphone recording");
                isRecording = false;
                OnSpeechToTextResult?.Invoke("Failed to start recording.");
                return;
            }
            
            OnRecordingStateChanged?.Invoke(true);
            Debug.Log("? Recording started...");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error starting recording: {e.Message}");
            isRecording = false;
            OnSpeechToTextResult?.Invoke($"Recording error: {e.Message}");
        }
    }

    /// <summary>
    /// 停止录音并处理
    /// </summary>
    public async void StopRecording()
    {
        if (!isRecording)
        {
            Debug.LogWarning("Not currently recording");
            return;
        }

        isRecording = false;
        Microphone.End(microphoneName);
        
        OnRecordingStateChanged?.Invoke(false);
        Debug.Log("? Recording stopped, processing...");

        await ProcessRecordedAudio();
    }

    /// <summary>
    /// 处理录制的音频
    /// </summary>
    private async Task ProcessRecordedAudio()
    {
        if (recordedClip == null)
        {
            Debug.LogError("No recorded audio to process");
            OnSpeechToTextResult?.Invoke("No audio recorded.");
            return;
        }

        // 检查网络连接
        if (Application.internetReachability == NetworkReachability.NotReachable)
        {
            Debug.LogError("No internet connection available");
            OnSpeechToTextResult?.Invoke("No internet connection. Speech recognition requires internet.");
            isProcessing = false;
            return;
        }

        isProcessing = true;

        try
        {
            // 转换音频为WAV字节数组
            byte[] audioData = WavUtility.FromAudioClip(recordedClip);
            
            if (audioData == null || audioData.Length == 0)
            {
                Debug.LogError("Failed to convert audio to WAV format");
                OnSpeechToTextResult?.Invoke("Audio conversion failed.");
                return;
            }
            
            // 发送到Whisper API
            string transcription = await SendToWhisperAPI(audioData);
            
            if (!string.IsNullOrEmpty(transcription))
            {
                Debug.Log($"? Transcription: {transcription}");
                OnSpeechToTextResult?.Invoke(transcription);
            }
            else
            {
                Debug.LogError("Failed to get transcription");
                OnSpeechToTextResult?.Invoke("");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error processing audio: {e.Message}");
            OnSpeechToTextResult?.Invoke("");
        }
        finally
        {
            isProcessing = false;
        }
    }

    /// <summary>
    /// 发送音频到Whisper API
    /// </summary>
    private async Task<string> SendToWhisperAPI(byte[] audioData)
    {
        if (OpenAIManager.Instance == null || OpenAIManager.Instance.apiConfig == null || 
            string.IsNullOrEmpty(OpenAIManager.Instance.apiConfig.apiKey))
        {
            Debug.LogError("OpenAIManager or API Key is missing!");
            return "";
        }

        WWWForm form = new WWWForm();
        form.AddBinaryData("file", audioData, "audio.wav", "audio/wav");
        form.AddField("model", "whisper-1");
        form.AddField("language", "en"); // 固定英文识别

        using (UnityWebRequest request = UnityWebRequest.Post(WhisperApiUrl, form))
        {
            request.SetRequestHeader("Authorization", "Bearer " + OpenAIManager.Instance.apiConfig.apiKey);

            var asyncOp = request.SendWebRequest();
            while (!asyncOp.isDone)
            {
                await Task.Yield();
            }

            if (request.result == UnityWebRequest.Result.Success)
            {
                try
                {
                    var response = JsonUtility.FromJson<WhisperResponse>(request.downloadHandler.text);
                    return response.text;
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"Error parsing Whisper response: {e.Message}");
                    return "";
                }
            }
            else
            {
                Debug.LogError($"Whisper API Error: {request.error}");
                Debug.LogError($"Response: {request.downloadHandler.text}");
                return "";
            }
        }
    }

    #endregion

    #region 文字转语音 (Text-to-Speech)

    /// <summary>
    /// 将文本转换为语音并播放
    /// </summary>
    public async Task SpeakText(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            Debug.LogWarning("No text to speak");
            return;
        }

        if (isProcessing)
        {
            Debug.LogWarning("Audio is currently being processed, please wait");
            return;
        }

        // 检查网络连接
        if (Application.internetReachability == NetworkReachability.NotReachable)
        {
            Debug.LogError("No internet connection available for TTS");
            return;
        }

        isProcessing = true;
        OnTTSStateChanged?.Invoke(true);

        try
        {
            Debug.Log($"? Converting text to speech: {text.Substring(0, Mathf.Min(50, text.Length))}...");
            
            byte[] audioData = await SendToTTSAPI(text);
            
            if (audioData != null && audioData.Length > 0)
            {
                await PlayTTSAudio(audioData);
            }
            else
            {
                Debug.LogError("Failed to get TTS audio data");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error in text-to-speech: {e.Message}");
        }
        finally
        {
            isProcessing = false;
            OnTTSStateChanged?.Invoke(false);
        }
    }

    /// <summary>
    /// 发送文本到TTS API
    /// </summary>
    private async Task<byte[]> SendToTTSAPI(string text)
    {
        if (OpenAIManager.Instance == null || OpenAIManager.Instance.apiConfig == null || 
            string.IsNullOrEmpty(OpenAIManager.Instance.apiConfig.apiKey))
        {
            Debug.LogError("OpenAIManager or API Key is missing!");
            return null;
        }

        var requestData = new TTSRequest
        {
            model = "tts-1",
            input = text,
            voice = selectedVoice.ToString(),
            speed = speechSpeed
        };

        string json = JsonUtility.ToJson(requestData);
        byte[] bodyRaw = Encoding.UTF8.GetBytes(json);

        using (UnityWebRequest request = new UnityWebRequest(TTSApiUrl, "POST"))
        {
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Authorization", "Bearer " + OpenAIManager.Instance.apiConfig.apiKey);

            var asyncOp = request.SendWebRequest();
            while (!asyncOp.isDone)
            {
                await Task.Yield();
            }

            if (request.result == UnityWebRequest.Result.Success)
            {
                return request.downloadHandler.data;
            }
            else
            {
                Debug.LogError($"TTS API Error: {request.error}");
                Debug.LogError($"Response: {request.downloadHandler.text}");
                return null;
            }
        }
    }

    /// <summary>
    /// 播放TTS音频
    /// </summary>
    private async Task PlayTTSAudio(byte[] audioData)
    {
        try
        {
            // 将MP3数据转换为AudioClip
            AudioClip clip = await ConvertMP3ToAudioClip(audioData);
            
            if (clip != null)
            {
                audioSource.clip = clip;
                audioSource.Play();
                
                Debug.Log($"? Playing TTS audio, duration: {clip.length:F1}s");
                
                // 等待播放完成
                while (audioSource.isPlaying)
                {
                    await Task.Yield();
                }
                
                Debug.Log("? TTS audio playback completed");
            }
            else
            {
                Debug.LogError("Failed to convert audio data to AudioClip");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error playing TTS audio: {e.Message}");
        }
    }

    /// <summary>
    /// 将MP3字节数组转换为AudioClip
    /// </summary>
    private async Task<AudioClip> ConvertMP3ToAudioClip(byte[] mp3Data)
    {
        // Unity不能直接播放MP3，这里我们需要用WAV格式
        // 对于简单实现，我们可以先保存为临时文件然后加载
        // 更复杂的方案是使用第三方库进行实时转换
        
        string tempPath = System.IO.Path.Combine(Application.temporaryCachePath, "temp_tts.mp3");
        
        try
        {
            System.IO.File.WriteAllBytes(tempPath, mp3Data);
            
            // 使用UnityWebRequest加载音频文件
            using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip("file://" + tempPath, AudioType.MPEG))
            {
                var asyncOp = www.SendWebRequest();
                while (!asyncOp.isDone)
                {
                    await Task.Yield();
                }

                if (www.result == UnityWebRequest.Result.Success)
                {
                    AudioClip clip = DownloadHandlerAudioClip.GetContent(www);
                    return clip;
                }
                else
                {
                    Debug.LogError($"Failed to load audio file: {www.error}");
                    return null;
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error converting MP3 to AudioClip: {e.Message}");
            return null;
        }
        finally
        {
            // 清理临时文件
            if (System.IO.File.Exists(tempPath))
            {
                System.IO.File.Delete(tempPath);
            }
        }
    }

    #endregion

    #region 公共方法

    /// <summary>
    /// 切换录音状态
    /// </summary>
    public void ToggleRecording()
    {
        if (isRecording)
        {
            StopRecording();
        }
        else
        {
            StartRecording();
        }
    }

    /// <summary>
    /// 停止当前播放的语音
    /// </summary>
    public void StopSpeaking()
    {
        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Stop();
            OnTTSStateChanged?.Invoke(false);
            Debug.Log("? TTS playback stopped");
        }
    }

    /// <summary>
    /// 检查是否有可用的麦克风
    /// </summary>
    public bool HasMicrophone()
    {
        return !string.IsNullOrEmpty(microphoneName);
    }

    /// <summary>
    /// 检查是否正在录音
    /// </summary>
    public bool IsRecording()
    {
        return isRecording;
    }

    /// <summary>
    /// 检查是否正在处理音频
    /// </summary>
    public bool IsProcessing()
    {
        return isProcessing;
    }

    /// <summary>
    /// 检查是否正在播放语音
    /// </summary>
    public bool IsSpeaking()
    {
        return audioSource != null && audioSource.isPlaying;
    }

    #endregion

    #region 数据结构

    [System.Serializable]
    public class WhisperResponse
    {
        public string text;
    }

    [System.Serializable]
    public class TTSRequest
    {
        public string model;
        public string input;
        public string voice;
        public float speed;
    }

    #endregion
}

/// <summary>
/// WAV工具类 - 用于音频格式转换
/// </summary>
public static class WavUtility
{
    const int HEADER_SIZE = 44;

    public static byte[] FromAudioClip(AudioClip audioClip)
    {
        if (audioClip == null)
            return null;

        var samples = new float[audioClip.samples * audioClip.channels];
        audioClip.GetData(samples, 0);

        var data = new byte[HEADER_SIZE + samples.Length * 2];
        
        // WAV header
        WriteHeader(data, audioClip.frequency, audioClip.channels, samples.Length);
        
        // Convert samples to 16-bit PCM
        for (int i = 0; i < samples.Length; i++)
        {
            var sample = Mathf.Clamp(samples[i], -1f, 1f);
            var value = (short)(sample * short.MaxValue);
            var bytes = System.BitConverter.GetBytes(value);
            data[HEADER_SIZE + i * 2] = bytes[0];
            data[HEADER_SIZE + i * 2 + 1] = bytes[1];
        }

        return data;
    }

    private static void WriteHeader(byte[] data, int sampleRate, int channels, int samples)
    {
        var hz = sampleRate;
        var bits = 16;
        var length = samples * 2;

        // RIFF header
        System.Text.Encoding.ASCII.GetBytes("RIFF").CopyTo(data, 0);
        System.BitConverter.GetBytes(length + HEADER_SIZE - 8).CopyTo(data, 4);
        System.Text.Encoding.ASCII.GetBytes("WAVE").CopyTo(data, 8);

        // Format chunk
        System.Text.Encoding.ASCII.GetBytes("fmt ").CopyTo(data, 12);
        System.BitConverter.GetBytes(16).CopyTo(data, 16);
        System.BitConverter.GetBytes((ushort)1).CopyTo(data, 20);
        System.BitConverter.GetBytes((ushort)channels).CopyTo(data, 22);
        System.BitConverter.GetBytes(hz).CopyTo(data, 24);
        System.BitConverter.GetBytes(hz * channels * bits / 8).CopyTo(data, 28);
        System.BitConverter.GetBytes((ushort)(channels * bits / 8)).CopyTo(data, 32);
        System.BitConverter.GetBytes((ushort)bits).CopyTo(data, 34);

        // Data chunk
        System.Text.Encoding.ASCII.GetBytes("data").CopyTo(data, 36);
        System.BitConverter.GetBytes(length).CopyTo(data, 40);
    }
}
