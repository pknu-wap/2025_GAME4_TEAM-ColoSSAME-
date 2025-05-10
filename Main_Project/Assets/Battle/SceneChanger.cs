using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Battle
{
    public class SceneChanger : MonoBehaviour
    {
        [SerializeField] private string sceneToLoad;

        public void ChangeScene()
        {
            StartCoroutine(CleanAndLoad());
        }

        private IEnumerator CleanAndLoad()
        { // 새로운 씬을 Single 모드로 로드
            SceneManager.LoadScene(sceneToLoad, LoadSceneMode.Single);
            yield break;
        }
    }
}