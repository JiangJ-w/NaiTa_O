using UnityEngine;
using TMPro;

public class ThoughtBubble : MonoBehaviour
{
    public TMP_Text textComponent;
    public float lifetime = 3f;
    
    void Start()
    {
        Destroy(gameObject, lifetime);
    }
    
    public void SetText(string text)
    {
        if (textComponent != null)
            textComponent.text = text;
    }
}