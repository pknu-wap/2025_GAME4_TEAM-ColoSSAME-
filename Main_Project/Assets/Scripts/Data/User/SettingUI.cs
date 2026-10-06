using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

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

    private static readonly FullScreenMode[] ScreenModes = { FullScreenMode.FullScreenWindow, FullScreenMode.Windowed };
    private static readonly string[] ScreenModeNames = { "전체화면", "창 모드" };

    private bool initialized;

    private void Start()
    {
        mainButton.SetActive(
            SceneManager.GetActiveScene().name != "MainMenu");

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

        initialized = true;
        RefreshUI();
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
                labels.Add($"{r.x} × {r.y}");

            resolutionDropdown.ClearOptions();
            resolutionDropdown.AddOptions(labels);
            resolutionDropdown.onValueChanged.AddListener(
                i => settings.SetResolution(settings.AvailableResolutions[i]));
        }

        if (screenModeDropdown != null)
        {
            screenModeDropdown.ClearOptions();
            screenModeDropdown.AddOptions(new List<string>(ScreenModeNames));
            screenModeDropdown.onValueChanged.AddListener(
                i => settings.SetScreenMode(ScreenModes[i]));
        }
    }

    private void RefreshUI()
    {
        var settings = SettingsManager.Instance;
        if (settings == null) return;

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
    }
}
