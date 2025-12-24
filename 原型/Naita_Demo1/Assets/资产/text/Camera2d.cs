using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Camera2d : MonoBehaviour
{
    [Header("Canvas 设置")]
    public Canvas canvas; // 在 Unity 编辑器中指定要控制的 Canvas
    
    [Header("延迟设置")]
    [Tooltip("Camera2d 启用后延迟多少秒显示 Canvas")]
    public float delaySeconds = 0f; // 延迟时间（秒），可在 Unity 编辑器中调整

    private Coroutine delayCoroutine; // 延迟协程引用

    void Start()
    {
        // 初始化时禁用 Canvas
        if (canvas != null)
        {
            canvas.gameObject.SetActive(false);
        }
        
        // 如果 Camera2d 一开始就是启用的，启动延迟显示
        if (enabled)
        {
            StartDelayShow();
        }
    }

    void OnEnable()
    {
        // 当 Camera2d 启用时，启动延迟显示 Canvas
        StartDelayShow();
    }

    void OnDisable()
    {
        // 停止延迟协程
        if (delayCoroutine != null)
        {
            StopCoroutine(delayCoroutine);
            delayCoroutine = null;
        }
        
        // 当 Camera2d 禁用时，Canvas 也跟着禁用
        if (canvas != null)
        {
            canvas.gameObject.SetActive(false);
        }
    }

    void StartDelayShow()
    {
        // 停止之前的协程（如果有）
        if (delayCoroutine != null)
        {
            StopCoroutine(delayCoroutine);
        }
        
        // 启动延迟显示协程
        delayCoroutine = StartCoroutine(DelayShowCanvas());
    }

    IEnumerator DelayShowCanvas()
    {
        // 等待指定秒数
        yield return new WaitForSeconds(delaySeconds);
        
        // 延迟后启用 Canvas
        if (canvas != null)
        {
            canvas.gameObject.SetActive(true);
        }
        
        delayCoroutine = null;
    }
}

