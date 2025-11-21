using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    // 원하는 씬의 이름을 이 변수에 넣어주세요.
    public string nextSceneName;

    // 특정 씬으로 전환하는 메소드
    public void LoadScene()
    {
        // 씬 이름을 사용하여 씬을 로드합니다.
        Debug.Log(nextSceneName);
        SceneManager.LoadScene(nextSceneName);
    }
}