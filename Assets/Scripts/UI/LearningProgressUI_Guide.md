# ? ARӢ��ѧϰApp - ѧϰ����UI���ָ��

## ? UI������� (���ɵ�MainPanel)

### 1. **MainPanel���� - ���ѧϰ��������**
```
Canvas
������ ChatPanel (�������)
������ MainPanel (������)
    ������ ProgressPanel (����ѧϰ��������)
    ��   ������ LevelInfo
    ��   ��   ������ LevelText: "Level 5"
    ��   ��   ������ TitleText: "Beginner"
    ��   ������ ExpInfo  
    ��       ������ ExpProgressBar (������)
    ��       ������ ExpText: "120/300 EXP"
    ������ Chat Panel Button (�Ѵ���)
    ������ Text (TMP) (�Ѵ���)
```

### 2. **MainPanel����Ч��**
```
������������������������������������������������������������������������������
��  ? Level 5 (Beginner)              ��
��  ������������������������?????? 120/300 EXP     ��  <- ѧϰ��������
������������������������������������������������������������������������������
��                                     ��
��          AR English Tutor           ��
��                                     ��
��      [? Open Chat Panel]           ��  <- ���а�ť
��                                     ��
��         Welcome Message             ��  <- �����ı�
��                                     ��
������������������������������������������������������������������������������
```

## ?? Unity UI ʵ�ֲ��� (MainPanel�汾)

### Step 1: ��MainPanel�����ProgressPanel
1. �� `MainPanel` �´��� `ProgressPanel` GameObject
2. ����Ϊ�������֣��߶�Լ80-100px

### Step 2: UI��νṹ (���°�)
```
Canvas
������ ChatPanel (���ֲ���)
������ MainPanel 
��   ������ ProgressPanel (�����ڶ���)
��   ��   ������ LevelInfo
��   ��   ��   ������ LevelText (TextMeshPro)
��   ��   ��   ������ TitleText (TextMeshPro)
��   ��   ������ ExpInfo
��   ��       ������ ExpProgressBar (Slider)
��   ��       ������ ExpText (TextMeshPro)
��   ������ Chat Panel Button (����)
��   ������ Text (TMP) (����)
������ LevelUpPanel (���������������㼶)
```

### Step 3: MainPanel�������� (�򻯰�)

#### ProgressPanel (��MainPanel����):
- **�ֶ���λ**: ֱ������AnchorΪTop Stretch
- **Position**: Top: 0, Height: 80px
- **Background**: ��͸����ɫ `rgba(0,0,0,0.3)`
- **Layout**: Horizontal Layout Group (����ProgressPanel�ڲ�)
- **Padding**: Left: 20, Right: 20, Top: 10, Bottom: 10

#### MainPanel���ּ�:
```
MainPanel (�����Layout Group)
������ ProgressPanel (�ֶ���λ������)
������ Chat Panel Button (����ԭλ��)
������ Text (TMP) (����ԭλ��)
```

#### ExpProgressBar��ȷ����:
- **Interactable**: ? ȡ����ѡ
- **Min Value**: 0, **Max Value**: 1
- **Handle**: ����Handle��Image��ɫΪ͸�� `rgba(0,0,0,0)`
- **Fill**: ���ý�������ɫ����ɫ�ȣ�
- **Background**: ��ɫ��͸�� `rgba(128,128,128,0.3)`

#### ��������:
```
LevelUpPanel (�򻯰�)
������ Background (Image) - ��͸������
������ LevelUpText (TextMeshPro) - ������Ϣ
������ CloseButton (Button) - �رհ�ť
```
**����Ҫ��levelIconͼƬ����**

## ? ���뼯�� (MainPanel�汾)

### �޸�����UI������:
��������Ľű���(�����UIManager.cs)���:

```csharp
[Header("Learning Progress UI")]
public LearningProgressUI progressUI;

private void Start()
{
    // ���д���...
    
    // ��ʼ��ѧϰ����UI
    if (progressUI != null)
    {
        progressUI.RefreshDisplay();
    }
}
```

### �������ò���:
1. **MainPanel����**: ���Vertical Layout Group���
2. **ProgressPanel����**: ��MainPanel��������
3. **LearningProgressUI�ű�**: ���ص�ProgressPanel
4. **UI��������**: ��Inspector����������UIԪ��

## ? MainPanelѧϰ������ʾЧ��

### ������״̬��ʾ:
- �û���App���ȿ���MainPanel
- ������ʾ��ǰѧϰ���Ⱥ͵ȼ�
- ������찴ť����ѧϰ�Ի�
- ѧϰ��ɺ󷵻�MainPanel�����Ȼ����

### �û���������:
1. **����App** �� MainPanel��ʾ��ǰ�ȼ�����
2. **�������** �� ����ChatPanel����ѧϰ�Ի�  
3. **���ѧϰ** �� ����MainPanel����������ֵ����
4. **��������** �� ������ʾ������Ϣ

��������Ƹ�����App������ܹ���ѧϰ������Ϊȫ��״̬��ʾ�������棡

## ? ���Թ���

### �������� (��Inspector��):
- **Add 10 EXP**: ���Ծ���ֵ����
- **Reset Progress**: ����ѧϰ����
- **Force Level Up**: ǿ����������

### ���Գ���:
1. ����Ӧ�� -> �鿴��ʼ�ȼ���ʾ
2. �ϴ�ͼƬ���г���ѧϰ -> �鿴����ֵ����
3. ��ζԻ� -> �鿴�����Ի�����
4. �ﵽ�������� -> �鿴��������

## ? �ƶ����Ż�

### ��Ӧʽ���:
- ʹ�� **Canvas Scaler** ȷ����ͬ��Ļ����
- **Safe Area** ����������
- **Touch-friendly** ��ť�ߴ�

### �����Ż�:
- ����UI�ػ�Ƶ��
- ʹ�ö���ع�������֪ͨ
- �첽���ض�����Դ

## ? ������չ����

### �ɾ�ϵͳ:
- ����ѧϰ�����ɾ�
- ����ѧϰ��̱�
- �Ի�������¼

### �罻����:
- �ȼ����а�
- ���ѱȽ�
- ѧϰС��

��������ƽ�Ϊ����ARӢ��ѧϰApp�ṩ��������Ϸ��ѧϰ���飡

## ? **�������ܽ�**

### Unity����Ҫ�������飺

1. **����ѧϰ���ȹ�������**
   - �����д�����GameObject �� ����`LearningProgressManager`
   - ����`LearningProgressManager.cs`�ű�

2. **��MainPanel�����ProgressPanel��**
   - MainPanel�´���Panel �� ������`ProgressPanel`
   - �ֶ�����Ϊ�������߶�80px
   - **����MainPanel���Layout Group**

3. **ProgressPanel�ڲ��ṹ��**
   ```
   ProgressPanel (Horizontal Layout Group)
   ������ LevelInfo (����TextMeshPro)
   ������ ExpInfo (Slider + TextMeshPro)
   ```

4. **���ӽű����ã�**
   - ProgressPanel����`LearningProgressUI.cs`
   - Inspector������5��UI���ü��ɣ�
     - `levelText`, `titleText`, `expText`, `expProgressBar`
     - `levelUpPanel`, `levelUpText`, `levelUpCloseButton`
   - **����Ҫ����levelIcon**

5. **������壺**
   - Canvas�´����򵥵���
   - ֻ��Ҫ����+�ı�+�رհ�ť

### ���ԣ�
���к����̨Ӧ��ʾ��
```
? Welcome back, English Learner!
? Level: 1 (Beginner)  
? EXP: 0 | Next Level: 50 EXP needed
```

**����������ˣ���ʵ�ã�û�ж���ĸ������á�**

## ? **�����������**

### ����ֵ��÷�ʽ��
1. **��ͨ�Ի�**��ÿ����AI�Ի� +5 EXP
2. **�����Ի�����**������3�ֶԻ����� +5 EXP  
3. **����ѧϰ**���ϴ�ͼƬѧϰ +10 EXP
4. **ÿ���״�ѧϰ**��ÿ���һ��ѧϰ +5 EXP
5. **ѧϰ�µ���**��+3 EXP (δ������)

### ����Ҫ��
- **Level 1��2**: ��Ҫ 50 EXP
- **Level 2��3**: ��Ҫ 100 EXP  
- **Level 3��4**: ��Ҫ 150 EXP
- **Level N��N+1**: ��Ҫ N��50 EXP

### ����ʾ����
```
Level 1: 0-49 EXP (��Ҫ50 EXP����)
Level 2: 50-149 EXP (��Ҫ100 EXP����) 
Level 3: 150-299 EXP (��Ҫ150 EXP����)
Level 4: 300-499 EXP (��Ҫ200 EXP����)
```

### �����������ԣ�
- ��tutor�Ի�10�� = 50 EXP = ����Level 2
- �ϴ�5��ͼƬ����ѧϰ = 50 EXP = ����Level 2
- ������Inspector��ʹ��"Add 10 EXP"��ť5��
