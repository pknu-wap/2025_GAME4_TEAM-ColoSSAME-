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

    private void Start()
    {
        masterSlider.onValueChanged.AddListener(
            SettingsManager.Instance.SetMasterVolume);

        bgmSlider.onValueChanged.AddListener(
            SettingsManager.Instance.SetBGMVolume);

        sfxSlider.onValueChanged.AddListener(
            SettingsManager.Instance.SetSFXVolume);

        ingameSlider.onValueChanged.AddListener(
            SettingsManager.Instance.SetIngameVolume);

        mainButton.SetActive(
        SceneManager.GetActiveScene().name != "MainMenu");
    }

    private void OnEnable()
    {
        RefreshSliders();
    }

    private void RefreshSliders()
    {
        masterSlider.SetValueWithoutNotify(
            SettingsManager.Instance.MasterVolume);

        bgmSlider.SetValueWithoutNotify(
            SettingsManager.Instance.BGMVolume);

        sfxSlider.SetValueWithoutNotify(
            SettingsManager.Instance.SFXVolume);

        ingameSlider.SetValueWithoutNotify(
            SettingsManager.Instance.IngameVolume);
    }
}