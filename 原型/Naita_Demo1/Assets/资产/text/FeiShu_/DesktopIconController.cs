using UnityEngine;

public class DesktopIconController : MonoBehaviour
{
    [Header("设置")]
    public GameObject appPrefab;          // 拖拽App_WorkSoftware预制体到这里
    public Vector3 spawnOffset = new Vector3(0, 0, 0.1f); // 应用打开位置偏移
    
    void OnMouseDown()
    {
        OpenApp();
    }
    
    void OpenApp()
    {
        if (!appPrefab) return;
        
        // 在图标前方创建应用
        Vector3 spawnPos = transform.position + spawnOffset;
        Instantiate(appPrefab, spawnPos, Quaternion.identity);
    }
}