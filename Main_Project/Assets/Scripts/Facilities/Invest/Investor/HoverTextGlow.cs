using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class HoverTextGlow : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public TextMeshProUGUI tmpText;
    private Color originalColor;

    void Start()
    {
        if (tmpText == null)
            tmpText = GetComponentInChildren<TextMeshProUGUI>();

        if (tmpText != null)
            originalColor = tmpText.color;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (tmpText != null)
            tmpText.color = originalColor * 5.0f; // 살짝 밝게
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (tmpText != null)
            tmpText.color = originalColor;
    }
}