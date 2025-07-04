using UnityEngine;

/// <summary>
/// ѧϰģʽ������������ - �Զ���ʼ��ѧϰģʽϵͳ
/// </summary>
[DefaultExecutionOrder(-100)]
public class LearningModeInitializer : MonoBehaviour
{
    [Header("Auto Setup")]
    public bool autoCreateLearningModeManager = true;
    
    private void Awake()
    {
        // ���û��LearningModeManager���Զ�����һ��
        if (autoCreateLearningModeManager && LearningModeManager.Instance == null)
        {
            GameObject modeManager = new GameObject("LearningModeManager");
            modeManager.AddComponent<LearningModeManager>();
            
            Debug.Log("LearningModeManager auto-created by LearningModeInitializer");
        }
    }
}
