using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject chatPanel;
    public GameObject imageUploadPanel;
    public GameObject mainMenuPanel;
    
    [Header("Main Menu Buttons")]
    public Button chatButton;
    public Button imageUploadButton;
    public Button exitButton;
    
    [Header("Navigation Buttons")]
    public Button backToChatButton;
    public Button backToMenuButton;
    
    [Header("Toggle Buttons")]
    public Button toggleUIButton; // 用于显示/隐藏整个UI
    
    private bool isUIVisible = true;
    
    void Start()
    {
        // 设置按钮事件
        if (chatButton != null)
            chatButton.onClick.AddListener(() => ShowPanel(chatPanel));
            
        if (imageUploadButton != null)
            imageUploadButton.onClick.AddListener(() => ShowPanel(imageUploadPanel));
            
        if (exitButton != null)
            exitButton.onClick.AddListener(ExitApplication);
            
        if (backToChatButton != null)
            backToChatButton.onClick.AddListener(() => ShowPanel(chatPanel));
            
        if (backToMenuButton != null)
            backToMenuButton.onClick.AddListener(() => ShowPanel(mainMenuPanel));
            
        if (toggleUIButton != null)
            toggleUIButton.onClick.AddListener(ToggleUI);
        
        // 默认显示主菜单
        ShowPanel(mainMenuPanel);
    }
    
    void Update()
    {
        // 按ESC键切换UI显示/隐藏
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ToggleUI();
        }
        
        // 按M键返回主菜单
        if (Input.GetKeyDown(KeyCode.M))
        {
            ShowPanel(mainMenuPanel);
        }
    }
    
    public void ShowPanel(GameObject targetPanel)
    {
        // 隐藏所有面板
        if (chatPanel != null) chatPanel.SetActive(false);
        if (imageUploadPanel != null) imageUploadPanel.SetActive(false);
        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
        
        // 显示目标面板
        if (targetPanel != null)
        {
            targetPanel.SetActive(true);
        }
    }
    
    public void ToggleUI()
    {
        isUIVisible = !isUIVisible;
        
        // 切换所有主要UI面板的显示状态
        if (chatPanel != null) chatPanel.SetActive(isUIVisible && chatPanel.activeSelf);
        if (imageUploadPanel != null) imageUploadPanel.SetActive(isUIVisible && imageUploadPanel.activeSelf);
        if (mainMenuPanel != null) mainMenuPanel.SetActive(isUIVisible && mainMenuPanel.activeSelf);
        
        // 如果UI被隐藏，显示一个小提示按钮
        if (toggleUIButton != null)
        {
            toggleUIButton.gameObject.SetActive(!isUIVisible);
        }
    }
    
    public void ExitApplication()
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
}