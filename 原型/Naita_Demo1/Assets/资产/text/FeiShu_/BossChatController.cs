using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class BossChatController : MonoBehaviour
{
    [Header("UI引用")]
    public TMP_InputField inputField;
    public Transform messageContainer;
    public GameObject messagePrefab;
    public Button sendButton;
    public Button backButton;
    
    [Header("消息设置")]
    public string bossName = "上司";
    public string[] bossReplies = {
        "好的，继续努力",
        "需要更多细节",
        "明天会议讨论",
        "这个方案再优化一下",
        "尽快完成"
    };
    
    private List<GameObject> messages = new List<GameObject>();
    
    void Start()
    {
        // 绑定发送按钮
        if (sendButton != null)
            sendButton.onClick.AddListener(SendMessage);
        
        // 绑定返回按钮 - 直接销毁面板，让主控制器处理
        if (backButton != null)
            backButton.onClick.AddListener(() => Destroy(gameObject));
    }
    
    public void SendMessage()
    {
        string userMessage = inputField.text.Trim();
        if (string.IsNullOrEmpty(userMessage)) return;
        
        // 显示用户消息
        AddMessage("你", userMessage);
        inputField.text = "";
        
        // 模拟上司回复
        Invoke("SendBossReply", 1f);
    }
    
    void SendBossReply()
    {
        string reply = bossReplies[Random.Range(0, bossReplies.Length)];
        AddMessage(bossName, reply);
    }
    
    void AddMessage(string sender, string message)
    {
        if (messagePrefab == null || messageContainer == null) return;
        
        GameObject msg = Instantiate(messagePrefab, messageContainer);
        TMP_Text text = msg.GetComponentInChildren<TMP_Text>();
        if (text != null) 
        {
            text.text = $"{sender}: {message}\n<size=12>{System.DateTime.Now.ToString("HH:mm")}</size>";
        }
        messages.Add(msg);
    }
}