using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Battle.Scripts
{
    public class Changer : MonoBehaviour
    {
        [SerializeField] private string sceneToLoad;
        [SerializeField] private float loadingtime;
        

        public void ChangeScene()
        {
            Debug.Log("버튼 눌러짐");
            StartCoroutine(CleanAndLoad());
        }

        private IEnumerator CleanAndLoad()
        {
			yield return new WaitForSeconds(loadingtime);
            SceneManager.LoadScene(sceneToLoad, LoadSceneMode.Single);
            yield break;
        }
    }
}
