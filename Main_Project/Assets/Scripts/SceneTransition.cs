using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransition : MonoBehaviour
{
    public static SceneTransition Instance { get; private set; }

    [SerializeField] private CanvasGroup fade;   // 검은 전체화면 Image + CanvasGroup
    [SerializeField] private float duration = 0.4f;
    [SerializeField] private LoadingScreen loadingScreen; // 선택: 페이드 중 보이는 로딩 화면(진행 바)

    private bool _isBusy = false;
    public bool IsBusy => _isBusy;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        fade.alpha = 0f;
        fade.blocksRaycasts = false;
    }

    public void Load(string sceneName)
    {
        if (_isBusy) return;     
        _isBusy = true;
        StartCoroutine(Co(sceneName));
    }

    public void LoadOverCurrentScene(string sceneName, float minimumWait)
    {
        if (_isBusy) return;
        var operation = SceneManager.LoadSceneAsync(sceneName);
        operation.allowSceneActivation = false;
        _isBusy = true;
        StartCoroutine(ShowHudUntilReady(operation, minimumWait));
    }

    private IEnumerator ShowHudUntilReady(AsyncOperation operation, float minimumWait)
    {
        loadingScreen.Begin(overCurrentScene: true);
        fade.blocksRaycasts = true;
        fade.alpha = 1f;
        double earliestActivation = Time.realtimeSinceStartupAsDouble + minimumWait;

        while (true)
        {
            loadingScreen.SetProgress(operation.progress / 0.9f);
            if (operation.progress >= 0.9f && Time.realtimeSinceStartupAsDouble >= earliestActivation && loadingScreen.IsCaughtUp)
                break;
            yield return null;
        }

        operation.completed += _ =>
        {
            fade.alpha = 0f;
            fade.blocksRaycasts = false;
            _isBusy = false;
        };
        operation.allowSceneActivation = true;
    }

    IEnumerator Co(string sceneName)
    {
        fade.blocksRaycasts = true;
        if (loadingScreen != null) loadingScreen.Begin();
        yield return Fade(0f, 1f);                        // 검게

        var op = SceneManager.LoadSceneAsync(sceneName);  // 비동기 로드
        while (!op.isDone)
        {
            if (loadingScreen != null) loadingScreen.SetProgress(op.progress / 0.9f);
            yield return null;
        }
        if (loadingScreen != null)
        {
            loadingScreen.SetProgress(1f);
            while (!loadingScreen.IsCaughtUp) yield return null; // 바가 끝까지 찬 뒤 밝게
        }

        yield return Fade(1f, 0f);                        // 밝게
        fade.blocksRaycasts = false;
        _isBusy = false;
    }

    IEnumerator Fade(float a, float b)
    {
        for (float t = 0; t < duration; t += Time.unscaledDeltaTime)
        {
            fade.alpha = Mathf.Lerp(a, b, t / duration);
            yield return null;
        }
        fade.alpha = b;
    }
}