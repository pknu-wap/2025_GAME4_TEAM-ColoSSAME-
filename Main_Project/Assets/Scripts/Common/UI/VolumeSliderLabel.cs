using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;

/// <summary>
/// 음량 슬라이더 옆의 표시: 퍼센트 글자, 0일 때 '음소거'와 꺼진 스피커 아이콘.
/// 슬라이더 값 1 = 100% (SettingsManager의 음량 범위는 0~2).
/// </summary>
[RequireComponent(typeof(Slider))]
public class VolumeSliderLabel : MonoBehaviour
{
    [SerializeField] private TMP_Text valueText;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private Image icon;
    [SerializeField] private Sprite onSprite;
    [SerializeField] private Sprite offSprite;

    [SerializeField] private Color valueColor = Color.white;
    [SerializeField] private Color nameColor = Color.white;
    [SerializeField] private Color mutedColor = Color.gray;

    [SerializeField] private string table = "UI";
    [SerializeField] private string mutedKey = "settings_muted";
    [SerializeField] private string mutedFallback = "음소거";

    private Slider slider;

    private void Awake()
    {
        slider = GetComponent<Slider>();
        slider.onValueChanged.AddListener(_ => Refresh());
    }

    private void OnEnable()
    {
        LocalizationSettings.SelectedLocaleChanged += OnLocaleChanged;
        Refresh();
    }

    private void OnDisable()
    {
        LocalizationSettings.SelectedLocaleChanged -= OnLocaleChanged;
    }

    private void OnLocaleChanged(Locale _) => Refresh();

    private void Refresh()
    {
        bool muted = slider.value <= 0.0001f;

        if (valueText != null)
        {
            valueText.text = muted ? MutedText() : Mathf.RoundToInt(slider.value * 100f) + "%";
            valueText.color = muted ? mutedColor : valueColor;
        }

        if (nameText != null) nameText.color = muted ? mutedColor : nameColor;
        if (icon != null) icon.sprite = muted ? offSprite : onSprite;
    }

    private string MutedText()
    {
        if (!LocalizationSettings.InitializationOperation.IsDone) return mutedFallback;

        var text = LocalizationSettings.StringDatabase.GetLocalizedString(table, mutedKey);
        return string.IsNullOrEmpty(text) ? mutedFallback : text;
    }
}
