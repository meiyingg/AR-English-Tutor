# AgentButton�ܹ��ع�����ǿ�Ƴ�����ȡ�������ı�����

## ? �������

**ԭ���Ĵ������**��
- AgentButton = �������� + ǿ�Ƴ�����ȡ
- ����ͨ��AgentButton�����붼�᳢�Ի�ȡ����������
- ����Sceneģʽ���޷����д��ı��Ի�

**��ȷ���������**��
- AgentButton = ����ת�ı�����
- ������ȡӦ�û����û���ͼ��ģʽ���󣬶��������뷽ʽ

## ? �ع�����

### 1. **�Ƴ�ǿ�Ƴ�����**
```csharp
// ? ����ľ��߼�
if (agentButton?.gameObject.activeInHierarchy == true && 
    captureSceneOnAgentRecord && 
    BackgroundSceneMonitor.Instance != null)
{
    // ǿ�ƻ�ȡ�����������û���ͼ
}

// ? ��ȷ�����߼�  
// AgentButtonֻ��������ת�ı���������ȡ�������ݷ���
```

### 2. **�������ݵ������ж�**
����ϵͳ�����**�û�˵��������**������**���뷽ʽ**�������Ƿ���Ҫ����������

#### Normalģʽ��
- **��ȷ����ѯ�ʲŻ�ȡ����**��
  - "What do you see?"
  - "Describe this picture"
  - "Look at this image"
- **�����������붼�Ǵ��ı��Ի�**

#### Sceneģʽ��
- **�����ж��Ƿ���Ҫ��������**��
  - �״ν��� �� ��������
  - ��ȷ�������� �� ��������  
  - �����Ի��ź� �� ���ı��Ի�
  - 8�ֺ���ģ������ �� ��������

#### Wordģʽ��
- **�����ж��Ƿ���Ҫ�ʻ����**��
  - �״ν��� �� �ʻ����
  - ��ȷ�ʻ����� �� �´ʻ����
  - ��ϰ�ź� �� ���ı���ϰ�Ի�
  - 6�ֺ���ģ������ �� �´ʻ����

## ? �޸�����û�����

### Sceneģʽ�µ�AgentButtonʹ�ã�
```
�û�: [���AgentButton] "I see a kitchen" (�״�)
AI: [��������] "Yes! This is a kitchen. I can see a coffee maker..."

�û�: [���AgentButton] "I love coffee" (�����Ի��ź�)
AI: [���ı��Ի�] "That's wonderful! What's your favorite type of coffee?"

�û�: [���AgentButton] "I prefer espresso" (�����Ի��ź�)  
AI: [���ı��Ի�] "Espresso is strong and delicious! Do you make it at home?"

�û�: [���AgentButton] "What else do you see?" (��ȷ��������)
AI: [���³�������] "Looking at the kitchen again, I also notice..."
```

### Normalģʽ�µ�AgentButtonʹ�ã�
```
�û�: [���AgentButton] "How are you today?"
AI: [���ı��Ի� + �﷨����] "I'm doing well, thank you! How about you?"
AI: [Grammar correction] "Your grammar is perfect! 'How are you today?' is a great way to start a conversation."

�û�: [���AgentButton] "What do you see in this room?"
AI: [��������] "I can see you're in a living room with a sofa..."
```

## ? �ܹ�����

### 1. **���뷽ʽ����**
- �ı��������������������ȫ�ȼ�
- AgentButton�����������"������ȡ"Ȩ��
- �û���������ѡ�����뷽ʽ

### 2. **������ͼ����Ӧ**
- ϵͳ�����û�**˵��ʲô**������**��ô˵��**
- ����Ȼ�Ľ�������
- ��׼ȷ����ͼ���

### 3. **ģʽһ����**
- ��������ģʽ����һ�µ��ж��߼�
- �������ı�������������Ϊ����ͬ
- �����û�����

### 4. **�����ĳ����Ի�**
- Sceneģʽ�¿��Խ��г�ʱ�����ȶԻ�
- Wordģʽ�¿��������ϰ�ʻ�
- Normalģʽ�¿�����������

## ? ���ܾ���ϵͳ

### ������������������
| ģʽ | �������� |
|------|----------|
| **Normal** | ��ȷ����ѯ�ʹؼ��� |
| **Scene** | �״� + ��ȷ���� + ���Ի����� |
| **Word** | �״� + ��ȷ�ʻ����� + ��ϰ��� |

### �����Ի����źţ�
- ���˻�Ӧ��`"I like..."`, `"My favorite..."`
- �򵥻ش�`"Yes"`, `"No"`, `"OK"`
- �����Ի�Ӧ������`"because"`, `"so"`
- ���ʣ�`"Do you..."`, `"Can you..."`

## ? ������֤

### ��֤AgentButton����ǿ�ƻ�ȡ������
1. **Sceneģʽ**��
   - ˵"I like this" �� Ӧ�ü����Ի��������·�������
   - ˵"Tell me more" �� Ӧ�ü����Ի�
   - ˵"What do you see?" �� Ӧ�����·�������

2. **Normalģʽ**��
   - ˵"Hello" �� Ӧ�ô��ı��Ի� + �﷨����
   - ˵"Describe this picture" �� Ӧ�ý��г�������

3. **Wordģʽ**��
   - ˵"I use this word" �� Ӧ�ü����ʻ���ϰ
   - ˵"Teach me new words" �� Ӧ�ý����´ʻ�

## ? �ܽ�

����ع�����ˣ�
1. ? **AgentButtonǿ�ƻ�ȡ����������**
2. ? **Sceneģʽ�޷������Ի�������**  
3. ? **���뷽ʽ�빦�ܰ󶨵ļܹ�����**
4. ? **�û���ͼʶ��׼ȷ������**

����AgentButton������Ϊ��һ��**������������빤��**��������������Ϊ��һ��**�������������жϵĹ���**����������Ƹ������û�ֱ����Ҳ�����ں���������չ��
