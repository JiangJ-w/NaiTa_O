using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class tongshiPanel : MonoBehaviour, IPointerClickHandler
{
    [Header("触发对象")]
    public Toggle metor;     // metor Toggle（勾选时隐藏tongshiPanel）
    public Toggle tongshi;   // tongshi Toggle（勾选时显示tongshiPanel）
    
    [Header("互斥Panel")]
    public GameObject metorPanel;  // metorPanel（WX_Panel），不能同时显示
    
    [Header("淡入动画设置")]
    [Tooltip("Panel淡入动画时间（秒），0表示立即显示无动画")]
    public float fadeInDuration = 0f;  // 淡入动画时间
    
    [Header("metorPanel淡入动画设置")]
    [Tooltip("metorPanel淡入动画时间（秒），0表示立即显示无动画")]
    public float metorFadeInDuration = 0f;  // metorPanel淡入动画时间

    // Start is called before the first frame update
    void Start()
    {
        // 初始化时隐藏Panel
        gameObject.SetActive(false);

        // 为metor添加Toggle事件（显示metorPanel，隐藏tongshiPanel）
        if (metor != null)
        {
            metor.onValueChanged.RemoveListener(OnMetorToggle);
            metor.onValueChanged.AddListener(OnMetorToggle);
            
            // 为metor Toggle添加点击事件，即使Toggle已选中也能响应
            EventTrigger metorTrigger = metor.gameObject.GetComponent<EventTrigger>();
            if (metorTrigger == null)
            {
                metorTrigger = metor.gameObject.AddComponent<EventTrigger>();
            }
            
            // 移除旧的点击事件（如果存在）
            var metorEntries = metorTrigger.triggers;
            for (int i = metorEntries.Count - 1; i >= 0; i--)
            {
                if (metorEntries[i].eventID == EventTriggerType.PointerClick)
                {
                    metorEntries.RemoveAt(i);
                }
            }
            
            // 添加新的点击事件
            EventTrigger.Entry metorEntry = new EventTrigger.Entry();
            metorEntry.eventID = EventTriggerType.PointerClick;
            metorEntry.callback.AddListener((data) => { OnMetorClick(); });
            metorTrigger.triggers.Add(metorEntry);
            
            // 如果metor Toggle初始状态是选中，则显示metorPanel
            if (metor.isOn)
            {
                ShowMetorPanel();
            }
        }
        else
        {
            Debug.LogWarning("tongshiPanel: metor Toggle 未设置！");
        }
        
        // 为tongshi添加Toggle事件（显示tongshiPanel）
        if (tongshi != null)
        {
            tongshi.onValueChanged.RemoveListener(OnTongshiToggle);
            tongshi.onValueChanged.AddListener(OnTongshiToggle);
            
            // 为tongshi Toggle添加点击事件，即使Toggle已选中也能响应
            EventTrigger trigger = tongshi.gameObject.GetComponent<EventTrigger>();
            if (trigger == null)
            {
                trigger = tongshi.gameObject.AddComponent<EventTrigger>();
            }
            
            // 移除旧的点击事件（如果存在）
            var entries = trigger.triggers;
            for (int i = entries.Count - 1; i >= 0; i--)
            {
                if (entries[i].eventID == EventTriggerType.PointerClick)
                {
                    entries.RemoveAt(i);
                }
            }
            
            // 添加新的点击事件
            EventTrigger.Entry entry = new EventTrigger.Entry();
            entry.eventID = EventTriggerType.PointerClick;
            entry.callback.AddListener((data) => { OnTongshiClick(); });
            trigger.triggers.Add(entry);
            
            // 如果tongshi Toggle初始状态是选中，则显示Panel
            if (tongshi.isOn)
            {
                ShowPanel();
            }
        }
        else
        {
            Debug.LogWarning("tongshiPanel: tongshi Toggle 未设置！");
        }
    }

    // metor Toggle事件处理
    void OnMetorToggle(bool isOn)
    {
        if (isProcessingToggle) return;
        
        Debug.Log($"tongshiPanel: OnMetorToggle called, isOn = {isOn}, metorPanel active: {metorPanel != null && metorPanel.activeSelf}");
        if (isOn)
        {
            isProcessingToggle = true;
            // 确保tongshi Toggle被取消选中（如果它们互斥）
            if (tongshi != null && tongshi.isOn)
            {
                tongshi.isOn = false;
            }
            // 显示metorPanel，隐藏tongshiPanel
            ShowMetorPanel();
            isProcessingToggle = false;
        }
        else
        {
            // 当metor取消勾选时，也隐藏metorPanel
            HideMetorPanel();
        }
    }
    
    // metor Toggle点击事件处理（即使Toggle已选中也会触发）
    void OnMetorClick()
    {
        if (isProcessingToggle) return;
        
        Debug.Log($"tongshiPanel: OnMetorClick called, isOn = {metor.isOn}, metorPanel active: {metorPanel != null && metorPanel.activeSelf}");
        
        // 如果Toggle已选中但metorPanel未显示，直接显示metorPanel
        if (metor != null && metor.isOn && metorPanel != null && !metorPanel.activeSelf)
        {
            isProcessingToggle = true;
            // 确保tongshi Toggle被取消选中（如果它们互斥）
            if (tongshi != null && tongshi.isOn)
            {
                tongshi.isOn = false;
            }
            ShowMetorPanel();
            isProcessingToggle = false;
        }
    }

    // tongshi Toggle事件处理
    void OnTongshiToggle(bool isOn)
    {
        if (isProcessingToggle) return;
        
        Debug.Log($"tongshiPanel: OnTongshiToggle called, isOn = {isOn}, Panel active: {gameObject.activeSelf}");
        if (isOn)
        {
            isProcessingToggle = true;
            // 确保metor Toggle被取消选中（如果它们互斥）
            if (metor != null && metor.isOn)
            {
                metor.isOn = false;
            }
            // 无论Panel当前状态如何，都显示Panel（确保状态同步）
            ShowPanel();
            isProcessingToggle = false;
        }
        else
        {
            // 当tongshi取消勾选时，也隐藏panel
            HidePanel();
        }
    }

    private CanvasGroup canvasGroup;
    private CanvasGroup metorCanvasGroup;  // metorPanel的CanvasGroup
    private Coroutine fadeCoroutine;  // 用于存储淡入协程，以便可以停止它
    private Coroutine metorFadeCoroutine;  // 用于存储metorPanel淡入协程
    private bool isProcessingToggle = false;  // 防止递归调用的标志
    
    void Awake()
    {
        // 获取或添加CanvasGroup组件用于控制可见性
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
        
        // 为metorPanel获取或添加CanvasGroup组件
        if (metorPanel != null)
        {
            metorCanvasGroup = metorPanel.GetComponent<CanvasGroup>();
            if (metorCanvasGroup == null)
            {
                metorCanvasGroup = metorPanel.AddComponent<CanvasGroup>();
            }
        }
    }
    
    // tongshi Toggle点击事件处理（即使Toggle已选中也会触发）
    void OnTongshiClick()
    {
        if (isProcessingToggle) return;
        
        Debug.Log($"tongshiPanel: OnTongshiClick called, isOn = {tongshi.isOn}, Panel active: {gameObject.activeSelf}");
        
        // 如果Toggle已选中但Panel未显示，直接显示Panel
        if (tongshi != null && tongshi.isOn && !gameObject.activeSelf)
        {
            isProcessingToggle = true;
            // 确保metor Toggle被取消选中（如果它们互斥）
            if (metor != null && metor.isOn)
            {
                metor.isOn = false;
            }
            ShowPanel();
            isProcessingToggle = false;
        }
    }
    
    // IPointerClickHandler接口实现（用于Panel自身的点击处理，如果需要的话）
    public void OnPointerClick(PointerEventData eventData)
    {
        // 可以在这里添加Panel点击时的处理逻辑
    }
    
    // 显示Panel（同时隐藏metorPanel）
    void ShowPanel()
    {
        Debug.Log("tongshiPanel: ShowPanel called");
        
        // 如果Panel已经激活，先确保状态正确
        if (gameObject.activeSelf)
        {
            // 停止任何正在运行的淡入协程
            if (fadeCoroutine != null)
            {
                StopCoroutine(fadeCoroutine);
                fadeCoroutine = null;
            }
        }
        
        // 先隐藏metorPanel，实现互斥
        HideMetorPanel();
        
        // 立即激活GameObject，确保Panel立即可见
        gameObject.SetActive(true);
        
        // 确保CanvasGroup组件存在
        if (canvasGroup == null)
        {
            canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }
        }
        
        // 如果有淡入动画时间，使用淡入效果；否则立即完全显示
        if (fadeInDuration > 0f)
        {
            // 从透明开始淡入
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
            fadeCoroutine = StartCoroutine(FadeInPanel());
        }
        else
        {
            // 立即完全显示
            canvasGroup.alpha = 1f;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }
    }
    
    // 淡入动画协程
    IEnumerator FadeInPanel()
    {
        float elapsedTime = 0f;
        
        while (elapsedTime < fadeInDuration)
        {
            elapsedTime += Time.deltaTime;
            if (canvasGroup != null)
            {
                canvasGroup.alpha = Mathf.Clamp01(elapsedTime / fadeInDuration);
            }
            yield return null;
        }
        
        // 确保最终alpha为1
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
        }
        
        fadeCoroutine = null;
    }

    // 显示metorPanel（同时隐藏tongshiPanel）
    void ShowMetorPanel()
    {
        if (metorPanel == null)
        {
            Debug.LogWarning("tongshiPanel: metorPanel 未设置！");
            return;
        }
        
        Debug.Log("tongshiPanel: ShowMetorPanel called");
        
        // 如果metorPanel已经激活，先确保状态正确
        if (metorPanel.activeSelf)
        {
            // 停止任何正在运行的淡入协程
            if (metorFadeCoroutine != null)
            {
                StopCoroutine(metorFadeCoroutine);
                metorFadeCoroutine = null;
            }
        }
        
        // 先隐藏tongshiPanel，实现互斥
        HidePanel();
        
        // 立即激活metorPanel，确保Panel立即可见
        metorPanel.SetActive(true);
        
        // 确保metorCanvasGroup组件存在
        if (metorCanvasGroup == null)
        {
            metorCanvasGroup = metorPanel.GetComponent<CanvasGroup>();
            if (metorCanvasGroup == null)
            {
                metorCanvasGroup = metorPanel.AddComponent<CanvasGroup>();
            }
        }
        
        // 如果有淡入动画时间，使用淡入效果；否则立即完全显示
        if (metorFadeInDuration > 0f)
        {
            // 从透明开始淡入
            metorCanvasGroup.alpha = 0f;
            metorCanvasGroup.interactable = true;
            metorCanvasGroup.blocksRaycasts = true;
            metorFadeCoroutine = StartCoroutine(FadeInMetorPanel());
        }
        else
        {
            // 立即完全显示
            metorCanvasGroup.alpha = 1f;
            metorCanvasGroup.interactable = true;
            metorCanvasGroup.blocksRaycasts = true;
        }
    }
    
    // metorPanel淡入动画协程
    IEnumerator FadeInMetorPanel()
    {
        float elapsedTime = 0f;
        
        while (elapsedTime < metorFadeInDuration)
        {
            elapsedTime += Time.deltaTime;
            if (metorCanvasGroup != null)
            {
                metorCanvasGroup.alpha = Mathf.Clamp01(elapsedTime / metorFadeInDuration);
            }
            yield return null;
        }
        
        // 确保最终alpha为1
        if (metorCanvasGroup != null)
        {
            metorCanvasGroup.alpha = 1f;
        }
        
        metorFadeCoroutine = null;
    }
    
    // 隐藏metorPanel
    void HideMetorPanel()
    {
        if (metorPanel == null) return;
        
        Debug.Log("tongshiPanel: HideMetorPanel called");
        
        // 停止任何正在运行的淡入协程
        if (metorFadeCoroutine != null)
        {
            StopCoroutine(metorFadeCoroutine);
            metorFadeCoroutine = null;
        }
        
        // 重置CanvasGroup状态，确保下次显示时状态正确
        if (metorCanvasGroup != null)
        {
            metorCanvasGroup.alpha = 0f;
            metorCanvasGroup.interactable = false;
            metorCanvasGroup.blocksRaycasts = false;
        }
        
        metorPanel.SetActive(false);
    }
    
    // 隐藏Panel
    void HidePanel()
    {
        Debug.Log("tongshiPanel: HidePanel called");
        
        // 停止任何正在运行的淡入协程
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
            fadeCoroutine = null;
        }
        
        // 重置CanvasGroup状态，确保下次显示时状态正确
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }
        
        gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
