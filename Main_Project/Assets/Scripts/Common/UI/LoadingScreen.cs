using UnityEngine;
using UnityEngine.Localization;
using TMPro;

/// <summary>
/// 씬 전환 중 보이는 로딩 화면. SceneTransition이 진행률을 넘겨주면 진행 바와 %를 부드럽게 채운다.
/// 화면 표시/숨김은 SceneTransition의 CanvasGroup 페이드가 맡는다.
/// </summary>
public class LoadingScreen : MonoBehaviour
{
    [SerializeField] private RectTransform fill;          // 진행 바 채움 (anchorMax.x로 길이 조절)
    [SerializeField] private TMP_Text percentText;
    [SerializeField] private TMP_Text statusText;
    [SerializeField] private TMP_Text tipText;
    [SerializeField] private LocalizedString status;
    [SerializeField] private LocalizedString[] tips;
    [SerializeField] private string tipLabel = "<color=#FFD98A>TIP</color>  ";
    [SerializeField] private float fillSpeed = 2f;        // 초당 채우는 비율

    [SerializeField] private UnityEngine.UI.Image backdrop;
    [SerializeField] private RectTransform bottomBand;
    [SerializeField] private RectTransform progressBar;
    [SerializeField, Min(0.01f)] private float overlayBarScale = 2f;
    [UnityEngine.Serialization.FormerlySerializedAs("overlayScale")]
    [SerializeField, Range(0.1f, 1.5f), Tooltip("새 게임 진행 바와 모든 문구의 공통 배율")]
    private float overlayContentScale = 0.6f;
    [SerializeField, Min(0.1f), Tooltip("공통 배율에 곱하는 상태, 퍼센트, TIP 문구 배율")]
    private float overlayTextScale = 1.8f;

    // The band stretches across its parent. Only its children are scaled.
    private float layoutWidth;
    private Vector2 originalBandSize;
    private RectTransform[] overlayItems;
    private Vector2[] originalPositions;
    private Vector3[] originalScales;

    private bool overCurrentScene;
    private float target;
    private float shown;

    /// <summary>바가 목표 진행률까지 다 찼는지</summary>
    public bool IsCaughtUp => shown >= target - 0.001f;

    public void Begin(bool overCurrentScene = false)
    {
        this.overCurrentScene = overCurrentScene;
        if (backdrop != null) backdrop.canvasRenderer.SetAlpha(overCurrentScene ? 0f : 1f);
        RefreshLayout();
        target = shown = 0f;
        Apply();
        SetLocalized(status, statusText, "");
        if (tips != null && tips.Length > 0)
            SetLocalized(tips[Random.Range(0, tips.Length)], tipText, tipLabel);
    }

    private RectTransform Bar
    {
        get
        {
            if (progressBar != null) return progressBar;
            return bottomBand != null ? bottomBand.Find("ProgressBar") as RectTransform : null;
        }
    }

    private Vector2 BarSize(float width)
    {
        var bar = Bar;
        if (bar == null) return Vector2.zero;
        float scale = Mathf.Min(overlayContentScale * overlayBarScale, width * 0.94f / bar.rect.width);
        return Vector2.Scale(bar.rect.size, new Vector2(scale, scale * 0.45f));
    }

    public void RefreshLayout()
    {
        if (bottomBand == null) return;
        if (!overCurrentScene)
        {
            if (overlayItems == null) return;
            bottomBand.sizeDelta = originalBandSize;
            for (int i = 0; i < overlayItems.Length; i++)
            {
                overlayItems[i].localScale = originalScales[i];
                overlayItems[i].anchoredPosition = originalPositions[i];
            }
            overlayItems = null;
            return;
        }

        var bar = Bar;
        if (bar == null || statusText == null || percentText == null || tipText == null) return;
        var statusRect = statusText.rectTransform;
        var percentRect = percentText.rectTransform;
        var tipRect = tipText.rectTransform;
        if (overlayItems == null)
        {
            originalBandSize = bottomBand.sizeDelta;
            overlayItems = new[] { bar, statusRect, percentRect, tipRect };
            originalPositions = System.Array.ConvertAll(overlayItems, item => item.anchoredPosition);
            originalScales = System.Array.ConvertAll(overlayItems, item => item.localScale);
        }

        layoutWidth = bottomBand.rect.width;
        if (layoutWidth <= 0f) return;
        Vector2 size = BarSize(layoutWidth);
        float textScale = Mathf.Min(overlayContentScale * overlayTextScale, layoutWidth * 0.94f / tipRect.rect.width);
        float gap = 5f * overlayContentScale;
        float padding = 7f * overlayContentScale;
        float tipHeight = tipRect.rect.height * textScale;
        float labelHeight = Mathf.Max(statusRect.rect.height, percentRect.rect.height) * textScale;
        float barBottom = padding + tipHeight + gap;
        float labelBottom = barBottom + size.y + gap;

        bottomBand.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, labelBottom + labelHeight + padding);
        bar.localScale = new Vector3(size.x / bar.rect.width, size.y / bar.rect.height, 1f);
        bar.anchoredPosition = new Vector2(0f, barBottom + size.y * 0.5f);

        // Leave enough horizontal space for both labels even with a short bar.
        float labelWidth = Mathf.Max(size.x, (statusRect.rect.width + percentRect.rect.width) * textScale + gap);
        float labelY = labelBottom + labelHeight * 0.5f;
        PlaceText(statusRect, textScale, (statusRect.rect.width * textScale - labelWidth) * 0.5f, labelY);
        PlaceText(percentRect, textScale, (labelWidth - percentRect.rect.width * textScale) * 0.5f, labelY);
        PlaceText(tipRect, textScale, 0f, padding + tipHeight * 0.5f);
    }

    private static void PlaceText(RectTransform rect, float scale, float x, float y)
    {
        rect.localScale = new Vector3(scale, scale, 1f);
        rect.anchoredPosition = new Vector2(x, y);
    }

#if UNITY_EDITOR
    public Vector2 OverlayBarSize(float referenceWidth) => BarSize(referenceWidth);
#endif

    public void SetProgress(float progress) => target = Mathf.Clamp01(progress);

    private void Update()
    {
        if (overCurrentScene && bottomBand != null && !Mathf.Approximately(layoutWidth, bottomBand.rect.width))
            RefreshLayout();
        if (shown >= target) return;
        shown = Mathf.MoveTowards(shown, target, fillSpeed * Time.unscaledDeltaTime);
        Apply();
    }

    private void Apply()
    {
        if (fill != null) fill.anchorMax = new Vector2(shown, fill.anchorMax.y);
        if (percentText != null) percentText.text = Mathf.RoundToInt(shown * 100f) + "%";
    }

    private static void SetLocalized(LocalizedString str, TMP_Text text, string prefix)
    {
        if (text == null || str == null || str.IsEmpty) return;
        var op = str.GetLocalizedStringAsync();
        if (op.IsDone) text.text = prefix + op.Result;
        else op.Completed += h => { if (text != null) text.text = prefix + h.Result; };
    }
}
