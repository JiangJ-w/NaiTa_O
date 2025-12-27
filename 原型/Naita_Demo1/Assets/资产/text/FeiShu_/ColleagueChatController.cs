using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class ColleagueChatController : MonoBehaviour
{
    [Header("UI引用 - 拖拽到这里")]
    public TMP_InputField inputField;          // 输入框
    public Transform messageContainer;         // 消息容器（Content对象）
    public GameObject messagePrefab;           // 消息预制体
    public Button sendButton;                  // 发送按钮
    public Button backButton;                  // 返回按钮
    
    [Header("聊天设置")]
    public string colleagueName = "同事";      // 同事名字
    public float replyDelay = 0.8f;            // 回复延迟时间（秒）
    
    [Header("同事回复内容")]
    [TextArea(2, 3)]
    public string[] colleagueReplies = {
        "哈哈，我也觉得！",
        "晚上一起吃饭？",
        "周末有什么计划？",
        "这个项目你那边怎么样了？",
        "加油！我们一起努力！",
        "昨天看到一家不错的餐厅",
        "你听说了吗？公司下个月有团建",
        "这个任务完成得不错呀",
        "需要帮忙随时叫我",
        "一起喝杯咖啡吗？"
    };
    
    private List<GameObject> messages = new List<GameObject>();  // 保存所有消息
    
    void Start()
    {
        // 绑定发送按钮点击事件
        if (sendButton != null)
        {
            sendButton.onClick.AddListener(SendMessage);
        }
        else
        {
            Debug.LogError("发送按钮未设置！请在Inspector中拖拽发送按钮到sendButton字段");
        }
        
        // 绑定返回按钮点击事件
        if (backButton != null)
        {
            backButton.onClick.AddListener(ReturnToMainMenu);
        }
        else
        {
            Debug.LogError("返回按钮未设置！请在Inspector中拖拽返回按钮到backButton字段");
        }
        
        // 绑定回车键发送
        if (inputField != null)
        {
            inputField.onSubmit.AddListener((text) => SendMessage());
        }
        
        // 发送一条欢迎消息
        Invoke("SendWelcomeMessage", 0.5f);
    }
    
    void Update()
    {
        // 检测回车键发送（如果输入框被选中）
        if (inputField != null && inputField.isFocused && Input.GetKeyDown(KeyCode.Return))
        {
            SendMessage();
        }
    }
    
    // 发送欢迎消息
    void SendWelcomeMessage()
    {
        AddMessage(colleagueName, "嗨！今天怎么样？", true);
    }
    
    // 发送消息（用户点击发送按钮时调用）
    public void SendMessage()
    {
        // 获取输入框内容
        string userMessage = "";
        if (inputField != null)
        {
            userMessage = inputField.text.Trim();
        }
        
        // 检查消息是否为空
        if (string.IsNullOrEmpty(userMessage))
        {
            Debug.Log("消息为空，不发送");
            return;
        }
        
        // 显示用户消息
        AddMessage("你", userMessage, false);
        
        // 清空输入框
        if (inputField != null)
        {
            inputField.text = "";
            inputField.ActivateInputField(); // 重新激活输入框
        }
        
        // 延迟后模拟同事回复
        if (colleagueReplies.Length > 0)
        {
            Invoke("SendColleagueReply", replyDelay);
        }
    }
    
    // 模拟同事回复
    void SendColleagueReply()
    {
        // 随机选择一个回复
        int randomIndex = Random.Range(0, colleagueReplies.Length);
        string reply = colleagueReplies[randomIndex];
        
        // 显示同事消息
        AddMessage(colleagueName, reply, true);
    }
    
    // 添加消息到聊天窗口
    void AddMessage(string sender, string message, bool isLeftAlign)
    {
        // 检查必要组件
        if (messagePrefab == null)
        {
            Debug.LogError("消息预制体未设置！请在Inspector中拖拽消息预制体到messagePrefab字段");
            return;
        }
        
        if (messageContainer == null)
        {
            Debug.LogError("消息容器未设置！请拖拽Content对象到messageContainer字段");
            return;
        }
        
        // 实例化消息预制体
        GameObject messageObj = Instantiate(messagePrefab, messageContainer);
        
        // 获取TMP_Text组件
        TMP_Text textComponent = messageObj.GetComponentInChildren<TMP_Text>();
        if (textComponent != null)
        {
            // 设置消息内容
            string timeStamp = System.DateTime.Now.ToString("HH:mm");
            textComponent.text = $"{sender}: {message}\n<size=12><color=#888888>{timeStamp}</color></size>";
            
            // 设置对齐方式（可选）
            if (isLeftAlign)
            {
                textComponent.alignment = TextAlignmentOptions.Left;
            }
            else
            {
                textComponent.alignment = TextAlignmentOptions.Right;
                // 可以改变用户消息的颜色
                textComponent.color = new Color(0.1f, 0.5f, 1f); // 蓝色
            }
        }
        
        // 保存消息引用
        messages.Add(messageObj);
        
        // 自动滚动到底部
        ScrollToBottom();
    }
    
    // 滚动到聊天窗口底部
    void ScrollToBottom()
    {
        // 获取ScrollRect组件
        ScrollRect scrollRect = GetComponentInChildren<ScrollRect>();
        if (scrollRect != null)
        {
            // 延迟一帧确保布局更新
            StartCoroutine(ScrollToBottomCoroutine(scrollRect));
        }
    }
    
    // 协程：滚动到底部
    System.Collections.IEnumerator ScrollToBottomCoroutine(ScrollRect scrollRect)
    {
        yield return new WaitForEndOfFrame();
        Canvas.ForceUpdateCanvases();
        scrollRect.verticalNormalizedPosition = 0f;
    }
    
    // 返回主菜单（销毁当前界面）
    void ReturnToMainMenu()
    {
        Destroy(gameObject);
    }
    
    // 清理所有消息（可选）
    void ClearAllMessages()
    {
        foreach (GameObject msg in messages)
        {
            if (msg != null)
            {
                Destroy(msg);
            }
        }
        messages.Clear();
    }
    
    // 当界面被销毁时清理
    void OnDestroy()
    {
        // 移除所有监听器，防止内存泄漏
        if (sendButton != null)
        {
            sendButton.onClick.RemoveAllListeners();
        }
        
        if (backButton != null)
        {
            backButton.onClick.RemoveAllListeners();
        }
        
        if (inputField != null)
        {
            inputField.onSubmit.RemoveAllListeners();
        }
        
        ClearAllMessages();
    }
}