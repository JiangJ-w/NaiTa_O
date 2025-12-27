using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MainAppController : MonoBehaviour
{
    [Header("主菜单UI - 拖拽到这里")]
    public GameObject mainMenuPanel;      // 主菜单面板
    public Button bossChatButton;         // 上司聊天按钮
    public Button colleagueChatButton;    // 同事聊天按钮
    public Button workDocumentButton;     // 工作档案按钮
    public Button closeButton;            // 关闭按钮
    
    [Header("功能界面预制体 - 拖拽到这里")]
    public GameObject bossChatPrefab;     // 上司聊天界面预制体
    public GameObject colleagueChatPrefab; // 同事聊天界面预制体
    public GameObject workDocumentPrefab; // 工作档案界面预制体
    
    [Header("界面位置")]
    public Transform panelSpawnPoint;     // 界面打开位置（可选）
    
    private GameObject currentPanel;      // 当前打开的面板
    
    void Start()
    {
        // 绑定按钮事件 - 修复这里！
        bossChatButton.onClick.AddListener(() => OpenPanel(bossChatPrefab, "上司聊天"));
        colleagueChatButton.onClick.AddListener(() => OpenPanel(colleagueChatPrefab, "同事聊天"));
        workDocumentButton.onClick.AddListener(() => OpenPanel(workDocumentPrefab, "工作档案"));
        closeButton.onClick.AddListener(() => Destroy(gameObject));
        
        // 默认显示主菜单
        ShowMainMenu();
    }
    
    void OpenPanel(GameObject panelPrefab, string panelName)
    {
        // 关闭当前打开的面板
        if (currentPanel != null)
        {
            Destroy(currentPanel);
        }
        
        // 隐藏主菜单
        mainMenuPanel.SetActive(false);
        
        // 创建新面板
        Vector3 spawnPosition = panelSpawnPoint != null ? 
            panelSpawnPoint.position : transform.position;
        
        currentPanel = Instantiate(panelPrefab, spawnPosition, Quaternion.identity, transform);
        
        // 设置面板名称
        currentPanel.name = panelName + "_Panel";
        
        // 为面板添加返回按钮功能
        AddBackButtonToPanel(currentPanel);
    }
    
    void AddBackButtonToPanel(GameObject panel)
    {
        // 查找面板中的返回按钮
        Button backButton = null;
        Button[] buttons = panel.GetComponentsInChildren<Button>();
        
        foreach (Button btn in buttons)
        {
            if (btn.name == "BackButton" || btn.name.Contains("返回") || btn.GetComponentInChildren<TMP_Text>()?.text.Contains("返回") == true)
            {
                backButton = btn;
                break;
            }
        }
        
        // 如果没有找到返回按钮，创建一个
        if (backButton == null)
        {
            backButton = CreateBackButton(panel);
        }
        
        // 绑定返回事件 - 修复这里！
        backButton.onClick.RemoveAllListeners(); // 先移除所有监听器
        backButton.onClick.AddListener(() => ShowMainMenu());
    }
    
    Button CreateBackButton(GameObject panel)
    {
        // 创建一个新的返回按钮
        GameObject backButtonObj = new GameObject("BackButton");
        backButtonObj.transform.SetParent(panel.transform);
        
        // 添加RectTransform
        RectTransform rt = backButtonObj.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.85f, 0.9f);
        rt.anchorMax = new Vector2(0.95f, 0.95f);
        rt.anchoredPosition = Vector2.zero;
        rt.sizeDelta = Vector2.zero;
        
        // 添加Button组件
        Button backButton = backButtonObj.AddComponent<Button>();
        
        // 添加Image组件（按钮背景）
        Image image = backButtonObj.AddComponent<Image>();
        image.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);
        
        // 创建Text子对象
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(backButtonObj.transform);
        
        RectTransform textRT = textObj.AddComponent<RectTransform>();
        textRT.anchorMin = Vector2.zero;
        textRT.anchorMax = Vector2.one;
        textRT.offsetMin = Vector2.zero;
        textRT.offsetMax = Vector2.zero;
        
        // 添加TextMeshPro
        TMP_Text text = textObj.AddComponent<TextMeshProUGUI>();
        text.text = "返回主菜单";
        text.color = Color.white;
        text.alignment = TextAlignmentOptions.Center;
        text.fontSize = 20;
        
        return backButton;
    }
    
    public void ShowMainMenu()
    {
        // 关闭当前面板
        if (currentPanel != null)
        {
            Destroy(currentPanel);
            currentPanel = null;
        }
        
        // 显示主菜单
        mainMenuPanel.SetActive(true);
    }
}