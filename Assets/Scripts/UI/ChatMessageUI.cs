using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ChatMessageUI : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI messageText;

    [SerializeField]
    private HorizontalLayoutGroup layoutGroup;

    // 设置左右两侧的填充值，用于实现左右对齐
    [SerializeField]
    private int leftPadding = 20;  // 左侧默认填充
    
    [SerializeField]
    private int rightPadding = 150;  // 右侧更大的填充，用于右对齐效果
    
    /// <summary>
    /// 设置消息内容和对齐方式
    /// </summary>
    /// <param name="text">消息文本</param>
    /// <param name="isUserMessage">是否是用户发送的消息 (true = 右对齐, false = 左对齐)</param>
    public void SetMessage(string text, bool isUserMessage)
    {
        if (messageText != null)
        {
            messageText.text = text;
            
            // 设置文本的对齐方式
            messageText.alignment = isUserMessage ? TextAlignmentOptions.Right : TextAlignmentOptions.Left;
        }

        if (layoutGroup != null)
        {
            // 如果是用户消息（右对齐），则设置左侧有较大的空间，右侧较小
            // 如果是AI消息（左对齐），则设置左侧较小，右侧有较大的空间
            if (isUserMessage)
            {
                layoutGroup.padding.left = rightPadding;
                layoutGroup.padding.right = leftPadding;
            }
            else
            {
                layoutGroup.padding.left = leftPadding;
                layoutGroup.padding.right = rightPadding;
            }
        }
    }
}
