using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class WorkDocumentController : MonoBehaviour
{
    [System.Serializable]
    public class ClickableArea
    {
        public string areaName;
        public Rect area; // x,y,width,height (0-1范围)
        public string thoughtText;
    }
    
    [Header("UI引用")]
    public RawImage documentImage;
    public Texture2D workDocumentTexture;
    public Button backButton;
    public GameObject thoughtBubblePrefab;
    
    [Header("交互区域")]
    public ClickableArea[] clickableAreas;
    
    private RectTransform imageRectTransform;
    
    void Start()
    {
        // 设置文档图片
        if (documentImage != null && workDocumentTexture != null)
        {
            documentImage.texture = workDocumentTexture;
            imageRectTransform = documentImage.GetComponent<RectTransform>();
        }
        
        // 绑定返回按钮
        if (backButton != null)
            backButton.onClick.AddListener(() => Destroy(gameObject));
    }
    
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            HandleClick();
        }
    }
    
    void HandleClick()
    {
        if (imageRectTransform == null || thoughtBubblePrefab == null) return;
        
        // 获取鼠标在图片上的位置
        Vector2 localPoint;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            imageRectTransform,
            Input.mousePosition,
            null,
            out localPoint))
        {
            if (imageRectTransform.rect.Contains(localPoint))
            {
                // 转换为UV坐标(0-1)
                Vector2 uv = Rect.PointToNormalized(imageRectTransform.rect, localPoint);
                
                // 检查点击了哪个区域
                foreach (var area in clickableAreas)
                {
                    if (area.area.Contains(uv))
                    {
                        // 显示想法气泡
                        ShowThoughtBubble(localPoint, area.thoughtText);
                        break;
                    }
                }
            }
        }
    }
    
    void ShowThoughtBubble(Vector2 position, string thought)
    {
        GameObject bubble = Instantiate(thoughtBubblePrefab, transform);
        RectTransform rt = bubble.GetComponent<RectTransform>();
        
        // 设置气泡位置
        rt.anchoredPosition = position;
        
        // 设置气泡文本
        TMP_Text text = bubble.GetComponentInChildren<TMP_Text>();
        if (text != null) text.text = thought;
    }
}