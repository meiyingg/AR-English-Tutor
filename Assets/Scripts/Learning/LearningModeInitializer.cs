using UnityEngine;

/// <summary>
/// 学习模式管理器启动器 - 自动初始化学习模式系统
/// </summary>
[DefaultExecutionOrder(-100)]
public class LearningModeInitializer : MonoBehaviour
{
    [Header("Auto Setup")]
    public bool autoCreateLearningModeManager = true;
    
    private void Awake()
    {
        // 如果没有LearningModeManager，自动创建一个
        if (autoCreateLearningModeManager && LearningModeManager.Instance == null)
        {
            GameObject modeManager = new GameObject("LearningModeManager");
            modeManager.AddComponent<LearningModeManager>();
            
            Debug.Log("LearningModeManager auto-created by LearningModeInitializer");
        }
    }
}
