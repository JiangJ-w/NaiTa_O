using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CanvasComputer : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("隐藏时间（秒）")]
    public float hideDuration = 1f;
    [Tooltip("相机切换器引用")]
    public CameraSwitcher cameraSwitcher;
    
    private Canvas canvas;
    private CanvasGroup canvasGroup;
    private bool was2DMode = false;
    private bool isHiding = false;
    
    void Start()
    {
        // 获取Canvas或CanvasGroup组件
        canvas = GetComponent<Canvas>();
        canvasGroup = GetComponent<CanvasGroup>();
        
        // 如果没有CanvasGroup，创建一个
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
        
        // 如果没有找到CameraSwitcher，尝试在场景中查找
        if (cameraSwitcher == null)
        {
            cameraSwitcher = FindObjectOfType<CameraSwitcher>();
        }
        
        // 初始化：根据当前相机状态设置显示
        if (cameraSwitcher != null)
        {
            was2DMode = cameraSwitcher.camera2D.gameObject.activeSelf && 
                       !cameraSwitcher.camera3D.gameObject.activeSelf;
        }
    }

    void Update()
    {
        if (cameraSwitcher == null || isHiding) return;
        
        // 检查是否真正切换到2D模式（2D激活且3D未激活）
        bool is2DMode = cameraSwitcher.camera2D.gameObject.activeSelf && 
                        !cameraSwitcher.camera3D.gameObject.activeSelf;
        
        // 如果从3D切换到2D，开始隐藏流程
        if (!was2DMode && is2DMode)
        {
            StartCoroutine(HideAndShow());
        }
        
        was2DMode = is2DMode;
    }
    
    // 立即隐藏Canvas（用于退出2D模式时）
    public void HideImmediately()
    {
        StopAllCoroutines();
        isHiding = false;
        
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }
        else if (canvas != null)
        {
            canvas.enabled = false;
        }
    }
    
    IEnumerator HideAndShow()
    {
        isHiding = true;
        
        // 立即隐藏Canvas
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }
        else if (canvas != null)
        {
            canvas.enabled = false;
        }
        
        // 等待1秒
        yield return new WaitForSeconds(hideDuration);
        
        // 显示Canvas
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }
        else if (canvas != null)
        {
            canvas.enabled = true;
        }
        
        isHiding = false;
    }
}
