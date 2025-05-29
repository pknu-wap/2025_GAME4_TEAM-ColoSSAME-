using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Battle.Scripts
{
    public class SceneChanger : MonoBehaviour
    {
        [SerializeField] private string sceneToLoad;
        

        public void ChangeScene()
        {
            Debug.Log("버튼 눌러짐");
            StartCoroutine(CleanAndLoad());
        }

        private IEnumerator CleanAndLoad()
        {
            SceneManager.LoadScene(sceneToLoad, LoadSceneMode.Single);
            yield break;
        }
    }
}
