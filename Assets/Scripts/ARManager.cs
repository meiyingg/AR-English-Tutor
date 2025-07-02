using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ARManager : MonoBehaviour
{
    [Header("Placement Settings")]
    [Tooltip("The prefab for the AI Tutor to be placed.")]
    public GameObject tutorPrefab;

    private ARRaycastManager arRaycastManager;
    private static List<ARRaycastHit> hits = new List<ARRaycastHit>();
    private GameObject placedTutorInstance;
    public static Vector2 lastTouchPosition;
    public static Vector3 lastHitPosition;

    private void Awake()
    {
        arRaycastManager = GetComponent<ARRaycastManager>();
        
        if (arRaycastManager == null)
        {
            Debug.LogError("ARRaycastManager component missing!");
            if (DebugPanel.Instance != null)
                DebugPanel.Instance.LogError("ARRaycastManager missing!");
        }
        
        if (tutorPrefab == null)
        {
            Debug.LogError("Tutor prefab not assigned!");
            if (DebugPanel.Instance != null)
                DebugPanel.Instance.LogError("Tutor prefab not assigned!");
        }
    }
    
    private void Start()
    {
        if (DebugPanel.Instance != null)
        {
            DebugPanel.Instance.Log("ARManager initialized");
            DebugPanel.Instance.Log("Touch screen to place tutor");
            
            // Check AR components
            var arSession = FindObjectOfType<UnityEngine.XR.ARFoundation.ARSession>();
            if (arSession != null)
                DebugPanel.Instance.Log("ARSession found");
            else
                DebugPanel.Instance.LogError("No ARSession found!");
                
            var xrOrigin = FindObjectOfType<Unity.XR.CoreUtils.XROrigin>();
            if (xrOrigin != null)
                DebugPanel.Instance.Log("XROrigin found");
            else
            {
                // For backward compatibility
                var arSessionOrigin = FindObjectOfType<UnityEngine.XR.ARFoundation.ARSessionOrigin>();
                if (arSessionOrigin != null)
                    DebugPanel.Instance.Log("ARSessionOrigin found (legacy)");
                else
                    DebugPanel.Instance.LogError("No XROrigin found!");
            }
                
            var arPlaneManager = FindObjectOfType<UnityEngine.XR.ARFoundation.ARPlaneManager>();
            if (arPlaneManager != null)
                DebugPanel.Instance.Log("ARPlaneManager found");
            else
                DebugPanel.Instance.LogError("No ARPlaneManager found!");
        }
    }
    

    void Update()
    {
        // Early exit if tutor is already placed
        if (placedTutorInstance != null)
        {
            return;
        }

        // Constantly check input status and AR status
        if (Time.frameCount % 300 == 0) // Log every ~5 seconds
        {
            if (DebugPanel.Instance != null)
            {
                DebugPanel.Instance.Log(">>> INPUT STATUS CHECK <<<");
                DebugPanel.Instance.Log("Touch count: " + Input.touchCount);
                DebugPanel.Instance.Log("Mouse buttons: " + 
                    (Input.GetMouseButton(0) ? "L" : "") + 
                    (Input.GetMouseButton(1) ? "R" : "") +
                    (Input.GetMouseButton(2) ? "M" : ""));
            }
        }
        
        // Check for touch input or mouse input (for desktop testing)
        bool inputDetected = false;
        Vector2 inputPosition = Vector2.zero;
        
        // Handle touch input
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            Touch touch = Input.GetTouch(0);
            inputDetected = true;
            inputPosition = touch.position;
            lastTouchPosition = touch.position;
            
            if (DebugPanel.Instance != null)
            {
                DebugPanel.Instance.LogError("!!TOUCH DETECTED!!"); // Make this super obvious
                DebugPanel.Instance.Log("Touch detected at: " + inputPosition);
            }
        }
        // Handle mouse input (for desktop testing)
        else if (Input.GetMouseButtonDown(0))
        {
            inputDetected = true;
            inputPosition = Input.mousePosition;
            lastTouchPosition = Input.mousePosition;
            
            if (DebugPanel.Instance != null)
            {
                DebugPanel.Instance.LogError("!!MOUSE CLICK DETECTED!!"); // Make this super obvious
                DebugPanel.Instance.Log("Mouse click detected at: " + inputPosition);
            }
        }
        
        if (inputDetected)
        {
            // 基本的UI点击检测 - 使用最直接的方法
            bool isOverUI = false;
            
            // 方法1：简单的IsPointerOverGameObject
            if (Input.touchCount > 0)
            {
                isOverUI = EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId);
            }
            else
            {
                isOverUI = EventSystem.current.IsPointerOverGameObject();
            }
            
            if (DebugPanel.Instance != null)
            {
                DebugPanel.Instance.LogError("Basic UI detection: " + (isOverUI ? "UI" : "Scene"));
            }
            
            if (isOverUI)
            {
                if (DebugPanel.Instance != null)
                {
                    DebugPanel.Instance.LogError("Clicked on UI!");
                    DebugPanel.Instance.LogError("UI click position: " + inputPosition);
                }
                return;
            }
            
            // IMPORTANT: Log that we're proceeding with scene touch
            if (DebugPanel.Instance != null)
                DebugPanel.Instance.LogError("TOUCH ON SCENE - PROCEEDING!");
            
            // Verify ARRaycastManager is valid
            if (arRaycastManager == null)
            {
                Debug.LogError("ARRaycastManager is null!");
                if (DebugPanel.Instance != null)
                    DebugPanel.Instance.LogError("ARRaycastManager is null!");
                return;
            }
            
            if (DebugPanel.Instance != null)
                DebugPanel.Instance.Log("Attempting raycast...");
            
            // Clear previous hits
            hits.Clear();
            
            // Perform raycast
            bool didHit = false;
            try
            {
                if (DebugPanel.Instance != null)
                    DebugPanel.Instance.LogError("ATTEMPTING RAYCAST..."); // Very visible message
                
                // First try with PlaneWithinPolygon
                didHit = arRaycastManager.Raycast(inputPosition, hits, TrackableType.PlaneWithinPolygon);
                
                // If first attempt failed, try with a broader trackable type
                if (!didHit || hits.Count == 0)
                {
                    if (DebugPanel.Instance != null)
                        DebugPanel.Instance.Log("First raycast failed, trying with AllTypes...");
                        
                    // Try with all trackable types
                    didHit = arRaycastManager.Raycast(inputPosition, hits, TrackableType.AllTypes);
                }
                
                if (DebugPanel.Instance != null)
                {
                    DebugPanel.Instance.LogError("RAYCAST COMPLETE!"); // Very visible message
                    DebugPanel.Instance.Log("Raycast result: " + (didHit ? "HIT" : "MISS") + " (hits: " + hits.Count + ")");
                    
                    // Check if planes are detected at all
                    var arPlaneManager = FindObjectOfType<UnityEngine.XR.ARFoundation.ARPlaneManager>();
                    if (arPlaneManager != null)
                    {
                        int planeCount = arPlaneManager.trackables.count;
                        DebugPanel.Instance.LogError("DETECTED PLANES: " + planeCount); // Make this very visible
                        
                        // If no planes detected, that's likely the issue
                        if (planeCount == 0)
                        {
                            DebugPanel.Instance.LogError("NO PLANES DETECTED - MOVE DEVICE TO SCAN!");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                if (DebugPanel.Instance != null)
                {
                    DebugPanel.Instance.LogError("RAYCAST EXCEPTION: " + ex.Message);
                }
                Debug.LogException(ex);
            }
                
            if (didHit && hits.Count > 0)
            {
                var hitPose = hits[0].pose;
                lastHitPosition = hitPose.position;
                
                Debug.Log("Hit at: " + hitPose.position.ToString("F2"));
                if (DebugPanel.Instance != null)
                {
                    DebugPanel.Instance.Log("Hit position: " + hitPose.position.ToString("F2"));
                    DebugPanel.Instance.Log("Hit trackable: " + hits[0].trackable.name);
                    DebugPanel.Instance.Log("Attempting to place tutor...");
                }
                
                // Place the tutor at the hit position
                PlaceTutor(hitPose);
            }
            else
            {
                Debug.LogWarning("Touch but no plane hit: " + lastTouchPosition);
                if (DebugPanel.Instance != null)
                {
                    DebugPanel.Instance.LogWarning("No plane detected at touch point");
                    DebugPanel.Instance.Log("Make sure to scan the environment first");
                }
            }
        }
    }

    private void PlaceTutor(Pose hitPose)
    {
        if (DebugPanel.Instance != null)
        {
            DebugPanel.Instance.LogError("PLACE TUTOR CALLED!"); // Very visible
            DebugPanel.Instance.Log("PlaceTutor called at: " + hitPose.position.ToString("F2"));
            DebugPanel.Instance.Log("Pose rotation: " + hitPose.rotation.eulerAngles.ToString("F1"));
        }
            
        if (placedTutorInstance == null)
        {
            if (tutorPrefab == null)
            {
                Debug.LogError("Tutor prefab is null!");
                if (DebugPanel.Instance != null)
                {
                    DebugPanel.Instance.LogError("CRITICAL ERROR: Tutor prefab is null!");
                    DebugPanel.Instance.LogError("Check ARManager Inspector settings");
                }
                return;
            }
            
            if (DebugPanel.Instance != null)
            {
                DebugPanel.Instance.LogError("PREFAB CHECK PASSED!"); // Very visible
                DebugPanel.Instance.Log("Tutor prefab name: " + tutorPrefab.name);
                // Log prefab details
                DebugPanel.Instance.Log("Prefab active: " + tutorPrefab.activeSelf);
                DebugPanel.Instance.Log("Prefab layer: " + tutorPrefab.layer);
                DebugPanel.Instance.Log("Prefab tag: " + tutorPrefab.tag);
            }
            
            try
            {
                if (DebugPanel.Instance != null)
                {
                    DebugPanel.Instance.LogError("INSTANTIATING TUTOR..."); // Very visible
                    DebugPanel.Instance.Log("Instantiating tutor at " + hitPose.position.ToString("F2"));
                }
                    
                // Try to instantiate with parent = null explicitly
                placedTutorInstance = Instantiate(tutorPrefab, hitPose.position, hitPose.rotation, null);
                
                if (placedTutorInstance != null)
                {
                    Debug.Log("Tutor placed successfully!");
                    if (DebugPanel.Instance != null)
                    {
                        DebugPanel.Instance.LogError("SUCCESS: TUTOR PLACED!"); // Very visible
                        DebugPanel.Instance.Log("Tutor placed at " + hitPose.position.ToString("F2"));
                        DebugPanel.Instance.Log("Tutor GameObject name: " + placedTutorInstance.name);
                        DebugPanel.Instance.Log("Tutor active: " + placedTutorInstance.activeSelf);
                        DebugPanel.Instance.Log("Tutor layer: " + placedTutorInstance.layer);
                        DebugPanel.Instance.Log("Tutor components: " + placedTutorInstance.GetComponents<Component>().Length);
                    }
                    InitializeTutor(placedTutorInstance);
                }
                else
                {
                    Debug.LogError("Failed to instantiate tutor");
                    if (DebugPanel.Instance != null)
                    {
                        DebugPanel.Instance.LogError("CRITICAL: INSTANTIATE RETURNED NULL");
                        DebugPanel.Instance.LogError("Check tutor prefab for errors");
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogException(e);
                if (DebugPanel.Instance != null)
                {
                    DebugPanel.Instance.LogError("EXCEPTION during instantiation:");
                    DebugPanel.Instance.LogError(e.Message);
                    if (e.InnerException != null)
                        DebugPanel.Instance.LogError(e.InnerException.Message);
                }
            }
        }
        else
        {
            if (DebugPanel.Instance != null)
                DebugPanel.Instance.Log("Tutor already placed, ignoring");
        }
    }

    /// <summary>
    /// Initializes the placed tutor, sets its orientation and animations, and configures the chat UI.
    /// </summary>
    /// <param name="tutorInstance">The instance of the tutor GameObject.</param>
    public void InitializeTutor(GameObject tutorInstance)
    {
        if (DebugPanel.Instance != null)
            DebugPanel.Instance.Log("Initializing tutor...");
            
        // Make the tutor look at the user (without tilting up/down)
        Vector3 lookAtPosition = new Vector3(Camera.main.transform.position.x, tutorInstance.transform.position.y, Camera.main.transform.position.z);
        tutorInstance.transform.LookAt(lookAtPosition);

        // Play the waving animation by default
        var animatorController = tutorInstance.GetComponent<ARTutorAnimatorController>();
        if (animatorController != null)
        {
            animatorController.PlayWaving();
            if (DebugPanel.Instance != null)
                DebugPanel.Instance.Log("Playing waving animation");
        }
        else
        {
            if (DebugPanel.Instance != null)
                DebugPanel.Instance.LogWarning("No animator controller found");
        }

        // Find the world space container and assign it to the UI Manager
        Transform chatContainer = tutorInstance.transform.Find("ChatBubbleAnchor");
        if (chatContainer != null && ChatTestUI.Instance != null)
        {
            ChatTestUI.Instance.SetWorldSpaceChatContainer(chatContainer);
            if (DebugPanel.Instance != null)
                DebugPanel.Instance.Log("Chat container setup complete");
        }
        else
        {
            if (DebugPanel.Instance != null)
                DebugPanel.Instance.LogWarning("Chat container setup failed");
        }
        
        if (DebugPanel.Instance != null)
            DebugPanel.Instance.Log("Tutor initialization complete");
    }
}
