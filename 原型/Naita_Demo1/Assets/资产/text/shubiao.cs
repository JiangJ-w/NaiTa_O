using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class shubiao : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("是否跟随鼠标")]
    public bool followMouse = true;
    [Tooltip("偏移量（相对于鼠标位置）")]
    public Vector2 offset = Vector2.zero;
    [Tooltip("是否将图片中心对齐鼠标（推荐开启）")]
    public bool centerOnMouse = true;
    
    [Header("Camera Settings")]
    [Tooltip("Camera2d引用（鼠标只在此相机激活时显示）")]
    public Camera camera2d;
    
    [Header("Boundary Settings")]
    [Tooltip("边界图片（鼠标只能在这个图片范围内显示）")]
    public RectTransform boundaryImage;
    [Tooltip("边界边距（鼠标距离边界的距离）")]
    public Vector2 boundaryMargin = Vector2.zero;
    
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private bool wasCamera2dActive = false;
    
    void Start()
    {
        // 获取RectTransform组件
        rectTransform = GetComponent<RectTransform>();
        if (rectTransform == null)
        {
            Debug.LogWarning("shubiao: 未找到RectTransform组件！此脚本需要用于UI元素。");
            enabled = false;
            return;
        }
        
        // 获取或创建CanvasGroup组件（用于控制显示/隐藏）
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
        
        // 关键设置：关闭Image的Raycast Target，避免阻挡Button点击
        UnityEngine.UI.Image image = GetComponent<UnityEngine.UI.Image>();
        if (image != null)
        {
            image.raycastTarget = false;
        }
        
        // 确保CanvasGroup不阻挡射线
        canvasGroup.blocksRaycasts = false;
        
        // 确保鼠标图片在Canvas最上层
        SetMouseOnTop();
        
        // 如果没有指定camera2d，尝试查找
        if (camera2d == null)
        {
            // 尝试通过名称查找
            GameObject camera2dObj = GameObject.Find("Camera2d");
            if (camera2dObj != null)
            {
                camera2d = camera2dObj.GetComponent<Camera>();
            }
            
            // 如果还没找到，尝试查找所有Camera
            if (camera2d == null)
            {
                Camera[] cameras = FindObjectsOfType<Camera>();
                foreach (Camera cam in cameras)
                {
                    if (cam.name.Contains("2d") || cam.name.Contains("2D"))
                    {
                        camera2d = cam;
                        break;
                    }
                }
            }
        }
        
        // 初始化显示状态
        if (camera2d != null)
        {
            wasCamera2dActive = camera2d.gameObject.activeSelf && camera2d.enabled;
            SetMouseVisible(wasCamera2dActive);
        }
        else
        {
            // 如果没有找到camera2d，默认隐藏
            SetMouseVisible(false);
        }
        
        // 初始化系统鼠标状态：默认隐藏
        UpdateSystemCursor(false);
    }
    
    void Update()
    {
        if (rectTransform == null) return;
        
        // 检测camera2d的激活状态
        bool isCamera2dActive = false;
        if (camera2d != null)
        {
            isCamera2dActive = camera2d.gameObject.activeSelf && camera2d.enabled;
        }
        
        // 如果状态改变，更新显示
        if (isCamera2dActive != wasCamera2dActive)
        {
            SetMouseVisible(isCamera2dActive);
            UpdateSystemCursor(isCamera2dActive);
            wasCamera2dActive = isCamera2dActive;
        }
        
        // 持续更新系统鼠标状态（防止被其他脚本修改）
        UpdateSystemCursor(isCamera2dActive);
        
        // 确保鼠标图片始终在Canvas最上层
        if (isCamera2dActive)
        {
            SetMouseOnTop();
        }
        
        // 只在camera2d激活时跟随鼠标
        if (!followMouse || !isCamera2dActive) return;

        // 获取鼠标屏幕坐标
        Vector2 mousePosition = Input.mousePosition;

        // 转换为UI坐标（适用于Overlay Canvas）
        RectTransform parentRect = rectTransform.parent as RectTransform;
        if (parentRect == null) return;

        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            parentRect,
            mousePosition,
            null,   // Overlay Canvas 必须是 null
            out localPoint
        );

        // 根据图片大小换算鼠标位置（中心对齐）
        if (centerOnMouse)
        {
            Vector2 size = rectTransform.rect.size;
            Vector2 pivot = rectTransform.pivot;
            localPoint -= new Vector2(
                size.x * (pivot.x - 0.5f),
                size.y * (pivot.y - 0.5f)
            );
        }

        // 应用偏移量
        localPoint += offset;

        // 限制在边界图片范围内
        if (boundaryImage != null)
        {
            localPoint = ClampToBoundary(localPoint);
        }

        // 更新图片位置（作为鼠标皮肤）
        rectTransform.anchoredPosition = localPoint;
    }
    
    Vector2 ClampToBoundary(Vector2 position)
    {
        if (boundaryImage == null) return position;
        
        // 获取鼠标图片的父级（用于坐标转换）
        RectTransform mouseParent = rectTransform.parent as RectTransform;
        
        // 使用GetWorldCorners获取边界图片的实际世界坐标边界（自动考虑scale）
        Vector3[] boundaryCorners = new Vector3[4];
        boundaryImage.GetWorldCorners(boundaryCorners);
        
        // 将世界坐标转换为鼠标图片父级的本地坐标
        Vector2 boundaryMin, boundaryMax;
        if (mouseParent != null)
        {
            boundaryMin = mouseParent.InverseTransformPoint(boundaryCorners[0]); // 左下角
            boundaryMax = mouseParent.InverseTransformPoint(boundaryCorners[2]); // 右上角
        }
        else
        {
            boundaryMin = boundaryCorners[0];
            boundaryMax = boundaryCorners[2];
        }
        
        // 获取鼠标图片的实际大小（考虑scale）
        Vector2 mouseSize = rectTransform.rect.size;
        Vector3 mouseScale = rectTransform.localScale;
        Vector2 actualMouseSize = new Vector2(mouseSize.x * mouseScale.x, mouseSize.y * mouseScale.y);
        
        // 计算鼠标图片的边界偏移（考虑中心对齐）
        float mouseOffsetX = centerOnMouse ? actualMouseSize.x * 0.5f : actualMouseSize.x * rectTransform.pivot.x;
        float mouseOffsetY = centerOnMouse ? actualMouseSize.y * 0.5f : actualMouseSize.y * rectTransform.pivot.y;
        float mouseOffsetMaxX = centerOnMouse ? actualMouseSize.x * 0.5f : actualMouseSize.x * (1f - rectTransform.pivot.x);
        float mouseOffsetMaxY = centerOnMouse ? actualMouseSize.y * 0.5f : actualMouseSize.y * (1f - rectTransform.pivot.y);
        
        // 应用边界边距和鼠标图片偏移
        float minX = boundaryMin.x + boundaryMargin.x + mouseOffsetX;
        float maxX = boundaryMax.x - boundaryMargin.x - mouseOffsetMaxX;
        float minY = boundaryMin.y + boundaryMargin.y + mouseOffsetY;
        float maxY = boundaryMax.y - boundaryMargin.y - mouseOffsetMaxY;
        
        // 限制位置在边界内
        position.x = Mathf.Clamp(position.x, minX, maxX);
        position.y = Mathf.Clamp(position.y, minY, maxY);
        
        return position;
    }
    
    void SetMouseVisible(bool visible)
    {
        if (canvasGroup != null)
        {
            canvasGroup.alpha = visible ? 1f : 0f;
            canvasGroup.interactable = false;  // 不交互
            canvasGroup.blocksRaycasts = false;  // 关键：不阻挡射线，让Button可以正常点击
        }
        else if (rectTransform != null)
        {
            rectTransform.gameObject.SetActive(visible);
        }
    }
    
    void UpdateSystemCursor(bool visible)
    {
        // 核心设置：系统鼠标始终隐藏，但保持可点击状态
        // visible = true 表示camera2d激活，此时显示图片鼠标皮肤
        // visible = false 表示camera2d未激活，此时隐藏图片鼠标皮肤
        Cursor.visible = false;  // 系统鼠标始终隐藏
        Cursor.lockState = CursorLockMode.None;  // 不锁定，保持可点击
    }
    
    void SetMouseOnTop()
    {
        if (rectTransform == null) return;
        
        // 方法1：将鼠标图片移动到父级的最后（最后渲染，显示在最上层）
        rectTransform.SetAsLastSibling();
        
        // 方法2：确保Canvas有最高的sortingOrder
        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas != null)
        {
            canvas.sortingOrder = 9999; // 设置一个很高的值，确保在最上层
        }
        
        // 方法3：如果鼠标图片本身有Canvas，也设置高sortingOrder
        Canvas selfCanvas = GetComponent<Canvas>();
        if (selfCanvas != null)
        {
            selfCanvas.sortingOrder = 9999;
        }
    }
}
