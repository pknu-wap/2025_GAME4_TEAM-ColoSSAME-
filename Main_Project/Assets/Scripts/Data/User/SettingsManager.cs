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

    [SerializeField] private float masterVolume = 1f;
    [SerializeField] private float bgmVolume = 1f;
    [SerializeField] private float sfxVolume = 1f;

    private string SettingsPath =>
        Path.Combine(Application.persistentDataPath, SettingsFileName);

    [Serializable]
    private class SettingsSaveData
    {
        public float masterVolume;
        public float bgmVolume;
        public float sfxVolume;
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

    public void SetMasterVolume(float value)
    {
        masterVolume = Mathf.Clamp01(value);

        ApplyMasterVolume();
        SaveSettings();
    }

    public void SetBGMVolume(float value)
    {
        bgmVolume = Mathf.Clamp01(value);

        ApplyBGMVolume();
        SaveSettings();
    }

    public void SetSFXVolume(float value)
    {
        Debug.Log($"Slider에서 받은 값: {value}");

        sfxVolume = Mathf.Clamp01(value);

        Debug.Log($"저장할 SFX 값: {sfxVolume}");

        ApplySFXVolume();
        SaveSettings();
    }

    public float GetMasterVolume()
    {
        return masterVolume;
    }

    public float GetBGMVolume()
    {
        return bgmVolume;
    }

    public float GetSFXVolume()
    {
        return sfxVolume;
    }

    private void ApplyAllVolumes()
    {
        ApplyMasterVolume();
        ApplyBGMVolume();
        ApplySFXVolume();
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

    private void SetMixerVolume(string parameterName, float value)
    {
        if (audioMixer == null)
        {
            Debug.Log("SettingsManager에 AudioMixer가 연결되지 않았습니다.");
            return;
        }

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

            masterVolume = Mathf.Clamp01(data.masterVolume);
            bgmVolume = Mathf.Clamp01(data.bgmVolume);
            sfxVolume = Mathf.Clamp01(data.sfxVolume);
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
            sfxVolume = sfxVolume
        };

        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(SettingsPath, json);
    }
}