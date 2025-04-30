using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChanger : MonoBehaviour
{
    // Inspector 창에서 설정할 씬 이름
    [SerializeField] private string sceneName;

    // 씬을 이름으로 불러오는 함수
    public void ChangeScene()
    {
        SceneManager.LoadScene(sceneName);
    }

    // 씬 번호로 불러오는 함수
    public void ChangeSceneByIndex(int sceneIndex)
    {
        SceneManager.LoadScene(sceneIndex);
    }

    // 프로그램 종료 함수 (선택사항)
    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("게임 종료");  // 에디터에서는 실제 종료되지 않으니까 로그 출력
    }
}