# �ƶ��豸�������ܼ����Ա���

## ? ����ӵ��ֻ�֧�ֹ���

### 1. **��˷�Ȩ�޴���**
```csharp
#if UNITY_ANDROID && !UNITY_EDITOR
// �Զ�������˷�Ȩ��
if (!UnityEngine.Android.Permission.HasUserAuthorizedPermission(UnityEngine.Android.Permission.Microphone))
{
    UnityEngine.Android.Permission.RequestUserPermission(UnityEngine.Android.Permission.Microphone);
}
#endif
```

### 2. **�豸���ʹ�����**
- ? �����˷��豸������
- ? ����¼��ǰ��֤Ȩ��
- ? ¼��ʧ��ʱ�ṩ�û��ѺõĴ�����Ϣ
- ? �Զ����Ի��ƣ����¼���豸��

### 3. **�������Ӽ��**
- ? STTǰ�������״̬
- ? TTSǰ�������״̬
- ? ������ʱ��ʾ�Ѻ���ʾ

### 4. **��Ƶ�����Ż�**
- ? WAV��ʽת��������OpenAI API��
- ? ���󲶻�ʹ���
- ? �ڴ������ֹй©��

## ? �ֻ�����Ҫ��

### AndroidȨ�ޣ��Զ�����
- `android.permission.RECORD_AUDIO` - ¼��Ȩ��
- `android.permission.INTERNET` - ����Ȩ��
- `android.permission.ACCESS_NETWORK_STATE` - ����״̬���

### ����Ҫ��
- ��Ҫ�ȶ����������ӣ�WiFi���ƶ����ݣ�
- STT��TTS����Ҫ����OpenAI API

### Ӳ��Ҫ��
- ���õ���˷��豸
- ��Ƶ�����豸��������/������

## ? ����Ľ�����

### AudioManager.cs �Ľ���
1. **Ȩ�޼��**: `CheckMicrophonePermission()`
2. **������**: `Application.internetReachability`
3. **������**: try-catch��װ
4. **�û�����**: ����ʱ����`OnSpeechToTextResult`

### �ֻ��ض�����
```csharp
// Android�豸���⴦��
#if UNITY_ANDROID && !UNITY_EDITOR
    // Ȩ�޺��豸���
#endif

// ����״̬���
if (Application.internetReachability == NetworkReachability.NotReachable)
{
    // �ṩ������ʾ
}
```

## ? APK�������

### 1. **Player Settings ����**
- Target API Level: ����API 28+
- Scripting Backend: IL2CPP
- Api Compatibility Level: .NET Standard 2.1

### 2. **Ȩ������**
Unity���Զ���ӱ�ҪȨ�ޣ���������`AndroidManifest.xml`��ȷ�ϣ�
```xml
<uses-permission android:name="android.permission.RECORD_AUDIO" />
<uses-permission android:name="android.permission.INTERNET" />
<uses-permission android:name="android.permission.ACCESS_NETWORK_STATE" />
```

### 3. **���Խ���**
- �ڲ�ͬAndroid�汾�豸�ϲ���
- ����WiFi���ƶ����绷��
- ����Ȩ�޾ܾ���������Ȩ���
- ���������жϺͻָ�

## ?? Ǳ������ͽ������

### 1. **�״�����Ȩ��**
**����**: �û����ܾܾ���˷�Ȩ��
**���**: �����Ѱ���Ȩ�޼����Ѻ���ʾ

### 2. **�����ӳ�**
**����**: �ֻ�������ܱ�WiFi��
**���**: ���������״̬�����û���ʾ

### 3. **��Ƶ����**
**����**: �ֻ���˷������β��
**���**: ʹ�ñ�׼������44.1kHz��OpenAI API������Ҫ�󲻸�

### 4. **API��Կ��ȫ**
**����**: APK�е�API��Կ���ܱ����򹤳�
**����**: ����ʹ�÷������������Կ����

## ? �����嵥

�ڴ��APK�󣬽�����ԣ�

- [ ] �״�����ʱȨ������
- [ ] ¼����ť��Ӧ��? �� STOP��
- [ ] ����ת����׼ȷ��
- [ ] AI�ظ��Զ�����TTS
- [ ] �����ж�ʱ�Ĵ�����
- [ ] Ȩ�ޱ��ܾ�ʱ����ʾ
- [ ] ��ͬ���������µ�¼��Ч��
- [ ] ��ʱ��ʹ�õ��ȶ���

## ����

? **�����Ѿ߱��ֻ���������**
- ���б�Ҫ��Ȩ�޼�������
- ����״̬�����ʵ��
- �������������
- �û������Ѻõ���ʾ��Ϣ

���԰�ȫ�ش��APK���в��ԣ�
