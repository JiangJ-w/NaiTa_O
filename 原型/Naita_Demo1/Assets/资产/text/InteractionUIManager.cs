using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// 交互UI管理器 - 管理所有交互相关的UI显示
/// </summary>
public class InteractionUIManager : MonoBehaviour
{
    // 单例实例
    public static InteractionUIManager Instance { get; private set; }

    [Header("=== UI引用 ===")]
    [Tooltip("对话面板的CanvasGroup")]
    [SerializeField] private CanvasGroup dialoguePanel;

    [Tooltip("对话文本显示")]
    [SerializeField] private Text dialogueText;

    [Tooltip("继续提示文本")]
    [SerializeField] private Text continuePromptText;

    [Tooltip("交互提示的CanvasGroup")]
    [SerializeField] private CanvasGroup interactionPrompt;

    [Tooltip("交互提示文本")]
    [SerializeField] private Text interactionPromptText;

    [Header("=== 对话设置 ===")]
    [Tooltip("淡入淡出速度")]
    [SerializeField] private float fadeSpeed = 5f;

    [Tooltip("继续提示文本")]
    [SerializeField] private string continueText = "点击继续";

    [Tooltip("对话文本颜色")]
    [SerializeField] private Color textColor = Color.white;

    [Tooltip("打字完成后继续提示的闪烁速度")]
    [SerializeField] private float promptBlinkSpeed = 2f;

    [Header("=== 外观设置 ===")]
    [Tooltip("对话字体")]
    [SerializeField] private Font dialogueFont;

    [Tooltip("对话字体大小")]
    [SerializeField] private int dialogueFontSize = 22;

    [Tooltip("提示字体大小")]
    [SerializeField] private int promptFontSize = 18;

    // 状态变量
    private InteractableObject currentDialogueObject;
    private bool isShowingDialogue = false;
    private bool isTyping = false;
    private Coroutine typingCoroutine;
    private Coroutine fadeCoroutine;

    void Awake()
    {
        // 设置单例
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        InitializeUI();
        Debug.Log("InteractionUIManager 初始化完成");
    }

    void Update()
    {
        // 更新交互提示
        UpdateInteractionPrompt();

        // 更新继续提示闪烁效果
        if (isShowingDialogue && continuePromptText != null && continuePromptText.enabled)
        {
            UpdateContinuePromptBlink();
        }

        // 调试快捷键：空格键跳过当前打字
        if (Input.GetKeyDown(KeyCode.Space) && isTyping)
        {
            CompleteCurrentTyping();
        }
    }

    /// <summary>
    /// 初始化UI
    /// </summary>
    void InitializeUI()
    {
        // 初始化对话面板
        if (dialoguePanel != null)
        {
            dialoguePanel.alpha = 0f;
            dialoguePanel.interactable = false;
            dialoguePanel.blocksRaycasts = false;
        }

        // 初始化交互提示
        if (interactionPrompt != null)
        {
            interactionPrompt.alpha = 0f;
        }

        // 初始化继续提示
        if (continuePromptText != null)
        {
            continuePromptText.text = continueText;
            continuePromptText.enabled = false;
        }

        // 设置字体
        if (dialogueText != null)
        {
            dialogueText.fontSize = dialogueFontSize;
            dialogueText.color = textColor;
            if (dialogueFont != null) dialogueText.font = dialogueFont;
        }

        if (interactionPromptText != null)
        {
            interactionPromptText.fontSize = promptFontSize;
        }
    }

    /// <summary>
    /// 开始对话
    /// </summary>
    public void StartDialogue(InteractableObject dialogueObject)
    {
        if (isShowingDialogue) return;

        currentDialogueObject = dialogueObject;
        isShowingDialogue = true;

        // 隐藏交互提示
        HideInteractionPrompt();

        // 显示对话面板
        ShowDialoguePanel(true);

        Debug.Log($"开始显示 {dialogueObject.name} 的对话");
    }

    /// <summary>
    /// 显示对话文本
    /// </summary>
    public void ShowDialogueText(string text, InteractableObject dialogueObject, float textSpeed = 0.05f)
    {
        if (dialogueObject != currentDialogueObject) return;

        // 停止正在进行的打字效果
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }

        if (textSpeed > 0 && !string.IsNullOrEmpty(text))
        {
            // 打字机效果
            typingCoroutine = StartCoroutine(TypeText(text, textSpeed));
        }
        else
        {
            // 立即显示
            dialogueText.text = text;
            isTyping = false;
            ShowContinuePrompt();
        }
    }

    /// <summary>
    /// 打字机效果协程
    /// </summary>
    IEnumerator TypeText(string text, float textSpeed)
    {
        isTyping = true;
        dialogueText.text = "";

        // 隐藏继续提示直到打字完成
        HideContinuePrompt();

        // 逐字显示
        for (int i = 0; i < text.Length; i++)
        {
            if (!isTyping) break;

            dialogueText.text += text[i];
            yield return new WaitForSeconds(textSpeed);
        }

        // 确保完整显示
        dialogueText.text = text;
        isTyping = false;

        // 显示继续提示
        ShowContinuePrompt();
    }

    /// <summary>
    /// 更新对话文本（外部调用）
    /// </summary>
    public void UpdateDialogueText(string text, InteractableObject dialogueObject)
    {
        if (dialogueObject != currentDialogueObject) return;

        dialogueText.text = text;
    }

    /// <summary>
    /// 显示继续提示
    /// </summary>
    void ShowContinuePrompt()
    {
        if (continuePromptText != null)
        {
            continuePromptText.enabled = true;
        }
    }

    /// <summary>
    /// 隐藏继续提示
    /// </summary>
    void HideContinuePrompt()
    {
        if (continuePromptText != null)
        {
            continuePromptText.enabled = false;
        }
    }

    /// <summary>
    /// 更新继续提示闪烁效果
    /// </summary>
    void UpdateContinuePromptBlink()
    {
        float alpha = 0.5f + Mathf.Sin(Time.time * promptBlinkSpeed) * 0.5f;
        Color color = continuePromptText.color;
        color.a = alpha;
        continuePromptText.color = color;
    }

    /// <summary>
    /// 结束对话
    /// </summary>
    public void EndDialogue()
    {
        if (!isShowingDialogue) return;

        // 停止打字效果
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }

        // 停止淡入淡出效果
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
        }

        // 隐藏对话面板
        ShowDialoguePanel(false);

        // 重置状态
        currentDialogueObject = null;
        isShowingDialogue = false;
        isTyping = false;

        Debug.Log("对话结束");
    }

    /// <summary>
    /// 显示/隐藏对话面板
    /// </summary>
    void ShowDialoguePanel(bool show)
    {
        if (dialoguePanel == null) return;

        float targetAlpha = show ? 1f : 0f;
        dialoguePanel.interactable = show;
        dialoguePanel.blocksRaycasts = show;

        fadeCoroutine = StartCoroutine(FadeCanvasGroup(dialoguePanel, targetAlpha, fadeSpeed));

        if (!show)
        {
            dialogueText.text = "";
            HideContinuePrompt();
        }
    }

    /// <summary>
    /// 更新交互提示
    /// </summary>
    void UpdateInteractionPrompt()
    {
        if (interactionPrompt == null || interactionPromptText == null) return;

        // 获取探测框管理器
        InteractionProbe probe = FindObjectOfType<InteractionProbe>();
        if (probe == null || isShowingDialogue)
        {
            // 隐藏提示
            interactionPrompt.alpha = Mathf.MoveTowards(
                interactionPrompt.alpha, 0f, Time.deltaTime * fadeSpeed
            );
            return;
        }

        string prompt = probe.GetInteractionPrompt();
        bool shouldShow = !string.IsNullOrEmpty(prompt);

        if (shouldShow)
        {
            interactionPromptText.text = prompt;
        }

        interactionPrompt.alpha = Mathf.MoveTowards(
            interactionPrompt.alpha,
            shouldShow ? 1f : 0f,
            Time.deltaTime * fadeSpeed
        );
    }

    /// <summary>
    /// 立即完成当前打字效果
    /// </summary>
    public void CompleteCurrentTyping()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }

        if (currentDialogueObject != null && isTyping)
        {
            dialogueText.text = currentDialogueObject.GetCurrentDialogueText();
            isTyping = false;
            ShowContinuePrompt();
            Debug.Log("打字效果已跳过");
        }
    }

    /// <summary>
    /// 强制显示交互提示
    /// </summary>
    public void ShowInteractionPrompt(string text)
    {
        if (interactionPrompt != null && interactionPromptText != null)
        {
            interactionPromptText.text = text;
            interactionPrompt.alpha = 1f;
        }
    }

    /// <summary>
    /// 隐藏交互提示
    /// </summary>
    public void HideInteractionPrompt()
    {
        if (interactionPrompt != null)
        {
            interactionPrompt.alpha = 0f;
        }
    }

    /// <summary>
    /// 淡入淡出效果
    /// </summary>
    IEnumerator FadeCanvasGroup(CanvasGroup group, float targetAlpha, float speed)
    {
        while (!Mathf.Approximately(group.alpha, targetAlpha))
        {
            group.alpha = Mathf.MoveTowards(group.alpha, targetAlpha, Time.deltaTime * speed);
            yield return null;
        }
    }

    // === 公开方法 ===

    /// <summary>
    /// 获取当前对话对象
    /// </summary>
    public InteractableObject GetCurrentDialogueObject()
    {
        return currentDialogueObject;
    }

    /// <summary>
    /// 是否正在显示对话
    /// </summary>
    public bool IsShowingDialogue()
    {
        return isShowingDialogue;
    }

    /// <summary>
    /// 是否正在打字
    /// </summary>
    public bool IsTyping()
    {
        return isTyping;
    }

    /// <summary>
    /// 设置对话字体
    /// </summary>
    public void SetDialogueFont(Font font)
    {
        dialogueFont = font;
        if (dialogueText != null)
        {
            dialogueText.font = font;
        }
    }
}