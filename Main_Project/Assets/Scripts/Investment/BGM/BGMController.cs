using UnityEngine;
using UnityEngine.SceneManagement;

public class BGMController : MonoBehaviour
{
    public static BGMController Instance;

    public AudioSource bgmAudioSource;
    public AudioClip defaultBgmClip;
    public AudioClip loadingBgmClip;
    public AudioClip pageClip;
    public AudioClip battleClip;
    public AudioClip strategyClip;

    private void Awake()
    {
        // 싱글톤 + 중복 제거 + 파괴 방지
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // ✅ 씬 전환 이벤트 등록
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnDestroy()
    {
        // 씬 전환 이벤트 제거 (중복 방지)
        if (Instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log("🎬 Scene 전환 감지: " + scene.name);

        // 씬 이름에 따라 자동으로 BGM 설정
        switch (scene.name)
        {
            case "kbeombMainMenu":
                PlayDefaultBGM();
                break;
            case "BattleStartTemp 1":
                PlayBattleBGM();
                break;
            case "StrategySceneTemp":
                PlayStrategyBGM();
                break;
            default:
                PlayDefaultBGM();
                break;
        }
    }

    public void PlayDefaultBGM()
    {
        PlayClip(defaultBgmClip, "Default BGM");
    }

    public void PlayLoadingBGM()
    {
        PlayClip(loadingBgmClip, "Loading BGM");
    }

    public void PlayBattleBGM()
    {
        PlayClip(battleClip, "Battle BGM");
    }

    public void PlayStrategyBGM()
    {
        PlayClip(strategyClip, "Strategy BGM");
    }

    public void PlayPageBGM()
    {
        PlayClip(pageClip, "Page BGM");
    }

    private void PlayClip(AudioClip clip, string label)
    {
        if (bgmAudioSource == null || clip == null)
        {
            Debug.LogWarning("🔇 AudioSource 또는 Clip이 null입니다");
            return;
        }

        if (bgmAudioSource.clip == clip && bgmAudioSource.isPlaying)
        {
            Debug.Log($"🎵 이미 {label}이 재생 중입니다");
            return;
        }

        Debug.Log($"🎵 {label} 재생 시작");
        bgmAudioSource.Stop();
        bgmAudioSource.clip = clip;
        bgmAudioSource.Play();
    }
}
