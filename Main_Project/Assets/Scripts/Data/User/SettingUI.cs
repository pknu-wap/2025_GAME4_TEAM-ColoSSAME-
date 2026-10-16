using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

public class SettingsUI : MonoBehaviour
{
    [SerializeField] private Slider masterSlider;
    [SerializeField] private Slider bgmSlider;
    [SerializeField] private Slider sfxSlider;
    [SerializeField] private Slider ingameSlider;
    [SerializeField] private GameObject mainButton;

    [Header("화면 (비어 있으면 무시)")]
    [SerializeField] private TMP_Dropdown resolutionDropdown;
    [SerializeField] private TMP_Dropdown screenModeDropdown;

    [Header("언어 (비어 있으면 무시)")]
    [SerializeField] private TMP_Dropdown languageDropdown;

    private const string UITable = "UI";

    private static readonly FullScreenMode[] ScreenModes = { FullScreenMode.FullScreenWindow, FullScreenMode.Windowed };
    private static readonly string[] ScreenModeKeys = { "screenmode_fullscreen", "screenmode_windowed" };
    private static readonly string[] ScreenModeFallback = { "전체화면", "창 모드" };

    private static readonly string[] LanguageCodes = { "ko", "en" };
    private static readonly string[] LanguageNames = { "한국어", "English" };

    private bool initialized;

    private void Start()
    {
        // 메인메뉴 설정창에는 메인화면 버튼이 없음
        if (mainButton != null)
            mainButton.SetActive(SceneManager.GetActiveScene().name != "MainMenu");

        var settings = SettingsManager.Instance;
        if (settings == null)
        {
            return;
        }

        masterSlider.onValueChanged.AddListener(settings.SetMasterVolume);
        bgmSlider.onValueChanged.AddListener(settings.SetBGMVolume);
        sfxSlider.onValueChanged.AddListener(settings.SetSFXVolume);
        ingameSlider.onValueChanged.AddListener(settings.SetIngameVolume);

        SetupDisplayDropdowns(settings);
        SetupLanguageDropdown(settings);
        LocalizationSettings.SelectedLocaleChanged += OnLocaleChanged;

        initialized = true;
        RefreshUI();

        if (!LocalizationSettings.InitializationOperation.IsDone)
            LocalizationSettings.InitializationOperation.Completed += _ =>
            {
                if (this == null) return;
                OnLocaleChanged(LocalizationSettings.SelectedLocale);
            };
    }

    private void OnDestroy()
    {
        LocalizationSettings.SelectedLocaleChanged -= OnLocaleChanged;
    }

    private void OnEnable()
    {
        if (initialized) RefreshUI();
    }

    private void SetupDisplayDropdowns(SettingsManager settings)
    {
        if (resolutionDropdown != null)
        {
            var labels = new List<string>();
            foreach (var r in settings.AvailableResolutions)
                labels.Add($"{r.x} x {r.y}");

            resolutionDropdown.ClearOptions();
            resolutionDropdown.AddOptions(labels);
            resolutionDropdown.onValueChanged.AddListener(
                i => settings.SetResolution(settings.AvailableResolutions[i]));
        }

        if (screenModeDropdown != null)
        {
            FillScreenModeOptions();
            screenModeDropdown.onValueChanged.AddListener(
                i => settings.SetScreenMode(ScreenModes[i]));
        }
    }

    private void SetupLanguageDropdown(SettingsManager settings)
    {
        if (languageDropdown == null) return;

        languageDropdown.ClearOptions();
        languageDropdown.AddOptions(new List<string>(LanguageNames));
        languageDropdown.onValueChanged.AddListener(i => settings.SetLanguage(LanguageCodes[i]));
    }

    private void FillScreenModeOptions()
    {
        var names = new List<string>();
        for (int i = 0; i < ScreenModeKeys.Length; i++)
            names.Add(Localize(ScreenModeKeys[i], ScreenModeFallback[i]));

        screenModeDropdown.ClearOptions();
        screenModeDropdown.AddOptions(names);
    }

    private void OnLocaleChanged(Locale locale)
    {
        if (screenModeDropdown != null) FillScreenModeOptions();
        RefreshUI();
    }

    private static string Localize(string key, string fallback)
    {
        if (!LocalizationSettings.InitializationOperation.IsDone) return fallback;

        var text = LocalizationSettings.StringDatabase.GetLocalizedString(UITable, key);
        return string.IsNullOrEmpty(text) ? fallback : text;
    }

    private void RefreshUI()
    {
        var settings = SettingsManager.Instance;
        if (settings == null) return;

        settings.SyncScreenMode();

        masterSlider.SetValueWithoutNotify(settings.MasterVolume);
        bgmSlider.SetValueWithoutNotify(settings.BGMVolume);
        sfxSlider.SetValueWithoutNotify(settings.SFXVolume);
        ingameSlider.SetValueWithoutNotify(settings.IngameVolume);

        if (resolutionDropdown != null)
        {
            int index = -1;
            for (int i = 0; i < settings.AvailableResolutions.Count; i++)
                if (settings.AvailableResolutions[i] == settings.Resolution) { index = i; break; }
            resolutionDropdown.SetValueWithoutNotify(Mathf.Max(0, index));
        }

        if (screenModeDropdown != null)
            screenModeDropdown.SetValueWithoutNotify(Mathf.Max(0, Array.IndexOf(ScreenModes, settings.ScreenMode)));

        if (languageDropdown != null)
            languageDropdown.SetValueWithoutNotify(Mathf.Max(0, Array.IndexOf(LanguageCodes, settings.LanguageCode)));
    }
}
