using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ChatTestUI : MonoBehaviour
{
    public TMP_InputField inputField;
    public Button sendButton;
    public GameObject chatMessagePrefab; // 我们将实例化的消息预制体
    public Transform chatContentPanel; // ScrollView的Content对象

    [Header("Chat Bubble Colors")]
    public Color userBubbleColor = new Color(0.0f, 0.5f, 1.0f); // 蓝色
    public Color tutorBubbleColor = new Color(0.8f, 0.8f, 0.8f); // 灰色

    void Start()
    {
        sendButton.onClick.AddListener(OnSendButtonClick);
        ChatManager.Instance.onNewMessage.AddListener(UpdateChatHistory);
    }

    private async void OnSendButtonClick()
    {
        string message = inputField.text;
        if (!string.IsNullOrEmpty(message))
        {
            // 禁用输入框和按钮，防止在等待AI响应时重复发送
            inputField.interactable = false;
            sendButton.interactable = false;

            await ChatManager.Instance.SendMessage(message);

            inputField.text = "";
            // 重新激活输入框和按钮
            inputField.interactable = true;
            sendButton.interactable = true;
            inputField.ActivateInputField(); // 重新聚焦到输入框
        }
    }

    private void UpdateChatHistory(string message, ChatManager.Sender sender)
    {
        // ★★★ 最终修复：使用 SetParent(parent, false) 来确保正确的UI缩放和定位 ★★★
        // 1. 先在世界空间中创建预制体，不指定父对象
        GameObject messageRow = Instantiate(chatMessagePrefab);

        // 2. 使用 SetParent 方法，并传入 false
        //    这个 false 参数 (worldPositionStays) 是解决问题的关键
        //    它告诉Unity：“不要试图保持这个物体原来的世界位置，
        //    而是让它的位置、旋转、缩放完全根据新的父对象来重新计算。”
        messageRow.transform.SetParent(chatContentPanel, false);

        // 使用ChatMessageUI脚本来设置消息和对齐
        ChatMessageUI messageUI = messageRow.GetComponent<ChatMessageUI>();
        if (messageUI != null)
        {
            // 设置消息内容和对齐方式（true表示用户消息，右对齐）
            messageUI.SetMessage(message, sender == ChatManager.Sender.User);
        }
        
        // 设置气泡颜色
        Image bubbleImage = messageRow.GetComponentInChildren<Image>();
        if (bubbleImage != null)
        {
            bubbleImage.color = (sender == ChatManager.Sender.User) ? userBubbleColor : tutorBubbleColor;
        }
    }
}
