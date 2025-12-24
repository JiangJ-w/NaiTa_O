using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

/// <summary>
/// 可交互物品 - 定义物品的交互行为
/// </summary>
public class InteractableObject : MonoBehaviour
{
    [Header("=== 基本设置 ===")]
    [Tooltip("物品的标签（自动设置）")]
    [SerializeField] private string interactionTag = "Interactable";

    [Tooltip("是否在开始时自动初始化")]
    [SerializeField] private bool autoInitialize = true;

    [Header("=== 高亮设置 ===")]
    [Tooltip("是否使用材质高亮")]
    [SerializeField] private bool useMaterialHighlight = true;

    [Tooltip("高亮时使用的材质")]
    [SerializeField] private Material highlightMaterial;

    [Tooltip("高亮时是否闪烁")]
    [SerializeField] private bool usePulsingEffect = false;

    [Tooltip("闪烁速度")]
    [SerializeField] private float pulseSpeed = 2f;

    [Header("=== 对话设置 ===")]
    [Tooltip("对话文本（支持多段对话）")]
    [TextArea(2, 5)]
    [SerializeField] private List<string> dialogueLines = new List<string>() { "这是第一段对话。" };

    [Tooltip("打字机效果的速度（秒/字符）。设为0立即显示")]
    [Range(0f, 0.1f)]
    [SerializeField] private float textDisplaySpeed = 0.05f;

    [Tooltip("交互提示文本（显示在屏幕上）")]
    [SerializeField] private string interactionPrompt = "按左键查看";

    [Header("=== 事件 ===")]
    [Tooltip("开始交互时触发")]
    public UnityEvent OnInteractionStart;

    [Tooltip("对话结束时触发")]
    public UnityEvent OnDialogueEnd;

    [Tooltip("鼠标悬停时触发")]
    public UnityEvent OnHoverEnter;

    [Tooltip("鼠标离开时触发")]
    public UnityEvent OnHoverExit;

    // 私有变量
    private Material originalMaterial;
    private Renderer objectRenderer;
    private bool isHighlighted = false;
    private int currentDialogueIndex = 0;
    private bool isInDialogue = false;
    private float pulseTimer = 0f;

    void Start()
    {
        if (autoInitialize)
        {
            Initialize();
        }
    }

    void Update()
    {
        // 更新闪烁效果
        if (isHighlighted && usePulsingEffect && objectRenderer != null && highlightMaterial != null)
        {
            UpdatePulsingEffect();
        }
    }

    /// <summary>
    /// 初始化物品
    /// </summary>
    public void Initialize()
    {
        // 设置标签
        gameObject.tag = interactionTag;

        // 获取渲染器
        objectRenderer = GetComponent<Renderer>();
        if (objectRenderer != null)
        {
            originalMaterial = objectRenderer.material;
        }
        else
        {
            Debug.LogWarning($"{name} 没有Renderer组件，无法高亮！");
        }

        // 确保有碰撞器（探测框需要）
        if (GetComponent<Collider>() == null)
        {
            gameObject.AddComponent<BoxCollider>();
            Debug.Log($"为 {name} 添加了BoxCollider");
        }

        Debug.Log($"{name} 初始化完成");
    }

    /// <summary>
    /// 启用高亮
    /// </summary>
    public void EnableHighlight()
    {
        if (!useMaterialHighlight || isHighlighted || objectRenderer == null) return;

        if (highlightMaterial != null)
        {
            objectRenderer.material = highlightMaterial;
            isHighlighted = true;
            pulseTimer = 0f;

            // 触发悬停事件
            OnHoverEnter?.Invoke();

            Debug.Log($"{name} 高亮已启用");
        }
        else
        {
            Debug.LogWarning($"{name} 的高亮材质未设置！");
        }
    }

    /// <summary>
    /// 禁用高亮（恢复原始材质）
    /// </summary>
    public void DisableHighlight()
    {
        if (!isHighlighted || objectRenderer == null) return;

        objectRenderer.material = originalMaterial;
        isHighlighted = false;

        // 触发离开事件
        OnHoverExit?.Invoke();

        Debug.Log($"{name} 高亮已禁用");
    }

    /// <summary>
    /// 更新闪烁效果
    /// </summary>
    void UpdatePulsingEffect()
    {
        pulseTimer += Time.deltaTime * pulseSpeed;
        float alpha = 0.5f + Mathf.Sin(pulseTimer) * 0.5f; // 0-1之间变化

        Color color = highlightMaterial.color;
        color.a = alpha;

        // 创建新的材质实例避免影响其他物体
        Material currentMat = objectRenderer.material;
        currentMat.color = color;

        if (currentMat.HasProperty("_EmissionColor"))
        {
            currentMat.SetColor("_EmissionColor", color * 2f);
        }
    }

    /// <summary>
    /// 开始交互（对话）
    /// </summary>
    public void StartInteraction()
    {
        if (isInDialogue) return;

        isInDialogue = true;
        currentDialogueIndex = 0;

        // 对话开始时恢复原始材质
        DisableHighlight();

        // 触发开始事件
        OnInteractionStart?.Invoke();

        // 通知UI管理器开始对话
        if (InteractionUIManager.Instance != null)
        {
            InteractionUIManager.Instance.StartDialogue(this);
        }
        else
        {
            Debug.LogError("未找到InteractionUIManager实例！");
        }

        // 显示第一段对话
        ShowCurrentDialogue();

        Debug.Log($"{name} 开始交互，共 {dialogueLines.Count} 段对话");
    }

    /// <summary>
    /// 显示当前对话
    /// </summary>
    void ShowCurrentDialogue()
    {
        if (currentDialogueIndex < dialogueLines.Count)
        {
            string dialogue = dialogueLines[currentDialogueIndex];

            if (InteractionUIManager.Instance != null)
            {
                if (textDisplaySpeed > 0)
                {
                    // 打字机效果
                    InteractionUIManager.Instance.ShowDialogueText(dialogue, this, textDisplaySpeed);
                }
                else
                {
                    // 立即显示
                    InteractionUIManager.Instance.ShowDialogueText(dialogue, this, 0);
                }
            }
        }
        else
        {
            Debug.LogWarning("对话索引超出范围！");
        }
    }

    /// <summary>
    /// 下一句对话
    /// </summary>
    public void NextDialogue()
    {
        currentDialogueIndex++;

        if (currentDialogueIndex < dialogueLines.Count)
        {
            // 显示下一句
            ShowCurrentDialogue();
            Debug.Log($"{name} 第 {currentDialogueIndex + 1}/{dialogueLines.Count} 段对话");
        }
        else
        {
            // 对话结束
            EndDialogue();
        }
    }

    /// <summary>
    /// 结束对话
    /// </summary>
    void EndDialogue()
    {
        isInDialogue = false;

        // 触发结束事件
        OnDialogueEnd?.Invoke();

        // 通知UI管理器
        if (InteractionUIManager.Instance != null)
        {
            InteractionUIManager.Instance.EndDialogue();
        }

        Debug.Log($"{name} 对话结束");
    }

    /// <summary>
    /// 跳过对话
    /// </summary>
    public void SkipDialogue()
    {
        EndDialogue();
        Debug.Log($"{name} 对话已跳过");
    }

    // === 公开方法 ===

    /// <summary>
    /// 获取交互提示文本
    /// </summary>
    public string GetInteractionPrompt()
    {
        return interactionPrompt;
    }

    /// <summary>
    /// 获取对话列表
    /// </summary>
    public List<string> GetDialogueLines()
    {
        return new List<string>(dialogueLines);
    }

    /// <summary>
    /// 获取当前对话索引
    /// </summary>
    public int GetCurrentDialogueIndex()
    {
        return currentDialogueIndex;
    }

    /// <summary>
    /// 获取总对话数量
    /// </summary>
    public int GetDialogueCount()
    {
        return dialogueLines.Count;
    }

    /// <summary>
    /// 获取当前显示的文本
    /// </summary>
    public string GetCurrentDialogueText()
    {
        if (currentDialogueIndex >= 0 && currentDialogueIndex < dialogueLines.Count)
        {
            return dialogueLines[currentDialogueIndex];
        }
        return "";
    }

    /// <summary>
    /// 是否在对话中
    /// </summary>
    public bool IsInDialogue()
    {
        return isInDialogue;
    }

    /// <summary>
    /// 是否被高亮
    /// </summary>
    public bool IsHighlighted()
    {
        return isHighlighted;
    }

    /// <summary>
    /// 设置高亮材质
    /// </summary>
    public void SetHighlightMaterial(Material newMaterial)
    {
        highlightMaterial = newMaterial;
    }

    /// <summary>
    /// 添加对话行
    /// </summary>
    public void AddDialogueLine(string line)
    {
        dialogueLines.Add(line);
    }

    /// <summary>
    /// 设置对话列表
    /// </summary>
    public void SetDialogueLines(List<string> lines)
    {
        dialogueLines = new List<string>(lines);
    }

    /// <summary>
    /// 设置交互提示文本
    /// </summary>
    public void SetInteractionPrompt(string prompt)
    {
        interactionPrompt = prompt;
    }

    void OnDestroy()
    {
        if (isHighlighted)
        {
            DisableHighlight();
        }
    }

#if UNITY_EDITOR
    /// <summary>
    /// 在编辑器中绘制交互范围（调试用）
    /// </summary>
    void OnDrawGizmosSelected()
    {
        // 绘制图标表示这是可交互物品
        Gizmos.color = new Color(0, 1, 0, 0.5f);
        Gizmos.DrawWireSphere(transform.position, 0.3f);

        // 绘制向上的线表示交互点
        Gizmos.DrawLine(transform.position, transform.position + Vector3.up * 0.5f);
        Gizmos.DrawSphere(transform.position + Vector3.up * 0.5f, 0.1f);
    }
#endif
}