using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class WorkAppController : MonoBehaviour
{
    [Header("基本设置")]
    public string appName = "工作软件";
    
    [Header("UI引用 - 拖拽到这里")]
    public GameObject[] tabPanels; // 按顺序：0上司,1同事,2工作
    public Button[] tabButtons;    // 同上顺序
    public Button closeButton;
    
    [Header("聊天系统")]
    public TMP_InputField chatInputField;
    public Transform messageContainer;
    public GameObject messagePrefab;
    
    [Header("工作文档")]
    public RawImage documentImage;
    public Texture2D workDocumentTexture;
    public GameObject thoughtBubblePrefab;
    
    void Start()
    {
        // 绑定按钮事件
        if (closeButton) closeButton.onClick.AddListener(() => Destroy(gameObject));
        
        // 标签页按钮事件
        for (int i = 0; i < tabButtons.Length; i++)
        {
            int index = i;
            tabButtons[i].onClick.AddListener(() => SwitchTab(index));
        }
        
        // 初始化显示第一个标签页
        SwitchTab(0);
        
        // 设置文档图片
        if (documentImage && workDocumentTexture)
        {
            documentImage.texture = workDocumentTexture;
        }
    }
    
    void SwitchTab(int index)
    {
        // 隐藏所有面板
        foreach (var panel in tabPanels)
        {
            panel.SetActive(false);
        }
        
        // 显示选中面板
        if (index >= 0 && index < tabPanels.Length)
        {
            tabPanels[index].SetActive(true);
        }
    }
    
    // 上司聊天发送
    public void SendBossMessage()
    {
        if (string.IsNullOrEmpty(chatInputField.text)) return;
        AddMessage("你", chatInputField.text);
        chatInputField.text = "";
        Invoke("BossReply", 1f);
    }
    
    void BossReply()
    {
        string[] replies = {"好的，继续努力", "需要更多细节", "明天会议讨论"};
        AddMessage("上司", replies[Random.Range(0, replies.Length)]);
    }
    
    // 同事聊天发送
    public void SendColleagueMessage()
    {
        if (string.IsNullOrEmpty(chatInputField.text)) return;
        AddMessage("你", chatInputField.text);
        chatInputField.text = "";
        Invoke("ColleagueReply", 0.8f);
    }
    
    void ColleagueReply()
    {
        string[] replies = {"哈哈，我也觉得", "晚上一起吃饭?", "周末有什么计划?"};
        AddMessage("同事", replies[Random.Range(0, replies.Length)]);
    }
    
    void AddMessage(string sender, string message)
    {
        if (!messagePrefab || !messageContainer) return;
        
        GameObject msg = Instantiate(messagePrefab, messageContainer);
        TMP_Text text = msg.GetComponentInChildren<TMP_Text>();
        if (text) text.text = $"{sender}: {message}";
    }
    
    // 显示想法气泡
    public void ShowThought(Vector2 position, string thought)
    {
        if (!thoughtBubblePrefab) return;
        
        GameObject bubble = Instantiate(thoughtBubblePrefab, transform);
        bubble.GetComponent<RectTransform>().anchoredPosition = position;
        bubble.GetComponent<ThoughtBubble>().SetText(thought);
    }
}