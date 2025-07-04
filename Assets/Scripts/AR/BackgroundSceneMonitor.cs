using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using Unity.Collections;
using System;

/// <summary>
/// Manages background scene monitoring for the AR English tutor.
/// This class continuously captures the AR camera view at regular intervals 
/// to create the illusion that the AI can "see" the real world continuously.
/// </summary>
public class BackgroundSceneMonitor : MonoBehaviour
{
    public static BackgroundSceneMonitor Instance { get; private set; }

    [Header("Capture Settings")]
    [Tooltip("How often to capture the scene (in seconds)")]
    public float captureInterval = 10.0f;
    
    [Tooltip("Resolution scale for captured images (1.0 = full resolution, 0.5 = half resolution)")]
    [Range(0.1f, 1.0f)]
    public float resolutionScale = 0.5f;
    
    [Tooltip("Whether to automatically start monitoring on app launch")]
    public bool startAutomatically = true;
    
    [Header("Performance Settings")]
    [Tooltip("Whether to pause monitoring when app is in background")]
    public bool pauseWhenInBackground = true;
    
    [Tooltip("Whether to pause monitoring when battery is low (below 15%)")]
    public bool pauseOnLowBattery = true;
    
    // References
    private ARCameraManager arCameraManager;
    private ChatManager chatManager;
    
    // State management
    private bool isMonitoring = false;
    private bool isPaused = false;
    private bool isProcessingImage = false;
    private float timeSinceLastCapture = 0f;
    
    // Scene memory
    private List<SceneMemory> recentScenes = new List<SceneMemory>();
    private int maxScenesRemembered = 5;
    
    // Current scene state
    [System.Serializable]
    public class SceneMemory
    {
        public string base64Image;
        public Texture2D texture;
        public Vector3 cameraPosition;
        public Quaternion cameraRotation;
        public DateTime captureTime;
        public string description; // May be filled in later after AI analysis
    }
    
    // Events
    public event Action<Texture2D> OnSceneCaptured;
    public event Action<string> OnSceneAnalysisComplete;

    private void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        
        // Find required components
        arCameraManager = FindObjectOfType<ARCameraManager>();
        chatManager = FindObjectOfType<ChatManager>();
        
        if (arCameraManager == null)
        {
            Debug.LogError("BackgroundSceneMonitor: ARCameraManager not found in the scene!");
        }
        
        if (chatManager == null)
        {
            Debug.LogWarning("BackgroundSceneMonitor: ChatManager not found in the scene!");
        }
    }

    private void Start()
    {
        if (startAutomatically)
        {
            StartMonitoring();
        }
        
        // Log to debug panel if available
        if (DebugPanel.Instance != null)
        {
            DebugPanel.Instance.Log("Background Scene Monitor initialized");
            if (startAutomatically)
            {
                DebugPanel.Instance.Log("Auto-monitoring enabled");
            }
        }
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        if (pauseWhenInBackground)
        {
            // Pause monitoring when app loses focus
            if (!hasFocus)
            {
                PauseMonitoring();
            }
            else if (isMonitoring) // Only resume if monitoring was active
            {
                ResumeMonitoring();
            }
        }
    }

    private void Update()
    {
        if (!isMonitoring || isPaused || isProcessingImage)
            return;
            
        // Check battery level if needed
        if (pauseOnLowBattery && SystemInfo.batteryLevel < 0.15f && SystemInfo.batteryStatus != BatteryStatus.Charging)
        {
            PauseMonitoring();
            Debug.LogWarning("Scene monitoring paused due to low battery");
            return;
        }
        
        // Update capture timer
        timeSinceLastCapture += Time.deltaTime;
        
        // Capture image at regular intervals
        if (timeSinceLastCapture >= captureInterval)
        {
            CaptureScene();
            timeSinceLastCapture = 0f;
        }
    }

    /// <summary>
    /// Starts the background monitoring of the environment
    /// </summary>
    public void StartMonitoring()
    {
        if (isMonitoring)
            return;
            
        isMonitoring = true;
        isPaused = false;
        timeSinceLastCapture = captureInterval; // Trigger first capture immediately
        
        Debug.Log("Background scene monitoring started");
        
        // Log to debug panel if available
        if (DebugPanel.Instance != null)
        {
            DebugPanel.Instance.Log("Scene monitoring started");
        }
    }

    /// <summary>
    /// Stops the background monitoring
    /// </summary>
    public void StopMonitoring()
    {
        if (!isMonitoring)
            return;
            
        isMonitoring = false;
        isPaused = false;
        
        Debug.Log("Background scene monitoring stopped");
        
        // Log to debug panel if available
        if (DebugPanel.Instance != null)
        {
            DebugPanel.Instance.Log("Scene monitoring stopped");
        }
    }

    /// <summary>
    /// Temporarily pauses monitoring
    /// </summary>
    public void PauseMonitoring()
    {
        if (!isMonitoring || isPaused)
            return;
            
        isPaused = true;
        Debug.Log("Scene monitoring paused");
    }

    /// <summary>
    /// Resumes monitoring after a pause
    /// </summary>
    public void ResumeMonitoring()
    {
        if (!isMonitoring || !isPaused)
            return;
            
        isPaused = false;
        Debug.Log("Scene monitoring resumed");
    }

    /// <summary>
    /// Captures the current scene from the AR camera
    /// </summary>
    public void CaptureScene()
    {
        if (arCameraManager == null || isProcessingImage)
            return;
            
        StartCoroutine(CaptureSceneCoroutine());
    }
    
    private IEnumerator CaptureSceneCoroutine()
    {
        isProcessingImage = true;
        
        // Attempt to get the latest camera image
        XRCpuImage cpuImage;
        if (!arCameraManager.TryAcquireLatestCpuImage(out cpuImage))
        {
            Debug.LogWarning("Failed to acquire AR camera image");
            isProcessingImage = false;
            yield break;
        }
        
        try
        {
            // Setup conversion parameters
            int width = Mathf.RoundToInt(cpuImage.width * resolutionScale);
            int height = Mathf.RoundToInt(cpuImage.height * resolutionScale);
            
            var conversionParams = new XRCpuImage.ConversionParams
            {
                inputRect = new RectInt(0, 0, cpuImage.width, cpuImage.height),
                outputDimensions = new Vector2Int(width, height),
                outputFormat = TextureFormat.RGB24,
                transformation = XRCpuImage.Transformation.MirrorY
            };

            // Create a texture and convert the image
            int size = cpuImage.GetConvertedDataSize(conversionParams);
            var buffer = new NativeArray<byte>(size, Allocator.Temp);
            
            cpuImage.Convert(conversionParams, buffer);
            
            // Create a texture from the buffer
            Texture2D texture = new Texture2D(width, height, TextureFormat.RGB24, false);
            texture.LoadRawTextureData(buffer);
            texture.Apply();
            
            // Convert to base64 for storage and API transmission
            string base64Image = Convert.ToBase64String(texture.EncodeToPNG());
            
            // Create scene memory
            SceneMemory newScene = new SceneMemory
            {
                base64Image = base64Image,
                texture = texture,
                cameraPosition = Camera.main.transform.position,
                cameraRotation = Camera.main.transform.rotation,
                captureTime = DateTime.Now
            };
            
            // Add to recent scenes
            AddSceneToMemory(newScene);
            
            // Notify subscribers that a scene was captured
            OnSceneCaptured?.Invoke(texture);
            
            // Log capture (in detailed log mode only)
            Debug.Log($"Scene captured: {width}x{height} at {DateTime.Now.ToString("HH:mm:ss")}");
            
            // Clean up the buffer
            buffer.Dispose();
        }
        catch (Exception e)
        {
            Debug.LogError($"Error in scene capture: {e.Message}");
        }
        finally
        {
            // Always dispose the CPU image
            cpuImage.Dispose();
            isProcessingImage = false;
        }
    }
    
    /// <summary>
    /// Gets the most recent captured scene
    /// </summary>
    public SceneMemory GetLatestScene()
    {
        if (recentScenes.Count > 0)
        {
            return recentScenes[recentScenes.Count - 1];
        }
        return null;
    }
    
    /// <summary>
    /// Gets all recent scene memories
    /// </summary>
    public List<SceneMemory> GetRecentScenes()
    {
        return recentScenes;
    }
    
    /// <summary>
    /// Clears all stored scene memories
    /// </summary>
    public void ClearSceneMemory()
    {
        foreach (var scene in recentScenes)
        {
            // Destroy texture to free memory
            if (scene.texture != null)
            {
                Destroy(scene.texture);
            }
        }
        
        recentScenes.Clear();
    }
    
    private void AddSceneToMemory(SceneMemory scene)
    {
        // Add new scene
        recentScenes.Add(scene);
        
        // Remove oldest scenes if we exceed our limit
        while (recentScenes.Count > maxScenesRemembered)
        {
            // Destroy texture to free memory
            if (recentScenes[0].texture != null)
            {
                Destroy(recentScenes[0].texture);
            }
            recentScenes.RemoveAt(0);
        }
    }
    
    /// <summary>
    /// Forces an immediate scene capture and analysis
    /// </summary>
    public void CaptureAndAnalyzeScene(string userQuery = "")
    {
        StartCoroutine(CaptureAndAnalyzeSceneCoroutine(userQuery));
    }
    
    private IEnumerator CaptureAndAnalyzeSceneCoroutine(string userQuery)
    {
        // Make sure we aren't already processing
        if (isProcessingImage)
        {
            yield return new WaitUntil(() => !isProcessingImage);
        }
        
        // Capture a new scene
        yield return CaptureSceneCoroutine();
        
        SceneMemory latestScene = GetLatestScene();
        if (latestScene == null || string.IsNullOrEmpty(latestScene.base64Image))
        {
            Debug.LogError("Failed to capture scene for analysis");
            yield break;
        }
        
        // Send to ChatManager for analysis
        if (chatManager != null)
        {
            yield return chatManager.SendSceneRecognitionRequest(
                latestScene.base64Image, 
                latestScene.texture, 
                userQuery,
                true); // Background scene monitor triggers are always first interactions
        }
    }
    
    private void OnDestroy()
    {
        // Clean up resources
        ClearSceneMemory();
    }
}
