using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ChatTestUI : MonoBehaviour
{
    public TMP_InputField inputField;
    public Button sendButton;
    public GameObject chatMessagePrefab; // ���ǽ�ʵ��������ϢԤ����
    public Transform chatContentPanel; // ScrollView��Content����

    [Header("Chat Bubble Colors")]
    public Color userBubbleColor = new Color(0.0f, 0.5f, 1.0f); // ��ɫ
    public Color tutorBubbleColor = new Color(0.8f, 0.8f, 0.8f); // ��ɫ

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
            // ���������Ͱ�ť����ֹ�ڵȴ�AI��Ӧʱ�ظ�����
            inputField.interactable = false;
            sendButton.interactable = false;

            await ChatManager.Instance.SendMessage(message);

            inputField.text = "";
            // ���¼��������Ͱ�ť
            inputField.interactable = true;
            sendButton.interactable = true;
            inputField.ActivateInputField(); // ���¾۽��������
        }
    }

    private void UpdateChatHistory(string message, ChatManager.Sender sender)
    {
        // ���� �����޸���ʹ�� SetParent(parent, false) ��ȷ����ȷ��UI���źͶ�λ ����
        // 1. ��������ռ��д���Ԥ���壬��ָ��������
        GameObject messageRow = Instantiate(chatMessagePrefab);

        // 2. ʹ�� SetParent ������������ false
        //    ��� false ���� (worldPositionStays) �ǽ������Ĺؼ�
        //    ������Unity������Ҫ��ͼ�����������ԭ��������λ�ã�
        //    ����������λ�á���ת��������ȫ�����µĸ����������¼��㡣��
        messageRow.transform.SetParent(chatContentPanel, false);

        // ʹ��ChatMessageUI�ű���������Ϣ�Ͷ���
        ChatMessageUI messageUI = messageRow.GetComponent<ChatMessageUI>();
        if (messageUI != null)
        {
            // ������Ϣ���ݺͶ��뷽ʽ��true��ʾ�û���Ϣ���Ҷ��룩
            messageUI.SetMessage(message, sender == ChatManager.Sender.User);
        }
        
        // ����������ɫ
        Image bubbleImage = messageRow.GetComponentInChildren<Image>();
        if (bubbleImage != null)
        {
            bubbleImage.color = (sender == ChatManager.Sender.User) ? userBubbleColor : tutorBubbleColor;
        }
    }
}
