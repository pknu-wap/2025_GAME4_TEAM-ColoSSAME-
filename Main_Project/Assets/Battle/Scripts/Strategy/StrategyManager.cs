using System;
using Battle.Scripts.Value.Data;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Battle.Scripts.Strategy
{
    public class StrategyManager : MonoBehaviour
    {
        public GameObject[] players;
        public GameObject[] characters;
        public GameObject[] enemyCharacters;
        public Transform ClickedCharacter;
        public Transform ClickedPlayer;
        public GameObject ClickedEnemy;
        public bool hasCharacter;
        public bool hasPlayer;
        public bool hasEnemy;
        public Vector2 MaxArea;
        public Vector2 MinArea;
        
        private void Awake()
        {
            players = GameObject.FindGameObjectsWithTag("Position");

            // characters를 characterKey 기준으로 정렬
            var unsortedCharacters = GameObject.FindGameObjectsWithTag("PlayerCharacter");
            Array.Sort(unsortedCharacters, (a, b) =>
            {
                var idA = a.GetComponent<CharacterID>();
                var idB = b.GetComponent<CharacterID>();

                if (idA == null || idB == null)
                    return 0;

                if (int.TryParse(idA.characterKey, out int keyA) && int.TryParse(idB.characterKey, out int keyB))
                {
                    return keyA.CompareTo(keyB); // 숫자 순으로 정렬
                }

                return string.Compare(idA.characterKey, idB.characterKey, StringComparison.Ordinal); // 파싱 실패 시 fallback
            });
            characters = unsortedCharacters;
            var unsortedEnemies = GameObject.FindGameObjectsWithTag("EnemyCharacter");
            Array.Sort(unsortedEnemies, (a, b) =>
            {
                var idA = a.GetComponent<CharacterID>();
                var idB = b.GetComponent<CharacterID>();

                if (idA == null || idB == null)
                    return 0;

                if (int.TryParse(idA.characterKey, out int keyA) && int.TryParse(idB.characterKey, out int keyB))
                {
                    return keyA.CompareTo(keyB); // 숫자 순으로 정렬
                }

                return string.Compare(idA.characterKey, idB.characterKey, StringComparison.Ordinal); // 파싱 실패 시 fallback
            });
            enemyCharacters = unsortedEnemies;
        }
        
        private void Start()
        {
            TwoTwo();
        }
        

        private void ResetCharacters()
        {
            int maxPerRow = 5;
            ResetCharacter();
            for (int j = 0; j < Mathf.CeilToInt(characters.Length / (float)maxPerRow); j++)
            {
                for (int i = 0; i < maxPerRow; i++)
                {
                    int index = i + maxPerRow * j;
                    if (index >= characters.Length) break;

                    characters[index].transform.localPosition = new Vector3(-0.3f + (i * 0.15f), 0.2f - (j * 0.2f), 0);
                    characters[index].layer = 0;
                }
            }
            
            for (int j = 0; j < Mathf.CeilToInt(enemyCharacters.Length / (float)maxPerRow); j++)
            {
                for (int i = 0; i < maxPerRow; i++)
                {
                    int index = i + maxPerRow * j;
                    if (index >= enemyCharacters.Length) break;

                    enemyCharacters[index].transform.localPosition = new Vector3(-0.3f + (i * 0.15f), 0.2f - (j * 0.2f), 0);
                    enemyCharacters[index].layer = 0;
                    Debug.Log(enemyCharacters[index].name);
                }
            }
        }
        private void ActivePlayer()
        {
            foreach (var player in players)
            {
                player.SetActive(true);
            }
        }

        public void OneTwoOne()
        {
            ActivePlayer();
            ResetCharacters();
            players[0].transform.localPosition = new Vector2(-0.2f, 0f);
            players[1].transform.localPosition = new Vector2(0.2f, 0f);
            players[2].transform.localPosition = new Vector2(0f, 0.2f);
            players[3].transform.localPosition = new Vector2(0f, -0.2f);
        }

        public void TwoTwo()
        {
            ActivePlayer();
            ResetCharacters();
            players[0].transform.localPosition = new Vector2(-0.15f, -0.15f);
            players[1].transform.localPosition = new Vector2(0.15f, -0.15f);
            players[2].transform.localPosition = new Vector2(-0.15f, 0.15f);
            players[3].transform.localPosition = new Vector2(0.15f, 0.15f);
        }

        public void OneOneTwo()
        {
            ActivePlayer();
            ResetCharacters();
            players[0].transform.localPosition = new Vector2(-0.15f, 0f);
            players[1].transform.localPosition = new Vector2(0f, 0f);
            players[2].transform.localPosition = new Vector2(0.15f, -0.2f);
            players[3].transform.localPosition = new Vector2(0.15f, 0.2f);
        }

        public void ResetCharacter()
        {
            if (ClickedCharacter != null)
            {
                hasCharacter = false;
                var sr = ClickedCharacter.GetComponent<SpriteRenderer>();
                if (sr != null) sr.material.color = Color.white;
                ClickedCharacter = null;
            }

            if (ClickedEnemy != null)
            {
                hasEnemy = false;
                var sr = ClickedEnemy.GetComponent<SpriteRenderer>();
                if (sr != null) sr.material.color = Color.white;
                ClickedEnemy = null;
            }
        }

        public void Move()
        {
            (ClickedPlayer.transform.position, ClickedCharacter.transform.position) = (ClickedCharacter.transform.position, ClickedPlayer.transform.position);
            
            var vector3 = ClickedPlayer.transform.position;
            vector3.y += -0.24f;
            ClickedPlayer.transform.position = vector3;
            
            vector3 = ClickedCharacter.transform.position;
            vector3.y += 0.24f;
            ClickedCharacter.transform.position = vector3;
            IsDeployed();
            
            ClickedPlayer.GetComponent<SpriteRenderer>().material.color = Color.white;
            ClickedCharacter.GetComponent<SpriteRenderer>().material.color = Color.white;
            
            ClickedPlayer = null;
            ClickedCharacter = null;
            hasCharacter = false;
            hasPlayer = false;
        }

        private void IsDeployed()
        {
            Debug.Log("isdeployed 실행되었음");
            foreach (var character in characters)
            {
                Vector2 characterPos = character.transform.localPosition;
                if (MinArea.x <= characterPos.x &&
                    MaxArea.x >= characterPos.x &&
                    MinArea.y <= characterPos.y &&
                    MaxArea.y >= characterPos.y)
                {
                    character.layer = 10;
                }
                else
                {
                    character.layer = 0;
                }
            }
        }
    }
}
