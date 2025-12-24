using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// HDRP鼠标控制摄像机 - 支持俯仰和偏航旋转，带有完整的转动限制
/// </summary>
public class HDRPMouseCameraController : MonoBehaviour
{
    [Header("鼠标灵敏度")]
    [SerializeField, Range(0.1f, 10f)] private float mouseSensitivity = 2f;
    [SerializeField] private bool invertY = false;

    [Header("俯仰角度限制")]
    [SerializeField] private bool limitPitch = true;
    [SerializeField, Range(-90f, 0f)] private float minPitchAngle = -80f;
    [SerializeField, Range(0f, 90f)] private float maxPitchAngle = 80f;

    [Header("偏航角度限制")]
    [SerializeField] private bool limitYaw = false;
    [SerializeField] private YawLimitMode yawLimitMode = YawLimitMode.Symmetrical;

    [Space(5)]
    [Tooltip("对称模式：左右对称限制")]
    [SerializeField, Range(0f, 180f)]
    private float maxYawAngle = 180f;

    [Space(5)]
    [Tooltip("非对称模式：分别设置左右限制")]
    [SerializeField, Range(-180f, 0f)]
    private float minYawAngle = -180f;

    [SerializeField, Range(0f, 180f)]
    private float maxYawAngleAsymmetrical = 180f;

    [Space(5)]
    [Tooltip("相对模式：相对于初始旋转的角度限制")]
    [SerializeField, Range(0f, 180f)]
    private float relativeYawLimit = 180f;

    [Header("平滑设置")]
    [SerializeField] private bool smoothRotation = true;
    [SerializeField, Range(0f, 0.3f)] private float smoothTime = 0.1f;

    [Header("输入设置")]
    [SerializeField] private bool lockCursor = true;
    [SerializeField] private KeyCode unlockCursorKey = KeyCode.Escape;

    [Header("初始旋转设置")]
    [SerializeField] private bool useInitialRotationAsCenter = true;
    [SerializeField] private Vector2 initialRotation = Vector2.zero;

    // 枚举定义
    public enum YawLimitMode
    {
        None,           // 无限制
        Symmetrical,    // 对称限制
        Asymmetrical,   // 非对称限制
        Relative        // 相对于初始旋转
    }

    // 私有变量
    private float currentYaw = 0f;
    private float currentPitch = 0f;
    private float targetYaw = 0f;
    private float targetPitch = 0f;
    private float yawVelocity = 0f;
    private float pitchVelocity = 0f;
    private float initialYaw = 0f;

    private Vector2 mouseDelta;
    private bool isCursorLocked = true;

    // Input System 支持
    private PlayerInput playerInput;
    private InputAction lookAction;

    void Start()
    {
        InitializeRotation();
        InitializeInputSystem();

        if (lockCursor)
        {
            LockCursor();
        }
    }

    void Update()
    {
        HandleCursorLock();

        if (isCursorLocked)
        {
            ProcessMouseInput();
            UpdateCameraRotation();
        }
    }

    /// <summary>
    /// 初始化摄像机当前朝向
    /// </summary>
    private void InitializeRotation()
    {
        Vector3 euler = transform.rotation.eulerAngles;

        if (useInitialRotationAsCenter)
        {
            // 使用初始旋转作为中心点
            currentYaw = targetYaw = initialRotation.y;
            currentPitch = targetPitch = initialRotation.x;
            initialYaw = currentYaw;

            // 立即应用初始旋转
            transform.rotation = Quaternion.Euler(currentPitch, currentYaw, 0f);
        }
        else
        {
            // 使用当前Transform的旋转
            currentYaw = targetYaw = euler.y;
            currentPitch = targetPitch = euler.x;
            initialYaw = currentYaw;
        }

        // 确保角度在-180到180范围内
        NormalizeAngles();
    }

    /// <summary>
    /// 规范化角度到-180到180范围
    /// </summary>
    private void NormalizeAngles()
    {
        currentPitch = NormalizeAngle(currentPitch);
        targetPitch = NormalizeAngle(targetPitch);
        currentYaw = NormalizeAngle(currentYaw);
        targetYaw = NormalizeAngle(targetYaw);
    }

    /// <summary>
    /// 规范化单个角度
    /// </summary>
    private float NormalizeAngle(float angle)
    {
        while (angle > 180f) angle -= 360f;
        while (angle < -180f) angle += 360f;
        return angle;
    }

    /// <summary>
    /// 初始化输入系统
    /// </summary>
    private void InitializeInputSystem()
    {
        playerInput = GetComponentInParent<PlayerInput>();

        if (playerInput != null)
        {
            lookAction = playerInput.actions["Look"];
        }
        else
        {
            Debug.Log("PlayerInput not found, using legacy Input system.");
        }
    }

    /// <summary>
    /// 处理鼠标输入
    /// </summary>
    private void ProcessMouseInput()
    {
        Vector2 inputDelta = Vector2.zero;

        if (lookAction != null)
        {
            inputDelta = lookAction.ReadValue<Vector2>();
        }
        else
        {
            inputDelta = new Vector2(
                Input.GetAxis("Mouse X"),
                Input.GetAxis("Mouse Y")
            );
        }

        // 应用灵敏度
        float yawInput = inputDelta.x * mouseSensitivity;
        float pitchInput = inputDelta.y * mouseSensitivity * (invertY ? 1f : -1f);

        // 更新目标旋转
        targetYaw += yawInput;
        targetPitch += pitchInput;

        // 应用旋转限制
        ApplyRotationLimits();
    }

    /// <summary>
    /// 应用旋转限制
    /// </summary>
    private void ApplyRotationLimits()
    {
        // 限制俯仰角度
        if (limitPitch)
        {
            targetPitch = Mathf.Clamp(targetPitch, minPitchAngle, maxPitchAngle);
        }
        else
        {
            targetPitch = NormalizeAngle(targetPitch);
        }

        // 限制偏航角度
        switch (yawLimitMode)
        {
            case YawLimitMode.Symmetrical:
                targetYaw = Mathf.Clamp(targetYaw, -maxYawAngle, maxYawAngle);
                break;

            case YawLimitMode.Asymmetrical:
                targetYaw = Mathf.Clamp(targetYaw, minYawAngle, maxYawAngleAsymmetrical);
                break;

            case YawLimitMode.Relative:
                float minRelativeYaw = initialYaw - relativeYawLimit;
                float maxRelativeYaw = initialYaw + relativeYawLimit;
                targetYaw = Mathf.Clamp(targetYaw, minRelativeYaw, maxRelativeYaw);
                break;

            case YawLimitMode.None:
                targetYaw = NormalizeAngle(targetYaw);
                break;
        }
    }

    /// <summary>
    /// 更新摄像机旋转
    /// </summary>
    private void UpdateCameraRotation()
    {
        if (smoothRotation)
        {
            // 平滑旋转
            currentYaw = Mathf.SmoothDampAngle(currentYaw, targetYaw, ref yawVelocity, smoothTime);
            currentPitch = Mathf.SmoothDampAngle(currentPitch, targetPitch, ref pitchVelocity, smoothTime);
        }
        else
        {
            // 立即旋转
            currentYaw = targetYaw;
            currentPitch = targetPitch;
        }

        // 应用旋转
        transform.rotation = Quaternion.Euler(currentPitch, currentYaw, 0f);
    }

    /// <summary>
    /// 处理光标锁定状态
    /// </summary>
    private void HandleCursorLock()
    {
        if (Input.GetKeyDown(unlockCursorKey))
        {
            ToggleCursorLock();
        }
    }

    /// <summary>
    /// 锁定光标
    /// </summary>
    private void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        isCursorLocked = true;
    }

    /// <summary>
    /// 解锁光标
    /// </summary>
    private void UnlockCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        isCursorLocked = false;
    }

    /// <summary>
    /// 切换光标锁定状态
    /// </summary>
    private void ToggleCursorLock()
    {
        if (isCursorLocked)
        {
            UnlockCursor();
        }
        else
        {
            LockCursor();
        }
    }

    /// <summary>
    /// 获取当前水平旋转限制的最小角度
    /// </summary>
    public float GetMinYawAngle()
    {
        switch (yawLimitMode)
        {
            case YawLimitMode.Symmetrical:
                return -maxYawAngle;
            case YawLimitMode.Asymmetrical:
                return minYawAngle;
            case YawLimitMode.Relative:
                return initialYaw - relativeYawLimit;
            default:
                return -180f;
        }
    }

    /// <summary>
    /// 获取当前水平旋转限制的最大角度
    /// </summary>
    public float GetMaxYawAngle()
    {
        switch (yawLimitMode)
        {
            case YawLimitMode.Symmetrical:
                return maxYawAngle;
            case YawLimitMode.Asymmetrical:
                return maxYawAngleAsymmetrical;
            case YawLimitMode.Relative:
                return initialYaw + relativeYawLimit;
            default:
                return 180f;
        }
    }

    /// <summary>
    /// 设置新的旋转角度
    /// </summary>
    public void SetRotation(float yaw, float pitch)
    {
        targetYaw = yaw;
        targetPitch = pitch;
        ApplyRotationLimits();

        if (!smoothRotation)
        {
            currentYaw = targetYaw;
            currentPitch = targetPitch;
            transform.rotation = Quaternion.Euler(currentPitch, currentYaw, 0f);
        }
    }

    /// <summary>
    /// 设置水平旋转限制模式
    /// </summary>
    public void SetYawLimitMode(YawLimitMode mode, params float[] limits)
    {
        yawLimitMode = mode;

        switch (mode)
        {
            case YawLimitMode.Symmetrical:
                if (limits.Length > 0) maxYawAngle = Mathf.Clamp(limits[0], 0f, 180f);
                break;
            case YawLimitMode.Asymmetrical:
                if (limits.Length > 0) minYawAngle = Mathf.Clamp(limits[0], -180f, 0f);
                if (limits.Length > 1) maxYawAngleAsymmetrical = Mathf.Clamp(limits[1], 0f, 180f);
                break;
            case YawLimitMode.Relative:
                if (limits.Length > 0) relativeYawLimit = Mathf.Clamp(limits[0], 0f, 180f);
                break;
        }

        ApplyRotationLimits();
    }

    /// <summary>
    /// 获取当前旋转角度
    /// </summary>
    public Vector2 GetRotation()
    {
        return new Vector2(currentYaw, currentPitch);
    }

    /// <summary>
    /// 设置鼠标灵敏度
    /// </summary>
    public void SetSensitivity(float sensitivity)
    {
        mouseSensitivity = Mathf.Clamp(sensitivity, 0.1f, 10f);
    }

    /// <summary>
    /// 重置旋转到初始值
    /// </summary>
    public void ResetRotation()
    {
        targetYaw = initialYaw;
        targetPitch = initialRotation.x;
        ApplyRotationLimits();
    }

#if UNITY_EDITOR
    /// <summary>
    /// 在Inspector中验证参数
    /// </summary>
    void OnValidate()
    {
        // 确保最小值不大于最大值
        if (minPitchAngle > maxPitchAngle)
        {
            float temp = minPitchAngle;
            minPitchAngle = maxPitchAngle;
            maxPitchAngle = temp;
        }

        if (minYawAngle > maxYawAngleAsymmetrical)
        {
            float temp = minYawAngle;
            minYawAngle = maxYawAngleAsymmetrical;
            maxYawAngleAsymmetrical = temp;
        }

        // 确保对称模式的限制为正数
        maxYawAngle = Mathf.Max(0f, maxYawAngle);
        relativeYawLimit = Mathf.Max(0f, relativeYawLimit);
    }

    /// <summary>
    /// 在编辑器中绘制角度限制的辅助线
    /// </summary>
    void OnDrawGizmosSelected()
    {
        if (!Application.isPlaying)
        {
            Vector3 position = transform.position;
            Vector3 forward = transform.forward;

            // 绘制当前朝向
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(position, position + forward * 2f);

            // 绘制俯仰限制
            if (limitPitch)
            {
                Gizmos.color = new Color(1f, 0.5f, 0f, 0.3f);
                Quaternion minRotation = Quaternion.Euler(minPitchAngle, transform.eulerAngles.y, 0f);
                Quaternion maxRotation = Quaternion.Euler(maxPitchAngle, transform.eulerAngles.y, 0f);

                Vector3 minDirection = minRotation * Vector3.forward;
                Vector3 maxDirection = maxRotation * Vector3.forward;

                Gizmos.DrawLine(position, position + minDirection * 3f);
                Gizmos.DrawLine(position, position + maxDirection * 3f);

                // 绘制弧形表示俯仰限制范围
                UnityEditor.Handles.color = new Color(1f, 0.5f, 0f, 0.1f);
                UnityEditor.Handles.DrawSolidArc(
                    position,
                    transform.right,
                    minDirection,
                    maxPitchAngle - minPitchAngle,
                    3f
                );
            }

            // 绘制水平限制
            if (limitYaw && yawLimitMode != YawLimitMode.None)
            {
                float minYaw = 0f;
                float maxYaw = 0f;

                switch (yawLimitMode)
                {
                    case YawLimitMode.Symmetrical:
                        minYaw = -maxYawAngle;
                        maxYaw = maxYawAngle;
                        break;
                    case YawLimitMode.Asymmetrical:
                        minYaw = minYawAngle;
                        maxYaw = maxYawAngleAsymmetrical;
                        break;
                    case YawLimitMode.Relative:
                        float currentYaw = transform.eulerAngles.y;
                        if (currentYaw > 180f) currentYaw -= 360f;
                        minYaw = currentYaw - relativeYawLimit;
                        maxYaw = currentYaw + relativeYawLimit;
                        break;
                }

                // 绘制水平限制线
                Gizmos.color = new Color(0f, 1f, 0.5f, 0.3f);
                Quaternion minYawRotation = Quaternion.Euler(0f, minYaw, 0f);
                Quaternion maxYawRotation = Quaternion.Euler(0f, maxYaw, 0f);

                Vector3 minYawDirection = minYawRotation * Vector3.forward;
                Vector3 maxYawDirection = maxYawRotation * Vector3.forward;

                Gizmos.DrawLine(position, position + minYawDirection * 3f);
                Gizmos.DrawLine(position, position + maxYawDirection * 3f);

                // 绘制弧形表示水平限制范围
                UnityEditor.Handles.color = new Color(0f, 1f, 0.5f, 0.1f);
                UnityEditor.Handles.DrawSolidArc(
                    position,
                    Vector3.up,
                    minYawDirection,
                    maxYaw - minYaw,
                    3f
                );
            }
        }
    }
#endif
}