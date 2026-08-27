using System;
using System.IO;
using UnityEngine;
using UnityEngine.Audio;

public class SettingsManager : MonoBehaviour
{
    public static SettingsManager Instance { get; private set; }

    [Header("Audio")]
    [SerializeField] private AudioMixer audioMixer;

    private const string SettingsFileName = "Settings.json";

    private const string MasterVolumeParameter = "MasterVolume";
    private const string BGMVolumeParameter = "BGMVolume";
    private const string SFXVolumeParameter = "SFXVolume";
    private const string IngameVolumeParameter = "IngameVolume";

    [SerializeField] private float masterVolume = 1f;
    [SerializeField] private float bgmVolume = 1f;
    [SerializeField] private float sfxVolume = 1f;
    [SerializeField] private float ingameVolume = 1f;

    private string SettingsPath => Path.Combine(Application.persistentDataPath, SettingsFileName);

    [Serializable]
    private class SettingsSaveData
    {
        public float masterVolume;
        public float bgmVolume;
        public float sfxVolume;
        public float ingameVolume;
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        LoadSettings();
        ApplyAllVolumes();
    }

    public float MasterVolume => masterVolume;
    public float BGMVolume => bgmVolume;
    public float SFXVolume => sfxVolume;
    public float IngameVolume => ingameVolume;

    public void SetMasterVolume(float value)
    {
        masterVolume = Mathf.Clamp(value, 0f, 2f);

        ApplyMasterVolume();
        SaveSettings();
    }

    public void SetBGMVolume(float value)
    {
        bgmVolume = Mathf.Clamp(value, 0f, 2f);

        ApplyBGMVolume();
        SaveSettings();
    }

    public void SetSFXVolume(float value)
    {
        sfxVolume = Mathf.Clamp(value, 0f, 2f);

        ApplySFXVolume();
        SaveSettings();
    }

    public void SetIngameVolume(float value)
    {
        ingameVolume = Mathf.Clamp(value, 0f, 2f);

        ApplyIngameVolume();
        SaveSettings();
    }


    private void ApplyAllVolumes()
    {
        ApplyMasterVolume();
        ApplyBGMVolume();
        ApplySFXVolume();
        ApplyIngameVolume();
    }

    private void ApplyMasterVolume()
    {
        SetMixerVolume(MasterVolumeParameter, masterVolume);
    }

    private void ApplyBGMVolume()
    {
        SetMixerVolume(BGMVolumeParameter, bgmVolume);
    }

    private void ApplySFXVolume()
    {
        SetMixerVolume(SFXVolumeParameter, sfxVolume);
    }

    private void ApplyIngameVolume()
    {
        SetMixerVolume(IngameVolumeParameter, ingameVolume);
    }

    private void SetMixerVolume(string parameterName, float value)
    {
        float decibel = value <= 0.0001f
            ? -80f
            : Mathf.Log10(value) * 20f;

        audioMixer.SetFloat(parameterName, decibel);
    }

    private void LoadSettings()
    {
        if (!File.Exists(SettingsPath))
        {
            SaveSettings();
            return;
        }

        try
        {
            string json = File.ReadAllText(SettingsPath);
            SettingsSaveData data = JsonUtility.FromJson<SettingsSaveData>(json);

            if (data == null)
            {
                return;
            }

            masterVolume = Mathf.Clamp(data.masterVolume, 0f, 2f);
            bgmVolume = Mathf.Clamp(data.bgmVolume, 0f, 2f);
            sfxVolume = Mathf.Clamp(data.sfxVolume, 0f, 2f);
            ingameVolume = Mathf.Clamp(data.ingameVolume, 0f, 2f);
        }
        catch (Exception e)
        {
            Debug.Log($"설정 파일을 불러오지 못했습니다");
        }
    }

    private void SaveSettings()
    {
        SettingsSaveData data = new SettingsSaveData
        {
            masterVolume = masterVolume,
            bgmVolume = bgmVolume,
            sfxVolume = sfxVolume,
            ingameVolume = ingameVolume
        };

        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(SettingsPath, json);
    }
}