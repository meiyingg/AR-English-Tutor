using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;
using UnityEngine.EventSystems; // 添加EventSystem引用

/// <summary>
/// Simple debug panel for AR applications - displays log messages on device
/// </summary>
public class DebugPanel : MonoBehaviour
{
    public static DebugPanel Instance { get; private set; }
    
    [Header("UI References")]
    public TextMeshProUGUI debugText;      // Text component to display logs
    public GameObject debugPanel;          // Panel to show/hide
    public Button toggleButton;            // Button to toggle panel visibility
    public int maxLines = 20;              // Maximum number of lines to display
    
    private Queue<string> logMessages;     // Queue to store log messages
    
    private void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            Debug.Log("[DebugPanel] Initialized singleton");
        }
        else
        {
            Debug.LogWarning("[DebugPanel] Instance already exists, destroying duplicate");
            Destroy(gameObject);
            return;
        }
        
        logMessages = new Queue<string>();
        
        // Add direct logging to console
        Debug.Log("[DebugPanel] Awake completed, debug panel is ready");
        
        // Verify UI components
        if (debugText == null)
        {
            Debug.LogError("[DebugPanel] debugText is null in Awake!");
        }
        if (debugPanel == null)
        {
            Debug.LogError("[DebugPanel] debugPanel is null in Awake!");
        }
    }
    
    private void Start()
    {
        // Setup toggle button
        if (toggleButton != null)
        {
            toggleButton.onClick.AddListener(TogglePanel);
        }
        
        // Make sure debug panel is visible on start
        if (debugPanel != null)
        {
            debugPanel.SetActive(true);
        }
        
        // Log initial message with version info
        Log("--- Debug Panel Active ---");
        Log("App Version: " + Application.version);
        Log("Unity Version: " + Application.unityVersion);
        Log("Device: " + SystemInfo.deviceModel);
        
        // Check if ARManager exists
        var arManager = FindObjectOfType<ARManager>();
        if (arManager != null)
        {
            Log("ARManager found");
            if (arManager.tutorPrefab != null)
                Log("Tutor prefab assigned: " + arManager.tutorPrefab.name);
            else
                LogError("NO TUTOR PREFAB ASSIGNED!");
        }
        else
        {
            LogError("NO ARMANAGER FOUND!");
        }
    }
    
    private void Update()
    {
        // 使用兼容新旧Input System的方式检测输入
        // 注：旧的Input API已被禁用，不使用Input.touchCount等
        
        // 通过时间戳记录，确保面板仍在工作
        if (Time.frameCount % 60 == 0) // 每秒更新一次
        {
            Log("Debug panel active - frame: " + Time.frameCount);
        }
        
        // 现在我们不尝试检测触摸，而是依赖按钮点击事件
        // 对于UI按钮点击，Unity会使用EventSystem自动处理，不受Input API变化影响
        
        UpdateDebugText(); // 每帧更新
        
        // Periodically update timestamp to show panel is alive
        if (Time.frameCount % 600 == 0) // Every ~10 seconds
        {
            // Force UI refresh to ensure everything is up to date
            UpdateDebugText();
        }
    }
    
    /// <summary>
    /// Adds a log message to the debug panel
    /// </summary>
    public void Log(string message)
    {
        // Also echo to Unity console
        Debug.Log("[DebugPanel] " + message);
        
        if (logMessages.Count >= maxLines)
        {
            logMessages.Dequeue(); // Remove oldest message
        }
        
        logMessages.Enqueue(message);
        
        // Ensure we update on the main thread
        if (gameObject.activeInHierarchy)
        {
            UpdateDebugText();
        }
    }
    
    /// <summary>
    /// Adds an error message to the debug panel
    /// </summary>
    public void LogError(string message)
    {
        Log("[ERROR] " + message);
    }
    
    /// <summary>
    /// Adds a warning message to the debug panel
    /// </summary>
    public void LogWarning(string message)
    {
        Log("[WARNING] " + message);
    }
    
    /// <summary>
    /// Toggles the visibility of the debug panel (can be called directly from UI Button)
    /// </summary>
    public void TogglePanel()
    {
        if (debugPanel != null)
        {
            debugPanel.SetActive(!debugPanel.activeInHierarchy);
            Debug.Log("Debug panel visibility toggled: " + debugPanel.activeInHierarchy);
        }
    }
    
    /// <summary>
    /// Clear all messages from the debug panel
    /// </summary>
    public void ClearLogs()
    {
        logMessages.Clear();
        UpdateDebugText();
    }
    
    /// <summary>
    /// Log raycast hit results
    /// </summary>
    public void LogRaycastHit(Vector3 hitPosition, string hitType)
    {
        Log("Raycast hit: " + hitType + " at " + hitPosition.ToString("F2"));
    }
    
    /// <summary>
    /// Log AR placement status
    /// </summary>
    public void LogPlacement(bool success, string objectName, Vector3 position)
    {
        if (success)
        {
            Log(objectName + " placed at " + position.ToString("F2"));
        }
        else
        {
            LogError("Failed to place " + objectName);
        }
    }
    
    // Update the text display with all messages in the queue
    private void UpdateDebugText()
    {
        if (debugText == null)
        {
            Debug.LogError("DebugPanel: debugText is null!");
            return;
        }
        
        // ULTRA-SIMPLIFIED FOR RELIABILITY
        string allMessages = string.Join("\n", logMessages.ToArray());
        
        // Simple timestamp to confirm updates
        allMessages = "UPDATE: " + Time.frameCount + " | " + DateTime.Now.ToString("HH:mm:ss") + "\n" + allMessages;
        
        // Direct update with no fancy formatting
        debugText.text = allMessages;
        
        try
        {
            // Force update everything possible
            debugText.ForceMeshUpdate(true);
            Canvas.ForceUpdateCanvases();
            
            if (debugPanel != null && !debugPanel.activeInHierarchy)
            {
                debugPanel.SetActive(true);  // FORCE PANEL TO BE VISIBLE FOR DEBUGGING
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Error updating debug text: " + e.Message);
        }
    }
}
