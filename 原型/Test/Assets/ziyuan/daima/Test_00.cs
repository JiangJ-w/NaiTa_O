using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test_00 : MonoBehaviour
{
    [Header("鼠标灵敏度")]
    public float mouseSensitivity = 2.0f;
    
    [Header("垂直旋转限制")]
    public float minVerticalAngle = -80f;
    public float maxVerticalAngle = 80f;
    
    [Header("交互设置")]
    public float interactionDistance = 5f; // 交互距离
    public LayerMask interactionLayer = -1; // 可交互物体的层级
    
    [Header("相机设置")]
    public Camera mainCamera; // 主相机
    public Camera cameraUI; // UI相机
    public string computerTag = "Computer"; // Computer物体的标签
    public float cameraTransitionSpeed = 2f; // 相机切换速度（平滑度）
    public float transitionDuration = 1f; // 过渡持续时间（秒）
    
    private float verticalRotation = 0f; // 垂直旋转角度
    private Camera cam; // 当前使用的摄像机组件
    private bool isUICameraMode = false; // 当前是否为UI相机模式
    private bool isTransitioning = false; // 是否正在切换相机
    private Vector3 mainCameraPosition; // 主相机原始位置
    private Quaternion mainCameraRotation; // 主相机原始旋转
    private Vector3 uiCameraPosition; // UI相机位置
    private Quaternion uiCameraRotation; // UI相机旋转
    
    void Start()
    {
        // 初始化相机
        if (mainCamera == null)
        {
            mainCamera = GetComponent<Camera>();
            if (mainCamera == null)
            {
                mainCamera = Camera.main;
            }
        }
        
        // 确保UI相机存在
        if (cameraUI == null)
        {
            Debug.LogWarning("cameraUI未设置！请在Inspector中分配UI相机。");
        }
        
        // 设置当前相机为主相机
        cam = mainCamera;
        
        // 保存主相机初始状态
        if (mainCamera != null)
        {
            mainCameraPosition = mainCamera.transform.position;
            mainCameraRotation = mainCamera.transform.rotation;
        }
        
        // 保存UI相机状态
        if (cameraUI != null)
        {
            uiCameraPosition = cameraUI.transform.position;
            uiCameraRotation = cameraUI.transform.rotation;
        }
        
        // 初始化相机状态
        if (mainCamera != null) mainCamera.enabled = true;
        if (cameraUI != null) cameraUI.enabled = false;
        
        // 锁定鼠标到屏幕中心
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    
    void Update()
    {
        // 处理U键切换相机
        if (Input.GetKeyDown(KeyCode.U))
        {
            ToggleCamera();
        }
        
        // 只在主相机模式下且不在切换过程中时处理鼠标旋转
        if (!isUICameraMode && !isTransitioning)
        {
            HandleMouseLook();
        }
        
        // 处理鼠标点击交互
        HandleInteraction();
        
        // 按ESC键解锁鼠标（可选）
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        
        // 点击鼠标左键重新锁定鼠标
        if (Input.GetMouseButtonDown(0) && Cursor.lockState == CursorLockMode.None)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
    
    void HandleMouseLook()
    {
        // 获取鼠标移动增量
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;
        
        // 水平旋转（绕Y轴）
        transform.Rotate(Vector3.up * mouseX);
        
        // 垂直旋转（绕X轴，限制角度）
        verticalRotation -= mouseY;
        verticalRotation = Mathf.Clamp(verticalRotation, minVerticalAngle, maxVerticalAngle);
        
        // 应用垂直旋转
        transform.localRotation = Quaternion.Euler(verticalRotation, transform.localEulerAngles.y, 0f);
    }
    
    void HandleInteraction()
    {
        // 检测鼠标左键点击
        if (Input.GetMouseButtonDown(0))
        {
            // 从摄像机中心发射射线
            Ray ray = cam.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
            RaycastHit hit;
            
            // 检测射线是否击中物体
            if (Physics.Raycast(ray, out hit, interactionDistance, interactionLayer))
            {
                // 检查是否点击了Computer物体
                if (hit.collider.CompareTag(computerTag) || 
                    hit.collider.name.ToLower().Contains("computer"))
                {
                    // 点击电脑时，如果当前是主相机模式，切换到UI相机
                    // 如果当前是UI相机模式，也可以切换回主相机（可选，或者只允许切换到UI）
                    if (!isUICameraMode && !isTransitioning)
                    {
                        StartCoroutine(SmoothSwitchToUICamera());
                        Debug.Log("点击了Computer，切换到UI相机");
                    }
                }
                else
                {
                    // 检查击中的物体是否有交互组件
                    IInteractable interactable = hit.collider.GetComponent<IInteractable>();
                    if (interactable != null)
                    {
                        interactable.OnInteract();
                    }
                    else
                    {
                        // 如果没有IInteractable接口，尝试调用OnMouseDown方法（Unity内置）
                        hit.collider.SendMessage("OnInteract", SendMessageOptions.DontRequireReceiver);
                    }
                }
                
                // 调试信息（可选）
                Debug.Log("点击了物体: " + hit.collider.name);
            }
        }
    }
    
    /// <summary>
    /// 切换相机模式
    /// </summary>
    void ToggleCamera()
    {
        if (isTransitioning) return; // 如果正在切换，忽略新的切换请求
        
        if (isUICameraMode)
        {
            StartCoroutine(SmoothSwitchToMainCamera());
        }
        else
        {
            StartCoroutine(SmoothSwitchToUICamera());
        }
    }
    
    /// <summary>
    /// 平滑切换到UI相机
    /// </summary>
    IEnumerator SmoothSwitchToUICamera()
    {
        if (cameraUI == null)
        {
            Debug.LogWarning("cameraUI未设置，无法切换！");
            yield break;
        }
        
        if (mainCamera == null)
        {
            Debug.LogWarning("mainCamera未设置，无法切换！");
            yield break;
        }
        
        isTransitioning = true;
        isUICameraMode = true;
        
        // 获取起始和结束状态
        Vector3 startPos = mainCamera.transform.position;
        Quaternion startRot = mainCamera.transform.rotation;
        Vector3 endPos = uiCameraPosition;
        Quaternion endRot = uiCameraRotation;
        
        // 设置UI相机到起始位置（与主相机同步）
        cameraUI.transform.position = startPos;
        cameraUI.transform.rotation = startRot;
        cameraUI.enabled = true;
        
        // 复制主相机的FOV和其他设置到UI相机
        cameraUI.fieldOfView = mainCamera.fieldOfView;
        
        float elapsedTime = 0f;
        
        // 平滑过渡：两个相机同步移动，然后切换
        while (elapsedTime < transitionDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsedTime / transitionDuration);
            
            // 同步移动两个相机
            Vector3 currentPos = Vector3.Lerp(startPos, endPos, t);
            Quaternion currentRot = Quaternion.Slerp(startRot, endRot, t);
            
            mainCamera.transform.position = currentPos;
            mainCamera.transform.rotation = currentRot;
            cameraUI.transform.position = currentPos;
            cameraUI.transform.rotation = currentRot;
            
            yield return null;
        }
        
        // 确保最终位置准确
        cameraUI.transform.position = endPos;
        cameraUI.transform.rotation = endRot;
        
        // 切换相机：禁用主相机，启用UI相机
        mainCamera.enabled = false;
        cameraUI.enabled = true;
        cam = cameraUI;
        
        isTransitioning = false;
        Debug.Log("切换到UI相机模式");
    }
    
    /// <summary>
    /// 平滑切换到主相机
    /// </summary>
    IEnumerator SmoothSwitchToMainCamera()
    {
        if (mainCamera == null)
        {
            Debug.LogWarning("mainCamera未设置，无法切换！");
            yield break;
        }
        
        if (cameraUI == null)
        {
            Debug.LogWarning("cameraUI未设置，无法切换！");
            yield break;
        }
        
        isTransitioning = true;
        isUICameraMode = false;
        
        // 获取起始和结束状态
        Vector3 startPos = cameraUI.transform.position;
        Quaternion startRot = cameraUI.transform.rotation;
        Vector3 endPos = mainCameraPosition;
        Quaternion endRot = mainCameraRotation;
        
        // 设置主相机到起始位置（与UI相机同步）
        mainCamera.transform.position = startPos;
        mainCamera.transform.rotation = startRot;
        mainCamera.enabled = true;
        
        // 复制UI相机的FOV和其他设置到主相机
        mainCamera.fieldOfView = cameraUI.fieldOfView;
        
        float elapsedTime = 0f;
        
        // 平滑过渡：两个相机同步移动，然后切换
        while (elapsedTime < transitionDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsedTime / transitionDuration);
            
            // 同步移动两个相机
            Vector3 currentPos = Vector3.Lerp(startPos, endPos, t);
            Quaternion currentRot = Quaternion.Slerp(startRot, endRot, t);
            
            cameraUI.transform.position = currentPos;
            cameraUI.transform.rotation = currentRot;
            mainCamera.transform.position = currentPos;
            mainCamera.transform.rotation = currentRot;
            
            yield return null;
        }
        
        // 确保最终位置准确
        mainCamera.transform.position = endPos;
        mainCamera.transform.rotation = endRot;
        
        // 切换相机：禁用UI相机，启用主相机
        cameraUI.enabled = false;
        mainCamera.enabled = true;
        cam = mainCamera;
        
        isTransitioning = false;
        Debug.Log("切换到主相机模式");
    }
    
    void OnDisable()
    {
        // 脚本禁用时解锁鼠标
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}

// 交互接口（可选，用于更规范的交互系统）
public interface IInteractable
{
    void OnInteract();
}
