using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Creates a colorful breathing light effect for UI elements.
/// Add this script to any GameObject with an Image component to create
/// a rainbow breathing effect that smoothly transitions between colors.
/// </summary>
[RequireComponent(typeof(Image))]
public class ColorfulBreathingEffect : MonoBehaviour
{
    [Header("Color Settings")]
    [Tooltip("Enable/disable the breathing effect")]
    public bool isActive = true;
    
    [Tooltip("Speed of the color transition (higher = faster)")]
    [Range(0.1f, 5.0f)]
    public float colorChangeSpeed = 1.0f;
    
    [Tooltip("Saturation of the colors (0 = grayscale, 1 = full color)")]
    [Range(0.0f, 1.0f)]
    public float colorSaturation = 0.8f;
    
    [Tooltip("Brightness of the colors")]
    [Range(0.2f, 1.0f)]
    public float colorBrightness = 0.8f;

    [Header("Breathing Effect")]
    [Tooltip("Speed of the breathing effect (higher = faster)")]
    [Range(0.1f, 5.0f)]
    public float breathingSpeed = 1.5f;
    
    [Tooltip("Minimum alpha value (transparency) during breathing")]
    [Range(0.0f, 1.0f)]
    public float minAlpha = 0.6f;
    
    [Tooltip("Maximum alpha value (transparency) during breathing")]
    [Range(0.0f, 1.0f)]
    public float maxAlpha = 1.0f;

    // Private variables
    private Image targetImage;
    private float hueValue = 0f;
    private float alphaValue = 1f;

    // Store original settings to revert when disabled
    private Color originalColor;

    private void Awake()
    {
        // Get the Image component
        targetImage = GetComponent<Image>();
        if (targetImage == null)
        {
            Debug.LogError("ColorfulBreathingEffect requires an Image component!");
            enabled = false;
            return;
        }

        // Store original color
        originalColor = targetImage.color;
    }

    private void Update()
    {
        if (!isActive || targetImage == null) return;

        // Update hue (color cycling)
        hueValue = (hueValue + Time.deltaTime * colorChangeSpeed * 0.2f) % 1.0f;
        
        // Calculate breathing alpha
        float breathingPhase = Mathf.Sin(Time.time * breathingSpeed) * 0.5f + 0.5f;
        alphaValue = Mathf.Lerp(minAlpha, maxAlpha, breathingPhase);
        
        // Apply color with HSV (hue, saturation, value)
        Color newColor = Color.HSVToRGB(hueValue, colorSaturation, colorBrightness);
        newColor.a = alphaValue;
        
        targetImage.color = newColor;
    }

    /// <summary>
    /// Enables or disables the breathing effect
    /// </summary>
    public void SetActive(bool active)
    {
        isActive = active;
        if (!active)
        {
            // Restore original color when disabled
            targetImage.color = originalColor;
        }
    }

    /// <summary>
    /// Sets a static color (disables breathing but keeps the given color)
    /// </summary>
    public void SetStaticColor(Color color)
    {
        isActive = false;
        targetImage.color = color;
    }

    private void OnDestroy()
    {
        // Clean up resources if needed
    }
}
