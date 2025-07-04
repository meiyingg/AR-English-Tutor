using UnityEngine;
using TMPro;

/// <summary>
/// TextMeshPro字体支持管理器 - 配置Text组件避免中文和emoji显示问题
/// </summary>
public class TextMeshProFontSupport : MonoBehaviour
{
    [Header("Font Settings")]
    public bool enableRichText = true;
    public bool parseCtrlCharacters = true;
    public bool enableAutoSizing = true;
    
    [Header("Auto Configure")]
    public bool autoConfigureAllTexts = true;
    
    private void Start()
    {
        if (autoConfigureAllTexts)
        {
            ConfigureAllTextComponents();
        }
    }
    
    public void ConfigureAllTextComponents()
    {
        // 查找所有TextMeshPro组件并配置
        TextMeshProUGUI[] allTexts = FindObjectsOfType<TextMeshProUGUI>();
        foreach (var text in allTexts)
        {
            ConfigureTextComponent(text);
        }
        
        Debug.Log($"Configured {allTexts.Length} TextMeshPro components for better text support");
    }
    
    public void ConfigureTextComponent(TextMeshProUGUI textComponent)
    {
        if (textComponent == null) return;
        
        // 配置基本设置
        textComponent.richText = enableRichText;
        textComponent.parseCtrlCharacters = parseCtrlCharacters;
        
        // 配置自动调整大小
        if (enableAutoSizing)
        {
            textComponent.enableAutoSizing = true;
            textComponent.fontSizeMin = 8f;
            textComponent.fontSizeMax = 72f;
        }
        
        // 确保文本换行
        textComponent.enableWordWrapping = true;
        
        // 设置垂直对齐
        textComponent.verticalAlignment = VerticalAlignmentOptions.Top;
        
        Debug.Log($"Configured TextMeshPro component: {textComponent.name}");
    }
    
    /// <summary>
    /// 清理文本内容，移除可能导致显示问题的字符
    /// </summary>
    public static string CleanTextForDisplay(string text)
    {
        if (string.IsNullOrEmpty(text))
            return text;
        
        // 移除可能导致显示问题的特殊字符
        // 保留基本的富文本标记
        string cleanedText = text;
        
        // 移除一些可能有问题的Unicode字符
        cleanedText = cleanedText.Replace("\u200B", ""); // 零宽空格
        cleanedText = cleanedText.Replace("\u200C", ""); // 零宽不连字符
        cleanedText = cleanedText.Replace("\u200D", ""); // 零宽连字符
        cleanedText = cleanedText.Replace("\uFEFF", ""); // 字节顺序标记
        
        return cleanedText;
    }
}
