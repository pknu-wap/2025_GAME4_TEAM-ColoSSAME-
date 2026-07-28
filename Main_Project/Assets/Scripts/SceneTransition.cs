using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransition : MonoBehaviour
{
    public static SceneTransition Instance { get; private set; }

    [SerializeField] private CanvasGroup fade;   // 검은 전체화면 Image + CanvasGroup
    [SerializeField] private float duration = 0.4f;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        fade.alpha = 0f;
        fade.blocksRaycasts = false;
    }

    public void Load(string sceneName) => StartCoroutine(Co(sceneName));

    IEnumerator Co(string sceneName)
    {
        fade.blocksRaycasts = true;
        yield return Fade(0f, 1f);                        // 검게

        var op = SceneManager.LoadSceneAsync(sceneName);  // 비동기 로드
        while (!op.isDone) yield return null;

        yield return Fade(1f, 0f);                        // 밝게
        fade.blocksRaycasts = false;
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