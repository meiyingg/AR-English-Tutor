using UnityEngine;
using System;
using System.IO;

public class ImageUploadManager : MonoBehaviour
{
    public static ImageUploadManager Instance { get; private set; }
    
    [Header("Image Settings")]
    public int maxImageSize = 2048; // Max width/height in pixels
    public int maxFileSizeKB = 2048; // Max file size in KB
    
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
    
    public void SelectImageFromGallery(System.Action<string, Texture2D> onImageSelected)
    {
        // For now, we'll use a simple file dialog approach
        // In a real mobile app, you'd use NativeGallery plugin
        
#if UNITY_EDITOR
        // Editor version - use file dialog
        string path = UnityEditor.EditorUtility.OpenFilePanel("Select Image", "", "png,jpg,jpeg");
        if (!string.IsNullOrEmpty(path))
        {
            ProcessSelectedImage(path, onImageSelected);
        }
#elif UNITY_ANDROID || UNITY_IOS
        // Mobile version - would need NativeGallery plugin
        Debug.LogWarning("Mobile image selection requires NativeGallery plugin");
        // For demo purposes, we'll simulate image selection
        SimulateImageSelection(onImageSelected);
#else
        Debug.LogWarning("Image selection not supported on this platform");
#endif
    }
    
    private void ProcessSelectedImage(string imagePath, System.Action<string, Texture2D> onImageSelected)
    {
        try
        {
            byte[] imageData = File.ReadAllBytes(imagePath);
            
            // Check file size
            if (imageData.Length > maxFileSizeKB * 1024)
            {
                Debug.LogWarning("Image file too large. Please select a smaller image.");
                return;
            }
            
            // Load as texture
            Texture2D texture = new Texture2D(2, 2);
            if (texture.LoadImage(imageData))
            {
                // Resize if necessary
                texture = ResizeTexture(texture, maxImageSize, maxImageSize);
                
                // Convert to base64
                string base64 = ConvertToBase64(texture);
                
                onImageSelected?.Invoke(base64, texture);
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error processing image: {e.Message}");
        }
    }
    
    private void SimulateImageSelection(System.Action<string, Texture2D> onImageSelected)
    {
        // Create a simple test image for demo
        Texture2D testTexture = new Texture2D(256, 256);
        Color[] colors = new Color[256 * 256];
        for (int i = 0; i < colors.Length; i++)
        {
            colors[i] = new Color(UnityEngine.Random.value, UnityEngine.Random.value, UnityEngine.Random.value, 1f);
        }
        testTexture.SetPixels(colors);
        testTexture.Apply();
        
        string base64 = ConvertToBase64(testTexture);
        onImageSelected?.Invoke(base64, testTexture);
    }
    
    private Texture2D ResizeTexture(Texture2D source, int maxWidth, int maxHeight)
    {
        if (source.width <= maxWidth && source.height <= maxHeight)
            return source;
            
        float aspectRatio = (float)source.width / source.height;
        int newWidth, newHeight;
        
        if (aspectRatio > 1) // Wider than tall
        {
            newWidth = maxWidth;
            newHeight = Mathf.RoundToInt(maxWidth / aspectRatio);
        }
        else // Taller than wide
        {
            newHeight = maxHeight;
            newWidth = Mathf.RoundToInt(maxHeight * aspectRatio);
        }
        
        RenderTexture rt = RenderTexture.GetTemporary(newWidth, newHeight);
        Graphics.Blit(source, rt);
        
        RenderTexture previous = RenderTexture.active;
        RenderTexture.active = rt;
        
        Texture2D resized = new Texture2D(newWidth, newHeight);
        resized.ReadPixels(new Rect(0, 0, newWidth, newHeight), 0, 0);
        resized.Apply();
        
        RenderTexture.active = previous;
        RenderTexture.ReleaseTemporary(rt);
        
        return resized;
    }
    
    public string ConvertToBase64(Texture2D texture)
    {
        byte[] imageData = texture.EncodeToPNG();
        return System.Convert.ToBase64String(imageData);
    }
    
    public Texture2D Base64ToTexture(string base64)
    {
        byte[] imageData = System.Convert.FromBase64String(base64);
        Texture2D texture = new Texture2D(2, 2);
        texture.LoadImage(imageData);
        return texture;
    }
}
