using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 설정 팝업의 취소 / 기본값 복원.
/// 설정은 값을 바꾸는 즉시 SettingsManager에 적용·저장되므로,
/// 여기서는 창을 열 때의 조작 값을 기억했다가 조작 요소를 되돌리는 방식으로 처리한다.
/// (조작 요소의 onValueChanged가 SettingsManager를 다시 호출한다)
/// '적용'은 현재 값을 그대로 두고 창만 닫으면 된다.
/// </summary>
public class SettingsPopupController : MonoBehaviour
{
    [SerializeField] private Slider[] volumeSliders;
    [SerializeField] private TMP_Dropdown languageDropdown;
    [SerializeField] private TMP_Dropdown screenModeDropdown;
    [SerializeField] private TMP_Dropdown resolutionDropdown;

    [Header("기본값")]
    [SerializeField] private float defaultVolume = 1f;
    [Tooltip("화면 모드 드롭다운의 기본 항목 (0 = 전체화면)")]
    [SerializeField] private int defaultScreenModeIndex = 0;

    private float[] openedVolumes;
    private int openedLanguage, openedScreenMode, openedResolution;

    private void OnEnable()
    {
        openedVolumes = new float[volumeSliders.Length];
        for (int i = 0; i < volumeSliders.Length; i++) openedVolumes[i] = volumeSliders[i].value;

        openedLanguage = languageDropdown.value;
        openedScreenMode = screenModeDropdown.value;
        openedResolution = resolutionDropdown.value;
    }

    /// <summary>창을 연 뒤 바꾼 값을 모두 되돌린다.</summary>
    public void Cancel()
    {
        if (openedVolumes == null) return;

        for (int i = 0; i < volumeSliders.Length; i++) volumeSliders[i].value = openedVolumes[i];
        screenModeDropdown.value = openedScreenMode;
        resolutionDropdown.value = openedResolution;
        languageDropdown.value = openedLanguage;
    }

    /// <summary>SettingsManager의 첫 실행 값과 같은 기본값을 넣는다.</summary>
    public void ResetToDefault()
    {
        foreach (var slider in volumeSliders) slider.value = defaultVolume;

        screenModeDropdown.value = defaultScreenModeIndex;
        // 해상도 목록은 낮은 것부터 들어 있고, 기본값은 지원하는 가장 높은 해상도
        resolutionDropdown.value = Mathf.Max(0, resolutionDropdown.options.Count - 1);
        // 언어 목록 순서: 한국어, English
        languageDropdown.value = Application.systemLanguage == SystemLanguage.Korean ? 0 : 1;
    }
}
