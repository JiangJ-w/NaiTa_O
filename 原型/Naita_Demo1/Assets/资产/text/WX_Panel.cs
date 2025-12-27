using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WX_Panel : MonoBehaviour
{
    [Header("触发对象")]
    public Toggle metor;     // 点击metor显示Panel
    public Toggle tongshi;   // 点击tongshi隐藏Panel
    
    [Header("互斥Panel")]
    public GameObject tongshiPanel;  // tongshiPanel，不能同时显示
    
    [Header("延迟设置")]
    [Tooltip("显示Panel前的等待时间（秒），0表示不延迟")]
    public float showDelay = 0f;  // 显示延迟时间

    // Start is called before the first frame update
    void Start()
    {
        // 初始化时隐藏Panel
        gameObject.SetActive(false);

        // 为metor添加Toggle事件（显示Panel）
        if (metor != null)
        {
            metor.onValueChanged.RemoveListener(OnMetorToggle);
            metor.onValueChanged.AddListener(OnMetorToggle);
        }
        
        // 为tongshi添加Toggle事件（隐藏Panel）
        if (tongshi != null)
        {
            tongshi.onValueChanged.RemoveListener(OnTongshiToggle);
            tongshi.onValueChanged.AddListener(OnTongshiToggle);
        }
    }

    // metor Toggle事件处理
    void OnMetorToggle(bool isOn)
    {
        if (isOn)
        {
            ShowPanel();
        }
        else
        {
            // 当metor取消勾选时，也隐藏panel
            HidePanel();
        }
    }

    // tongshi Toggle事件处理
    void OnTongshiToggle(bool isOn)
    {
        if (isOn)
        {
            // tongshi被勾选时，隐藏metorPanel
            HidePanel();
        }
        // 当tongshi取消勾选时，延迟检查metor状态（避免ToggleGroup切换时的时序问题）
        else
        {
            // 先激活GameObject以便可以启动协程，但先设为不可见
            if (!gameObject.activeSelf)
            {
                gameObject.SetActive(true);
                if (canvasGroup != null)
                {
                    canvasGroup.alpha = 0f;
                    canvasGroup.interactable = false;
                    canvasGroup.blocksRaycasts = false;
                }
            }
            StartCoroutine(CheckMetorStateAfterTongshiOff());
        }
    }
    
    // 检查metor状态（在tongshi取消勾选后）
    IEnumerator CheckMetorStateAfterTongshiOff()
    {
        yield return null; // 等待一帧，确保ToggleGroup的状态更新完成
        if (metor != null && metor.isOn)
        {
            ShowPanel();
        }
        else
        {
            // 如果metor没有勾选，隐藏Panel
            HidePanel();
        }
    }

    private CanvasGroup canvasGroup;
    
    void Awake()
    {
        // 获取或添加CanvasGroup组件用于控制可见性
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
    }
    
    // 显示Panel（同时隐藏tongshiPanel）
    void ShowPanel()
    {
        // 先隐藏tongshiPanel，实现互斥
        if (tongshiPanel != null)
        {
            tongshiPanel.SetActive(false);
        }
        
        // 如果有延迟，使用协程延迟显示；否则立即显示
        if (showDelay > 0f)
        {
            // 先激活GameObject以启动协程，但先设为不可见
            gameObject.SetActive(true);
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0f;
                canvasGroup.interactable = false;
                canvasGroup.blocksRaycasts = false;
            }
            StartCoroutine(ShowPanelDelayed());
        }
        else
        {
            gameObject.SetActive(true);
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 1f;
                canvasGroup.interactable = true;
                canvasGroup.blocksRaycasts = true;
            }
        }
    }
    
    // 延迟显示Panel
    IEnumerator ShowPanelDelayed()
    {
        yield return new WaitForSeconds(showDelay);
        // 延迟后显示Panel
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }
    }

    // 隐藏Panel
    void HidePanel()
    {
        gameObject.SetActive(false);
        // 重置CanvasGroup状态（如果需要的话，会在下次ShowPanel时重新设置）
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
