using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class SceneCleaner : MonoBehaviour
{
    [SerializeField] private string sceneToLoad;

    public void ChangeScene()
    {
        StartCoroutine(CleanAndLoad());
    }

    private IEnumerator CleanAndLoad()
    {
        // 현재 활성 씬 제외한 모든 씬 언로드
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            Scene s = SceneManager.GetSceneAt(i);
            if (s.name != sceneToLoad && s.isLoaded)
            {
                yield return SceneManager.UnloadSceneAsync(s);
            }
        }

        // 새로운 씬을 Single 모드로 로드
        SceneManager.LoadScene(sceneToLoad, LoadSceneMode.Single);
    }
}