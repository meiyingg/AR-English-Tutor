# AR English Tutor

һ������Unity��ARӢ��ѧϰӦ�ã�������OpenAI API�������ܶԻ�������ѧϰ��

## ��������

- AR��ǿ��ʵ����
- AI���ܶԻ�ϵͳ
- Ӣ��ѧϰ����
- ʵʱ��������

## ����˵��

### 1. ��¡��Ŀ
```bash
git clone https://github.com/meiyingg/AR-English-Tutor.git
cd AR-English-Tutor
```

### 2. Unity����
- ʹ��Unity 2022.3 LTS����߰汾����Ŀ
- ȷ���Ѱ�װAR Foundation��XR���

### 3. API��Կ����
**��Ҫ���벻Ҫ��API��Կֱ���ύ������ֿ⣡**

1. ��ȡ���OpenAI API��Կ��[OpenAI API Keys](https://platform.openai.com/api-keys)
2. ��Unity�༭���У�
   - �ҵ� `Assets/Scripts/AI/APIConfig.asset` �ļ�
   - ��Inspector����н� `YOUR_OPENAI_API_KEY_HERE` �滻Ϊ���ʵ��API��Կ
   - **ע�⣺����ļ�����ӵ�.gitignore�У����ᱻ�ύ**

### 4. �����������ã��Ƽ���
Ϊ�˸���ȫ�ع���API��Կ������ʹ�û���������

Windows:
```cmd
setx OPENAI_API_KEY "your_actual_api_key_here"
```

macOS/Linux:
```bash
export OPENAI_API_KEY="your_actual_api_key_here"
```

## ��Ŀ�ṹ

```
Assets/
������ Scripts/
��   ������ AI/          # AI��ؽű�
��   ������ AR/          # AR���ܽű�
��   ������ UI/          # �û�����ű�
��   ������ Utils/       # ������ű�
������ Prefabs/         # Ԥ����
������ Scenes/          # �����ļ�
������ Materials/       # ������Դ
```

## ��������

- Unity 2022.3 LTS+
- AR Foundation 5.0+
- XR Plugin Management
- OpenAI API

## ���֤

����Ŀ��ѭ MIT ���֤��

## ����

��ӭ�ύ����͹�������

## ��ȫ����

?? **��Ҫ��ȫ����**��
- ����Ҫ��API��Կ�ύ������ֿ�
- ʹ�û��������������ļ�����������Ϣ
- �����ֻ�API��Կ
- ���APIʹ�����
