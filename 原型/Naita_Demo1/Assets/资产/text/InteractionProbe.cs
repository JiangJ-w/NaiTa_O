using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 摄像机探测框管理器 - 处理物体检测、高亮和输入
/// </summary>
public class InteractionProbe : MonoBehaviour
{
    [Header("=== 探测框设置 ===")]
    [Tooltip("探测框的尺寸（宽、高、深）")]
    [SerializeField] private Vector3 probeSize = new Vector3(2f, 1.5f, 1f);

    [Tooltip("探测框相对于摄像机的位置")]
    [SerializeField] private Vector3 probeOffset = new Vector3(0f, 0f, 1.5f);

    [Tooltip("调试时显示的颜色")]
    [SerializeField] private Color gizmoColor = new Color(0, 1, 0, 0.3f);

    [Header("=== 检测设置 ===")]
    [Tooltip("可交互物体的标签")]
    [SerializeField] private string interactableTag = "Interactable";

    [Tooltip("检测哪些层的物体")]
    [SerializeField] private LayerMask detectionLayer = -1;

    [Tooltip("检测频率（秒）。值越小响应越快，但性能消耗越大")]
    [Range(0.01f, 0.2f)]
    [SerializeField] private float updateInterval = 0.05f;

    [Header("=== 目标选择设置 ===")]
    [Tooltip("是否启用屏幕中心优先选择")]
    [SerializeField] private bool preferScreenCenter = true;

    [Tooltip("屏幕中心优先选择的权重（0-1）")]
    [Range(0f, 1f)]
    [SerializeField] private float centerWeight = 0.7f;

    // 私有变量
    private BoxCollider probeCollider;
    private Camera cam;
    private List<InteractableObject> detectedObjects = new List<InteractableObject>();
    private float lastUpdateTime;
    private InteractableObject currentTarget;

    void Start()
    {
        cam = GetComponent<Camera>();
        CreateProbeCollider();
        Debug.Log("InteractionProbe 初始化完成");
    }

    void Update()
    {
        // 更新探测框位置（跟随摄像机）
        UpdateProbePosition();

        // 限制检测频率以优化性能
        if (Time.time - lastUpdateTime >= updateInterval)
        {
            DetectObjectsInProbe();
            UpdateCurrentTarget();
            lastUpdateTime = Time.time;
        }

        // 处理玩家输入
        HandlePlayerInput();
    }

    /// <summary>
    /// 创建探测框的碰撞体
    /// </summary>
    void CreateProbeCollider()
    {
        // 创建子物体作为探测框
        GameObject probeObject = new GameObject("InteractionProbe_Collider");
        probeObject.transform.SetParent(transform);
        probeObject.transform.localPosition = probeOffset;
        probeObject.transform.localRotation = Quaternion.identity;

        // 添加BoxCollider作为触发器
        probeCollider = probeObject.AddComponent<BoxCollider>();
        probeCollider.size = probeSize;
        probeCollider.isTrigger = true;

        // 添加刚体确保触发器工作
        Rigidbody rb = probeObject.AddComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;

        // 隐藏这个物体（不在游戏视图显示）
        probeObject.hideFlags = HideFlags.HideInHierarchy;

        Debug.Log($"探测框创建完成，尺寸: {probeSize}, 位置偏移: {probeOffset}");
    }

    /// <summary>
    /// 更新探测框位置（跟随摄像机旋转和移动）
    /// </summary>
    void UpdateProbePosition()
    {
        if (probeCollider != null)
        {
            probeCollider.transform.localPosition = probeOffset;
        }
    }

    /// <summary>
    /// 检测探测框内的所有可交互物体
    /// </summary>
    void DetectObjectsInProbe()
    {
        // 清空之前的检测结果
        detectedObjects.Clear();

        // 计算探测框的世界位置和旋转
        Vector3 center = transform.position + transform.TransformDirection(probeOffset);
        Vector3 halfExtents = probeSize * 0.5f;
        Quaternion orientation = transform.rotation;

        // 使用OverlapBox检测框内的所有碰撞体
        Collider[] hitColliders = Physics.OverlapBox(center, halfExtents, orientation, detectionLayer);

        // 筛选出可交互物体
        foreach (Collider col in hitColliders)
        {
            if (col.CompareTag(interactableTag))
            {
                InteractableObject interactable = col.GetComponent<InteractableObject>();
                if (interactable != null && !detectedObjects.Contains(interactable))
                {
                    detectedObjects.Add(interactable);
                }
            }
        }
    }

    /// <summary>
    /// 更新当前目标（自动选择最佳交互目标）
    /// </summary>
    void UpdateCurrentTarget()
    {
        // 如果有物体正在对话，不切换目标
        if (IsAnyObjectInDialogue())
        {
            return;
        }

        // 如果没有检测到物体，清除当前目标
        if (detectedObjects.Count == 0)
        {
            SetCurrentTarget(null);
            return;
        }

        // 如果当前目标仍然在探测框内，保持它
        if (currentTarget != null && detectedObjects.Contains(currentTarget))
        {
            return;
        }

        // 选择最佳目标
        InteractableObject bestTarget = null;
        float bestScore = float.MinValue;

        foreach (InteractableObject obj in detectedObjects)
        {
            if (obj == null || obj.IsInDialogue()) continue;

            // 计算目标得分
            float score = CalculateTargetScore(obj);

            if (score > bestScore)
            {
                bestScore = score;
                bestTarget = obj;
            }
        }

        // 设置新目标
        SetCurrentTarget(bestTarget);
    }

    /// <summary>
    /// 计算目标的得分（用于自动选择最佳目标）
    /// </summary>
    float CalculateTargetScore(InteractableObject target)
    {
        if (target == null) return float.MinValue;

        float score = 0f;

        // 1. 屏幕中心距离得分（距离越近得分越高）
        if (preferScreenCenter && cam != null)
        {
            Vector3 screenPos = cam.WorldToScreenPoint(target.transform.position);
            Vector2 screenCenter = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
            Vector2 targetPos = new Vector2(screenPos.x, screenPos.y);

            // 计算与屏幕中心的距离（像素）
            float distance = Vector2.Distance(screenCenter, targetPos);

            // 归一化距离（0-1，1表示在屏幕中心）
            float normalizedDistance = 1f - Mathf.Clamp01(distance / (Screen.width * 0.5f));

            score += normalizedDistance * centerWeight;
        }

        // 2. 距离摄像机距离得分（越近得分越高）
        float distanceToCamera = Vector3.Distance(transform.position, target.transform.position);
        float normalizedCameraDistance = 1f - Mathf.Clamp01(distanceToCamera / 10f); // 假设10米为最大有效距离
        score += normalizedCameraDistance * (1f - centerWeight);

        return score;
    }

    /// <summary>
    /// 设置当前目标（处理高亮切换）
    /// </summary>
    void SetCurrentTarget(InteractableObject newTarget)
    {
        if (currentTarget == newTarget) return;

        // 移除旧目标的高亮
        if (currentTarget != null)
        {
            currentTarget.DisableHighlight();
        }

        // 设置新目标
        currentTarget = newTarget;

        // 为新目标添加高亮
        if (currentTarget != null)
        {
            currentTarget.EnableHighlight();
        }
    }

    /// <summary>
    /// 处理玩家输入（鼠标点击）
    /// </summary>
    void HandlePlayerInput()
    {
        // 鼠标左键点击
        if (Input.GetMouseButtonDown(0))
        {
            // 检查是否点击在UI上（避免UI阻挡3D交互）
            if (IsPointerOverUI())
            {
                Debug.Log("点击被UI阻挡，不处理3D交互");
                return;
            }

            // 查找当前正在对话的物体
            InteractableObject dialogueObject = FindObjectInDialogue();

            if (dialogueObject != null)
            {
                // 继续对话
                dialogueObject.NextDialogue();
                Debug.Log($"继续对话: {dialogueObject.name}");
            }
            else if (currentTarget != null)
            {
                // 开始新对话
                currentTarget.StartInteraction();
                Debug.Log($"开始交互: {currentTarget.name}");
            }
        }

        // ESC键跳过当前对话
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            InteractableObject dialogueObject = FindObjectInDialogue();
            if (dialogueObject != null)
            {
                dialogueObject.SkipDialogue();
                Debug.Log($"跳过对话: {dialogueObject.name}");
            }
        }

        // 调试快捷键：Tab键显示/隐藏探测框
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            ToggleProbeVisibility();
        }
    }

    /// <summary>
    /// 查找正在对话的物体
    /// </summary>
    InteractableObject FindObjectInDialogue()
    {
        // 先检查探测框内的物体
        foreach (InteractableObject obj in detectedObjects)
        {
            if (obj != null && obj.IsInDialogue())
            {
                return obj;
            }
        }

        // 如果探测框内没有，检查场景中所有物体
        InteractableObject[] allInteractables = FindObjectsOfType<InteractableObject>();
        foreach (InteractableObject obj in allInteractables)
        {
            if (obj.IsInDialogue())
            {
                return obj;
            }
        }

        return null;
    }

    /// <summary>
    /// 检查是否有物体正在对话
    /// </summary>
    bool IsAnyObjectInDialogue()
    {
        return FindObjectInDialogue() != null;
    }

    /// <summary>
    /// 检查鼠标是否在UI元素上
    /// </summary>
    bool IsPointerOverUI()
    {
        if (UnityEngine.EventSystems.EventSystem.current != null)
        {
            return UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject();
        }
        return false;
    }

    /// <summary>
    /// 切换探测框的可见性（调试用）
    /// </summary>
    void ToggleProbeVisibility()
    {
        if (probeCollider != null)
        {
            GameObject probeObj = probeCollider.gameObject;
            Renderer renderer = probeObj.GetComponent<Renderer>();

            if (renderer == null)
            {
                // 创建可视化材质
                renderer = probeObj.AddComponent<MeshRenderer>();
                MeshFilter filter = probeObj.AddComponent<MeshFilter>();
                filter.mesh = Resources.GetBuiltinResource<Mesh>("Cube.fbx");

                Material mat = new Material(Shader.Find("Standard"));
                mat.color = new Color(0, 1, 0, 0.3f);
                mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                mat.EnableKeyword("_ALPHABLEND_ON");
                renderer.material = mat;

                Debug.Log("探测框可视化已启用");
            }
            else
            {
                renderer.enabled = !renderer.enabled;
                Debug.Log($"探测框可视化: {(renderer.enabled ? "显示" : "隐藏")}");
            }
        }
    }

    // === 公开方法供其他脚本调用 ===

    /// <summary>
    /// 获取当前目标
    /// </summary>
    public InteractableObject GetCurrentTarget()
    {
        return currentTarget;
    }

    /// <summary>
    /// 获取交互提示文本
    /// </summary>
    public string GetInteractionPrompt()
    {
        if (IsAnyObjectInDialogue())
        {
            return "点击继续...";
        }
        else if (currentTarget != null)
        {
            return currentTarget.GetInteractionPrompt();
        }
        return "";
    }

    /// <summary>
    /// 获取探测框内物体数量
    /// </summary>
    public int GetDetectedObjectCount()
    {
        return detectedObjects.Count;
    }

    /// <summary>
    /// 手动设置探测框尺寸
    /// </summary>
    public void SetProbeSize(Vector3 newSize)
    {
        probeSize = newSize;
        if (probeCollider != null)
        {
            probeCollider.size = newSize;
        }
    }

    /// <summary>
    /// 手动设置探测框偏移
    /// </summary>
    public void SetProbeOffset(Vector3 newOffset)
    {
        probeOffset = newOffset;
    }

#if UNITY_EDITOR
    /// <summary>
    /// 在编辑器中绘制探测框（调试用）
    /// </summary>
    void OnDrawGizmosSelected()
    {
        if (!Application.isPlaying) return;

        Gizmos.color = gizmoColor;

        // 绘制探测框轮廓
        Vector3 center = transform.position + transform.TransformDirection(probeOffset);
        Gizmos.DrawWireCube(center, probeSize);

        // 绘制半透明填充
        Color fillColor = gizmoColor;
        fillColor.a = 0.1f;
        Gizmos.color = fillColor;
        Gizmos.DrawCube(center, probeSize);

        // 绘制当前目标
        if (currentTarget != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, currentTarget.transform.position);
            Gizmos.DrawWireSphere(currentTarget.transform.position, 0.3f);
        }

        // 绘制所有检测到的物体
        Gizmos.color = Color.yellow;
        foreach (InteractableObject obj in detectedObjects)
        {
            if (obj != null && obj != currentTarget)
            {
                Gizmos.DrawWireSphere(obj.transform.position, 0.2f);
            }
        }
    }
#endif
}