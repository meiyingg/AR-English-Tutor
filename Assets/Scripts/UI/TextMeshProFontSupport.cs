using UnityEngine;
using TMPro;

/// <summary>
/// TextMeshPro����֧�ֹ����� - ����Text����������ĺ�emoji��ʾ����
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
        // ��������TextMeshPro���������
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
        
        // ���û�������
        textComponent.richText = enableRichText;
        textComponent.parseCtrlCharacters = parseCtrlCharacters;
        
        // �����Զ�������С
        if (enableAutoSizing)
        {
            textComponent.enableAutoSizing = true;
            textComponent.fontSizeMin = 8f;
            textComponent.fontSizeMax = 72f;
        }
        
        // ȷ���ı�����
        textComponent.enableWordWrapping = true;
        
        // ���ô�ֱ����
        textComponent.verticalAlignment = VerticalAlignmentOptions.Top;
        
        Debug.Log($"Configured TextMeshPro component: {textComponent.name}");
    }
    
    /// <summary>
    /// �����ı����ݣ��Ƴ����ܵ�����ʾ������ַ�
    /// </summary>
    public static string CleanTextForDisplay(string text)
    {
        if (string.IsNullOrEmpty(text))
            return text;
        
        // �Ƴ����ܵ�����ʾ����������ַ�
        // ���������ĸ��ı����
        string cleanedText = text;
        
        // �Ƴ�һЩ�����������Unicode�ַ�
        cleanedText = cleanedText.Replace("\u200B", ""); // ���ո�
        cleanedText = cleanedText.Replace("\u200C", ""); // ������ַ�
        cleanedText = cleanedText.Replace("\u200D", ""); // ������ַ�
        cleanedText = cleanedText.Replace("\uFEFF", ""); // �ֽ�˳����
        
        return cleanedText;
    }
}
