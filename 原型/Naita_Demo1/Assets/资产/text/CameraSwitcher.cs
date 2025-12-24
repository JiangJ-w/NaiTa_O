using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraSwitcher : MonoBehaviour
{
    [Header("Camera References")]
    public Camera camera3D;
    public Camera camera2D;
    [Tooltip("Canvas引用（用于控制UI显示）")]
    public Canvas canvas;

    [Header("Key Settings")]
    public KeyCode switchKey = KeyCode.E;

    [Header("Transition Settings")]
    [Tooltip("过渡时间（秒）")]
    [Range(0.1f, 3f)]
    public float transitionDuration = 0.5f;
    [Tooltip("过渡曲线类型")]
    public AnimationCurve transitionCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    private bool is2DMode = false;
    private bool isTransitioning = false;

    void Start()
    {
        // 初始化：设置为3D相机视角
        camera3D.gameObject.SetActive(true);
        camera2D.gameObject.SetActive(false);
        is2DMode = false;
        
        // 如果没有找到Canvas，尝试在场景中查找
        if (canvas == null)
        {
            canvas = FindObjectOfType<Canvas>();
        }
        
        // 如果没有设置曲线，使用默认的平滑曲线
        if (transitionCurve == null || transitionCurve.keys.Length == 0)
        {
            transitionCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(switchKey) && !isTransitioning)
        {
            if (is2DMode)
                StartCoroutine(SmoothSwitchTo3D());
            else
                StartCoroutine(SmoothSwitchTo2D());
        }
    }

    IEnumerator SmoothSwitchTo3D()
    {
        isTransitioning = true;
        
        // 回到 3D
        HideCanvasUI();
        camera3D.gameObject.SetActive(true);
        
        // 保存起始和目标相机的属性
        Camera from = camera2D;
        Camera to = camera3D;
        
        Vector3 startPos = from.transform.position;
        Vector3 endPos = to.transform.position;
        Quaternion startRot = from.transform.rotation;
        Quaternion endRot = to.transform.rotation;
        float startFOV = from.fieldOfView;
        float endFOV = to.fieldOfView;
        float startOrthoSize = from.orthographicSize;
        float endOrthoSize = to.orthographicSize;
        bool startOrtho = from.orthographic;
        bool endOrtho = to.orthographic;
        
        // 激活目标相机，让它从起始位置开始
        to.transform.position = startPos;
        to.transform.rotation = startRot;
        to.fieldOfView = startFOV;
        to.orthographicSize = startOrthoSize;
        to.orthographic = startOrtho;
        
        // 两个相机都激活，通过深度控制显示
        from.gameObject.SetActive(true);
        to.gameObject.SetActive(true);
        to.depth = from.depth + 1f; // 目标相机在前
        
        float elapsed = 0f;
        
        // 平滑过渡到3D相机位置
        while (elapsed < transitionDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / transitionDuration);
            float easedT = transitionCurve.Evaluate(t);
            
            // 平滑插值位置
            to.transform.position = Vector3.Lerp(startPos, endPos, easedT);
            
            // 平滑插值旋转
            to.transform.rotation = Quaternion.Slerp(startRot, endRot, easedT);
            
            // 平滑插值 FOV
            to.fieldOfView = Mathf.Lerp(startFOV, endFOV, easedT);
            
            // 平滑插值正交大小
            to.orthographicSize = Mathf.Lerp(startOrthoSize, endOrthoSize, easedT);
            
            // 在中间点切换正交/透视模式
            to.orthographic = easedT < 0.5f ? startOrtho : endOrtho;
            
            yield return null;
        }
        
        // 确保最终状态完全匹配目标相机
        to.transform.position = endPos;
        to.transform.rotation = endRot;
        to.fieldOfView = endFOV;
        to.orthographicSize = endOrthoSize;
        to.orthographic = endOrtho;
        
        // 关闭源相机
        from.gameObject.SetActive(false);
        
        is2DMode = false;
        isTransitioning = false;
    }

    IEnumerator SmoothSwitchTo2D()
    {
        isTransitioning = true;
        
        // 保存起始和目标相机的属性
        Camera from = camera3D;
        Camera to = camera2D;
        
        Vector3 startPos = from.transform.position;
        Vector3 endPos = to.transform.position;
        Quaternion startRot = from.transform.rotation;
        Quaternion endRot = to.transform.rotation;
        float startFOV = from.fieldOfView;
        float endFOV = to.fieldOfView;
        float startOrthoSize = from.orthographicSize;
        float endOrthoSize = to.orthographicSize;
        bool startOrtho = from.orthographic;
        bool endOrtho = to.orthographic;
        
        // 激活目标相机，让它从起始位置开始
        to.transform.position = startPos;
        to.transform.rotation = startRot;
        to.fieldOfView = startFOV;
        to.orthographicSize = startOrthoSize;
        to.orthographic = startOrtho;
        
        // 两个相机都激活，通过深度控制显示
        from.gameObject.SetActive(true);
        to.gameObject.SetActive(true);
        to.depth = from.depth + 1f; // 目标相机在前
        
        float elapsed = 0f;
        
        // 平滑过渡到2D相机位置
        while (elapsed < transitionDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / transitionDuration);
            float easedT = transitionCurve.Evaluate(t);
            
            // 平滑插值位置
            to.transform.position = Vector3.Lerp(startPos, endPos, easedT);
            
            // 平滑插值旋转
            to.transform.rotation = Quaternion.Slerp(startRot, endRot, easedT);
            
            // 平滑插值 FOV
            to.fieldOfView = Mathf.Lerp(startFOV, endFOV, easedT);
            
            // 平滑插值正交大小
            to.orthographicSize = Mathf.Lerp(startOrthoSize, endOrthoSize, easedT);
            
            // 在中间点切换正交/透视模式
            to.orthographic = easedT < 0.5f ? startOrtho : endOrtho;
            
            yield return null;
        }
        
        // 确保最终状态完全匹配目标相机
        to.transform.position = endPos;
        to.transform.rotation = endRot;
        to.fieldOfView = endFOV;
        to.orthographicSize = endOrthoSize;
        to.orthographic = endOrtho;
        
        // 关闭源相机
        from.gameObject.SetActive(false);
        
        // 进入 2D 模式（转场结束后）
        camera2D.gameObject.SetActive(true);
        ShowCanvasUI();
        
        is2DMode = true;
        isTransitioning = false;
    }
    
    void ShowCanvasUI()
    {
        if (canvas != null)
        {
            CanvasGroup canvasGroup = canvas.GetComponent<CanvasGroup>();
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 1f;
                canvasGroup.interactable = true;
                canvasGroup.blocksRaycasts = true;
            }
            else
            {
                canvas.enabled = true;
            }
        }
    }
    
    void HideCanvasUI()
    {
        if (canvas != null)
        {
            CanvasGroup canvasGroup = canvas.GetComponent<CanvasGroup>();
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0f;
                canvasGroup.interactable = false;
                canvasGroup.blocksRaycasts = false;
            }
            else
            {
                canvas.enabled = false;
            }
        }
    }
}

