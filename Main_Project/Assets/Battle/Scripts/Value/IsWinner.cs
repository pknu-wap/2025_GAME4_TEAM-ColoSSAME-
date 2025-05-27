using Battle.Scripts.UI;
using UnityEngine;

namespace Battle.Scripts.Value
{
    
    public class IsWinner : MonoBehaviour
    {
        public static IsWinner Instance { get; private set; }

        private void Awake()
        {
            Instance = this;
        }
        
        public int playersNumber;
        public int enemiesNumber;
        GameObject[] players;
        GameObject[] enemies;
        public GameObject Result;

        public void startSetting()
        {
            Result.SetActive(false);
            players = GameObject.FindGameObjectsWithTag("Player");
            playersNumber = players.Length;
            Debug.Log("플레이어 수 : " + playersNumber);
            enemies = GameObject.FindGameObjectsWithTag("Enemy");
            enemiesNumber = enemies.Length;
            Debug.Log("적 수 : " + enemiesNumber);
        }
        
        public void Winner(GameObject tags)
        {
            if(tags.CompareTag("Player")) playersNumber--;
            if(tags.CompareTag("Enemy")) enemiesNumber--;
            if (playersNumber == 0 || enemiesNumber == 0)
            {
                if (playersNumber < enemiesNumber) {
                    
                } else {
                    
                }
                gameObject.SetActive(false);
                Result.SetActive(true);
            }
        }
    }
}
