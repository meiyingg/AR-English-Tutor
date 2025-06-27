# Image Upload Feature Implementation Guide

## ? �¹��ܸ���
AR English Tutor����֧��ͼƬ�ϴ���AI�Ӿ�ʶ���ܣ��û������ϴ�ͼƬ��AI����Ӣ������ͼƬ���ݣ�����Ӣ��ѧϰ��

## ? ʵʩ״̬

### ? ����ɵ������

1. **ImageUploadManager.cs** - ͼƬ�ϴ�������
   - ֧��ͼƬѡ�񣨱༭��ģʽ��ʹ���ļ��Ի���
   - ͼƬѹ����Base64����
   - �ļ���С���ƣ�2MB��

2. **��չ��OpenAIManager.cs** - ֧��Vision API
   - ����`PostVisionRequest`����
   - ֧��GPT-4 Vision Previewģ��
   - ����ͼƬ+�ı��Ļ������

3. **ImageMessageUI.cs** - ͼƬ��ϢUI���
   - ��ʾͼƬ���ı���Ϣ
   - ֧���û�/AI��Ϣ����
   - ���ͼƬ����չ��Ԥ�����ܣ�

4. **��չ��ChatManager.cs** - ͼƬ��Ϣ����
   - ����`SendImageMessage`����
   - ͼƬ��Ϣ�¼�����
   - AIͼƬ������������

5. **���µ�ChatTestUI.cs** - UI����
   - ͼƬ�ϴ���ť֧��
   - ͼƬ��Ϣ��ʾ
   - UI״̬����

### ? UI�Ľ���
- ����������������ɫ��ͼƬ�ϴ���ť ?
- ��ťλ�ã������ͷ��Ͱ�ť֮��
- �ִ�������ɫ����

## ? ��һ������

### ��Ҫ�ֶ����õ���Ŀ��

1. **ΪImageUploadManager��ӽű����**
   ```
   ѡ�񳡾��е�ImageUploadManager GameObject
   �� Add Component �� ImageUploadManager
   ```

2. **����ChatTestUI��ͼƬ��ť����**
   ```
   ѡ��ChatTestUI GameObject
   �� ��Inspector���ҵ�imageButton�ֶ�
   �� ��קImageButton�����ֶ�
   ```

3. **����ͼƬ��ϢԤ����**����ѡ��
   ```
   �������е�chatMessagePrefab
   �� ���Image���������ʾͼƬ
   �� ����ΪimageMessagePrefab
   �� ��ChatTestUI�����ø�Ԥ����
   ```

## ? ����ʹ������

1. **�û�����**��
   - �����ɫ��?��ť
   - ѡ��ͼƬ���༭��ģʽ�»���ļ�ѡ��Ի���
   - ͼƬ�Զ����͸�AI����

2. **AI����**��
   - ��ʾ"Analyzing image..."��ʾ
   - ����GPT-4 Vision API
   - ����Ӣ������

3. **�����ʾ**��
   - ���������ʾ�û��ϴ���ͼƬ
   - AI��Ӣ��������ʾ���·�

## ?? ����ϸ��

### API����
- ʹ��OpenAI GPT-4 Vision Previewģ��
- Ĭ����ʾ�ʣ�"Describe this image in English for language learning. Be detailed and educational."
- �����Ӧ���ȣ�300 tokens

### ͼƬ����
- ֧�ָ�ʽ��PNG, JPG, JPEG
- ���ߴ磺2048x2048����
- ����ļ���С��2MB
- �Զ�ѹ���������Ƶ�ͼƬ

### �ƶ���֧��
- ��ǰ�汾�ڱ༭����ʹ���ļ��Ի���
- �ƶ�����Ҫ���NativeGallery���
- ��Ԥ���ƶ������/����ӿ�

## ? �����ų�

### �������⣺

1. **"ImageUploadManager not found"����**
   - ȷ����������ImageUploadManager GameObject
   - ȷ����GameObject��ImageUploadManager�ű����

2. **ͼƬ��ťû�з�Ӧ**
   - ���ChatTestUI��imageButton�����Ƿ���ȷ
   - ȷ����ť��onClick�¼�������

3. **API����ʧ��**
   - ���APIConfig�е�API��Կ�Ƿ���ȷ
   - ȷ��������������
   - �鿴Unity Console�Ĵ�����־

## ? ������չ����

1. **ͼƬԤ������**
   - ѡ��ͼƬ����ʾԤ��
   - �����û���Ӷ����ı�����

2. **������չ���**
   - ����NativeCamera���
   - ֧��ʵʱ�����ϴ�

3. **ͼƬ��ʷ��¼**
   - �����ϴ���ͼƬ
   - ֧�����²鿴�������

4. **������֧��**
   - ֧����Ӣ���л�
   - AI��������ѡ��

## ? ��ϲ��
ͼƬ�ϴ���AI�Ӿ�ʶ�����ѻ���ʵ�֣��û����ڿ���ͨ���ϴ�ͼƬ��ѧϰӢ��ʻ�ͱ�﷽ʽ��
