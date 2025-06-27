using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ChatMessageUI : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI messageText;

    [SerializeField]
    private HorizontalLayoutGroup layoutGroup;

    // ����������������ֵ������ʵ�����Ҷ���
    [SerializeField]
    private int leftPadding = 20;  // ���Ĭ�����
    
    [SerializeField]
    private int rightPadding = 150;  // �Ҳ�������䣬�����Ҷ���Ч��
    
    /// <summary>
    /// ������Ϣ���ݺͶ��뷽ʽ
    /// </summary>
    /// <param name="text">��Ϣ�ı�</param>
    /// <param name="isUserMessage">�Ƿ����û����͵���Ϣ (true = �Ҷ���, false = �����)</param>
    public void SetMessage(string text, bool isUserMessage)
    {
        if (messageText != null)
        {
            messageText.text = text;
            
            // �����ı��Ķ��뷽ʽ
            messageText.alignment = isUserMessage ? TextAlignmentOptions.Right : TextAlignmentOptions.Left;
        }

        if (layoutGroup != null)
        {
            // ������û���Ϣ���Ҷ��룩������������нϴ�Ŀռ䣬�Ҳ��С
            // �����AI��Ϣ������룩������������С���Ҳ��нϴ�Ŀռ�
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
