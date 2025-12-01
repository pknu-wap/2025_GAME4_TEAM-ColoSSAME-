using UnityEngine;
using UnityEngine.UI;

public class HPBar : MonoBehaviour
{
    [SerializeField] private AICore OwnerAi;
    [SerializeField] private Slider hpSlider;            // ← Scrollbar 대신 Slider
    [Header("UI Settings")]
    [SerializeField] private Canvas hpsCanvas;
    [SerializeField] private Vector3 worldOffset = new Vector3(0f, -0.3f, 0f);

    private RectTransform _rect;

    private void Awake()
    {
        if (hpSlider == null)
            hpSlider = GetComponent<Slider>();

        if (hpsCanvas == null)
            hpsCanvas = GetComponentInParent<Canvas>(true);

        _rect = GetComponent<RectTransform>();
        hpsCanvas.worldCamera = FindObjectOfType<Camera>();

        hpSlider.minValue = 0f;
        hpSlider.maxValue = 1f;
        hpSlider.wholeNumbers = false;
        hpSlider.interactable = false;

        // 초기값 반영
        if (OwnerAi != null && OwnerAi.maxHp > 0)
            hpSlider.value = Mathf.Clamp01((float)OwnerAi.hp / OwnerAi.maxHp);
    }

    private void LateUpdate()
    {
        if (OwnerAi == null || _rect == null || hpsCanvas == null) return;

        Vector3 worldPos = OwnerAi.transform.position + worldOffset;

        if (hpsCanvas.renderMode == RenderMode.ScreenSpaceOverlay ||
            hpsCanvas.renderMode == RenderMode.ScreenSpaceCamera)
        {
            Vector2 screenPt = RectTransformUtility.WorldToScreenPoint(hpsCanvas.worldCamera, worldPos);
            RectTransform canvasRect = hpsCanvas.transform as RectTransform;

            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenPt, hpsCanvas.worldCamera, out Vector2 local))
            {
                _rect.anchoredPosition = local;
                _rect.localPosition = new Vector3(_rect.localPosition.x, _rect.localPosition.y, 0f);
            }
            _rect.localScale = (OwnerAi.transform.localScale.x < 0) ? new Vector3(-1f,1f,1f) : new Vector3(1f,1f,1f);
        }
    }

    public void UpdateHPBar()
    {
        if (hpSlider == null || OwnerAi == null) return;
        float v = (OwnerAi.maxHp <= 0) ? 0f : Mathf.Clamp01((float)OwnerAi.hp / OwnerAi.maxHp);
        hpSlider.value = v;
    }
}
