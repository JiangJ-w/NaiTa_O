using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Camera2d : MonoBehaviour
{
    public CanvasGroup canvasGroup;

    void Start()
    {
        // 一开始隐藏
        HideUI();
    }

    public void ShowUI()
    {
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }
    }

    public void HideUI()
    {
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }
    }
}
