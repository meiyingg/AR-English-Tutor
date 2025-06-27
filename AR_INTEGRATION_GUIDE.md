# Unity AR ���ܼ���ָ�� (Android)

���ĵ�ּ��Ϊ���ṩһ���������ֲ���ָ�ϣ���������� "AR English Tutor" ��Ŀ�м��ɺ��� AR ���ܡ�

## �׶�һ��ʵ�� AR ƽ���� (AR Plane Detection)

�������Ǽ��� AR �ĵ�һ����Ŀ������Ӧ���ܹ�ʶ�𲢸�����ʾ��ʵ�����е�ƽ�棨����桢���棩��

**ʵ�ֲ���:**

1.  **���� AR ����**:
    *   �½�һ������ (���� `ARScene`)��
    *   ɾ��������Ĭ�ϵ� `Main Camera`��

2.  **��� AR �������**:
    *   �ڳ�������� `AR Session` GameObject������������������ AR �Ự���������ڡ� (`GameObject -> XR -> AR Session`)
    *   ��� `XR Origin (Mobile AR)` GameObject���ڽ��°汾�� Unity �У���ȡ���˾ɵ� `AR Session Origin`������ AR ��������ĵ㣬���� AR ���ݶ��������������λ�� (`GameObject -> XR -> XR Origin (Mobile AR)`)
        *   **��ݷ�ʽ**: ��Ҳ����ֱ���� `Hierarchy` �����Ҽ����Ĭ�ϵ� `Main Camera`��Ȼ��ѡ�� `XR -> Convert Main Camera To XR Rig`�����һ����λ������滻��
        *   �� `XR Origin` �£����Զ�����һ�� `AR Camera`������ȡ������ɾ���� `Main Camera`��������Ⱦ AR ��ͼ��

3.  **����ƽ����**:
    *   �� `XR Origin` �ϣ���� `AR Plane Manager` �����
    *   ��������������ʵ�����е�ƽ�档����ҪΪ��ָ��һ�� **Plane Prefab**������⵽�µ�ƽ��ʱ��Unity ��ʹ����� Prefab ��ʵ����һ�����󣬴Ӷ����Ӿ��ϰ�ƽ���ʾ������

4.  **����ƽ����ӻ� Prefab**:
    *   ����һ���µ� 3D ���� (����һ���򵥵� `Plane`) ��Ϊ���ӻ��Ļ�����
    *   ������� `AR Plane Mesh Visualizer` ��������������Զ����ݼ�⵽��ƽ����״���� Mesh��
    *   Ϊ�����ۣ����Ը���һ����͸���Ĳ��ʣ����� `Line Renderer` ������ƽ��ı߽硣
    *   ��������úõĶ�����ק�� `Assets/Prefabs` �ļ��У�������һ�� Prefab��
    *   ��󣬽���� Prefab ��ֵ�� `AR Plane Manager` �� `Plane Prefab` �ֶΡ�

5.  **��Ŀ���ü��**:
    *   ǰ�� `Project Settings > XR Plug-in Management`��
    *   ȷ���� Android ��ǩҳ�£�`ARCore` ����Ѿ�����ѡ��

������ϲ���󣬽�Ӧ�ò��� Android �豸�ϣ�����ܿ�������ͷ��׽����ʵ�����У���ʶ�����ƽ��ᱻ��͸�������񸲸ǡ�

---

## �׶ζ���������� (Chat UI) ��������

Ŀ���ǽ��������ݻ��������촰�ڣ��̶��� AR Tutor ���������ͷ����

**ʵ��˼·:**

1.  **ʹ������ռ仭�� (World Space Canvas)**:
    *   ����һ�� `Canvas` GameObject��
    *   ���� `Render Mode` ����Ϊ `World Space`��������UI Ԫ�ؾͻ��� 3D �����е���ͨ����һ���������Ǹ�������Ļ�ϡ�

2.  **UI ��Ϊ�Ӷ���**:
    *   ����� `World Space Canvas` ��ק����� AR Tutor ģ��**ͷ������ (Head Bone) �� Transform**�£���Ϊ�����Ӷ���
    *   ���� Canvas ��λ�ú����ţ�ʹ������λ��ͷ���ĺ���λ�á�

3.  **���������**:
    *   Ϊ��ȷ�������û����ĸ��Ƕȿ���UI �����������û��ģ����Ը� Canvas ���һ���򵥵Ľű��������� `transform.rotation` ��ÿһ֡ (`Update`) ������ `AR Camera` ��λ�á�

ͨ�����ַ�ʽ���� AR Tutor �ƶ�����תʱ��ͷ���� UI Ҳ����֮�ƶ��������������ǡ���������ͷ��һ����
