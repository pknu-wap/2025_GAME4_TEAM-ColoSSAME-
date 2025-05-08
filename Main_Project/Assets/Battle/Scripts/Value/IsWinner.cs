using System.Linq;
using UnityEngine;

namespace Battle.Value
{
    
    public class IsWinner : MonoBehaviour
    {
        public int playersNumber;
        public int enemiesNumber;
        private SceneChanger sceneChanger;
        private GameObject[] exitevent;
        private void Start()
        {
           sceneChanger = FindObjectOfType<SceneChanger>();
            GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
            playersNumber = players.Length;
            Debug.Log("플레이어 수 : " + playersNumber);
            GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
            enemiesNumber = enemies.Length;
            Debug.Log("적 수 : " + enemiesNumber);
            exitevent = GameObject.FindGameObjectsWithTag("Exitevent");
            foreach (GameObject obj in exitevent)
            {
                obj.SetActive(false);
            }
        }

        public void Winner(GameObject tags)
        {
            if(tags.CompareTag("Player")) playersNumber--;
            if(tags.CompareTag("Enemy")) enemiesNumber--;
            if (playersNumber == 0 || enemiesNumber == 0)
            {
                Debug.Log(playersNumber == 0 ? "플레이어 패배" : "플레이어 승리");
                sceneChanger.ChangeScene();
                gameObject.SetActive(false);
            }
            foreach (GameObject obj in exitevent)
            {
                obj.SetActive(true);
            }
        }
    }
}
