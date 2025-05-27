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
        private GameObject[] exitevent;
        public WinnerText winnerText;
        public NextScene nextScene;
        GameObject[] players;
        GameObject[] enemies;
        GameObject Ai;

        public void startSetting()
        {
            Ai = GameObject.FindGameObjectWithTag("Ai");
            players = GameObject.FindGameObjectsWithTag("Player");
            playersNumber = players.Length;
            Debug.Log("플레이어 수 : " + playersNumber);
            enemies = GameObject.FindGameObjectsWithTag("Enemy");
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
                winnerText.gameObject.SetActive(true);
                nextScene.ButtonOn();
                if (playersNumber < enemiesNumber) {
                    winnerText.Lose();
                } else {
                    winnerText.Win();
                }
                gameObject.SetActive(false);
            }
            foreach (GameObject obj in exitevent)
            {
                obj.SetActive(true);
            }
        }
    }
}
