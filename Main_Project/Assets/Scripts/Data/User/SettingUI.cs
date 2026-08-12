using UnityEngine;
using UnityEngine.UI;

public class SettingsUI : MonoBehaviour
{
    [SerializeField] private Slider masterSlider;
    [SerializeField] private Slider bgmSlider;
    [SerializeField] private Slider sfxSlider;

     private void Start()
    {
        masterSlider.onValueChanged.AddListener(
            SettingsManager.Instance.SetMasterVolume);

        bgmSlider.onValueChanged.AddListener(
            SettingsManager.Instance.SetBGMVolume);

        sfxSlider.onValueChanged.AddListener(
            SettingsManager.Instance.SetSFXVolume);
    }

    private void OnEnable()
    {
        RefreshSliders();
    }

    private void RefreshSliders()
    {
        masterSlider.SetValueWithoutNotify(
            SettingsManager.Instance.GetMasterVolume());

        bgmSlider.SetValueWithoutNotify(
            SettingsManager.Instance.GetBGMVolume());

        sfxSlider.SetValueWithoutNotify(
            SettingsManager.Instance.GetSFXVolume());
    }
}