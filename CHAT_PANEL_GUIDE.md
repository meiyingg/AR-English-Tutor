# Chat Panel ����/��ʾ����˵��

## ���ܸ���
ΪChatTestUI�����Chat�������غ���ʾ���ܣ�֧�ּ򵥵��л�������

## �����ֶ�

��Unity Inspector����Ҫ���õ��ֶΣ�

### Chat Panel Control
- **Chat Panel**: ��קChat���ĸ�GameObject��ͨ����Canvas�µ�ChatPanel��
- **Toggle Chat Button**: ��ק�����л���ʾ/���صİ�ť

## ʹ�÷���

### 1. ����Toggle Button�����򵥣�

1. **��Unity�д�����ť**��
   - �Ҽ����Hierarchy�е�`Canvas`
   - ѡ�� `UI` �� `Button - TextMeshPro`
   - ������Ϊ `ChatToggleButton`

2. **���ð�ť**��
   - ��ק��ť������λ�ã�������Ļ���䣩
   - ������ť��С������50x50���أ�
   - �޸İ�ť����Ϊ "?" �� "����"

### 2. ���ӵ�ChatTestUI

1. ѡ�����`ChatTestUI`�����GameObject
2. ��Inspector��"Chat Panel Control"���֣�
   - **Chat Panel**: ��ק������������GameObject��ͨ����ChatPanel��
   - **Toggle Chat Button**: ��ק�մ�����ChatToggleButton

### 3. ��ɣ�
������Ϸ�������ť��������/��ʾChat����ˡ�

### 2. ����ʱ����

#### ͨ����ť�л�
�û����Toggle Chat Button�����л�Chat������ʾ/����״̬��

#### ͨ���������
```csharp
// ��ȡChatTestUI���
ChatTestUI chatUI = FindObjectOfType<ChatTestUI>();

// ��ʾChat���
chatUI.ShowChatPanel();

// ����Chat���
chatUI.HideChatPanel();

// �л�Chat���״̬
chatUI.ToggleChatPanel();

// ���Chat����Ƿ�ɼ�
bool isVisible = chatUI.IsChatPanelVisible();
```

## ����

- ? �򵥵���ʾ/�����л�
- ? ״̬��¼��isChatPanelVisible��
- ? Debug��־���
- ? ����API���ⲿ����
- ? ��ʼ״̬���ã�Ĭ����ʾ��
- ? ��ȫ��飨null��飩

## ��չ����

�����Ҫ�����ӵĹ��ܣ����Կ��ǣ�

1. **����Ч��**: ʹ��DOTween��Unity Animation��ӵ��뵭��Ч��
2. **��ť�ı�����**: ����״̬��ʾ"Show Chat"/"Hide Chat"
3. **��ݼ�֧��**: ��Ӽ��̿�ݼ�����Escape����
4. **�Զ�����**: ���ض��������Զ����أ���ARɨ��ʱ��
5. **״̬����**: �����û���ƫ������

## ע������

- ȷ����Inspector����ȷ���������ֶ�
- Chat PanelӦ���ǰ��������������ĸ�GameObject
- Toggle Button���Է������κ�λ�ã�������UI�����۴�
