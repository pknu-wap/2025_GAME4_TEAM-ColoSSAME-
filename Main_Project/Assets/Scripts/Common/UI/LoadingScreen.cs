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

    private float target;
    private float shown;

    /// <summary>바가 목표 진행률까지 다 찼는지</summary>
    public bool IsCaughtUp => shown >= target - 0.001f;

    public void Begin()
    {
        target = shown = 0f;
        Apply();
        SetLocalized(status, statusText, "");
        if (tips != null && tips.Length > 0)
            SetLocalized(tips[Random.Range(0, tips.Length)], tipText, tipLabel);
    }

    public void SetProgress(float progress) => target = Mathf.Clamp01(progress);

    private void Update()
    {
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
