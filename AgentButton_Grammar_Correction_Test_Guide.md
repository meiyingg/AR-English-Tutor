# AgentButton��Grammar Correction��������ָ��

## �����޸�˵��

### ԭ����
��Normalģʽ�£�ʹ��AgentButton�������������AIû���ṩgrammar correction��ʾ����

### ����ԭ��
1. **·����֧����**: `OnTranscriptionReceived`�����У�AgentButton���������������ѡ��`SendSceneRecognitionRequest`������`SendMessage`
2. **ȱʧ�߼�**: `SendSceneRecognitionRequest`����û�е���grammar correction����

### �޸�����
1. **�޸�ChatManager.cs**: ��`SendSceneRecognitionRequest`�����У�������Normalģʽʱ��AI�ظ����Զ�����`ProvideGrammarCorrection`
2. **�Ż�OnTranscriptionReceived�߼�**: ���ݵ�ǰѧϰģʽ�����ж��Ƿ�ʹ�ó���������

## ���Բ���

### ����1: Normalģʽ��AgentButton��������
1. **�л���Normalģʽ**:
   ```
   ���"Normal Mode"��ť������"/normal"
   ```

2. **ʹ��AgentButton������������**:
   - ���AgentButton���������밴ť��
   - ˵һ�ΰ����﷨�����Ӣ���: "I are going to school yesterday"
   - �۲�AI�ظ�

3. **�������**:
   - AI�����ṩ�Ի��ظ�
   - Ȼ���Զ��ṩ�﷨�����ʾ��
   - ��ʽ���ƣ�
     ```
     ? Grammar Analysis:
     Original: "I are going to school yesterday"
     Correction: "I went to school yesterday"
     
     ? Grammar Tips:
     - Use "went" (past tense) instead of "are going" when talking about yesterday
     - "Yesterday" indicates past time, so use past tense verbs
     
     ? Practice Examples:
     - "I went to the store yesterday"
     - "She visited her friend last week"
     ```

### ����2: Sceneģʽ��AgentButton��Ϊ
1. **�л���Sceneģʽ**:
   ```
   ���"Scene Mode"��ť������"/scene"
   ```

2. **ʹ��AgentButton**:
   - ȷ���пɼ���AR��������
   - ���AgentButton������������
   - ˵: "What do you see here?"

3. **�������**:
   - AI������ǰAR����
   - �ṩ������ص�Ӣ��ѧϰ����
   - **��**�ṩgrammar correction����Ϊ��Sceneģʽ��

### ����3: Normalģʽ�³�����ز�ѯ
1. **�л���Normalģʽ**
2. **ʹ��AgentButton˵�����������**:
   - "What do you see in this picture?"
   - "Describe what's here"
   - "Tell me about this scene"

3. **�������**:
   - AI�������������Ϊ��ѯ�����볡����أ�
   - ͬʱ�ṩgrammar correction����Ϊ��Normalģʽ��

### ����4: Normalģʽ�·ǳ�����ѯ
1. **�л���Normalģʽ**
2. **ʹ��AgentButton˵һ��������**:
   - "How are you today?"
   - "I like pizza very much"
   - "The weather is nice"

3. **�������**:
   - AI����һ��Ի���������������
   - �ṩgrammar correction

## �����߼�˵��

### ���ܳ����������ж�
��Normal��Wordģʽ�£�ϵͳ�������ж��û������Ƿ��볡����أ�
- **������عؼ���**: "what", "see", "this", "here", "picture", "image"
- **Sceneģʽ**: ���ǳ��Է�������
- **Normal/Wordģʽ**: ֻ���ڼ�⵽������عؼ���ʱ�ŷ�������

### Grammar Correction��������
- **SendMessage����**: Normalģʽ���Զ�����
- **SendSceneRecognitionRequest����**: Normalģʽ�������û�����ʱ�Զ�����

## ������Ϣ

### �����־
1. **ģʽ�л���־**:
   ```
   [LearningModeManager] Mode changed to: Normal
   ```

2. **�������봦����־**:
   ```
   Transcription received: [�û���������]
   [ChatManager] Current mode: Normal, providing grammar correction
   ```

3. **Grammar Correction��־**:
   ```
   [ChatManager] Grammar correction requested for: [�û�����]
   [ChatManager] Grammar correction completed
   ```

## �����ų�

### ���Grammar Correction��Ȼ��������
1. **���LearningModeManager**: ȷ�ϵ�ǰģʽȷʵ��Normal
2. **���ChatManager��־**: ȷ��`ProvideGrammarCorrection`����������
3. **���OpenAI��Ӧ**: ȷ��AI��ȷ�����˸�ʽ�����﷨��������

### �������������������
1. **���BackgroundSceneMonitor**: ȷ�ϳ���ͼ����ȷ����
2. **���captureSceneOnAgentRecord**: ȷ�ϴ�ѡ��������
3. **���AgentButton״̬**: ȷ�ϰ�ť���ڻ�Ծ״̬

## �ܽ�

�޸���AgentButton��Normalģʽ�µ���ΪӦ���ǣ�
1. **����·��ѡ��**: �����û��������ݾ����Ƿ��������
2. **˫�ع���**: ���ṩ�Ի��ظ������ṩ�﷨����
3. **ģʽ��֪**: �ڲ�ͬģʽ�±��ֳ���ͬ����Ϊ�ص�

����ȷ�����û����κ�ģʽ��ʹ��AgentButton���ܻ��һ����������ѧϰ���顣
