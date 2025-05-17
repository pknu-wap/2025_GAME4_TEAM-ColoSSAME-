using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Battle.Scripts.Strategy
{
    public class StrategyManager : MonoBehaviour
    {
        public GameObject[] players;
        public GameObject[] characters;
        public Vector3[] transforms;
        public Transform ClickedCharacter;
        public Transform ClickedPlayer;
        public bool hasCharacter;
        public bool hasPlayer;
        
        private void Awake()
        {
            players = GameObject.FindGameObjectsWithTag("Position");
            characters = GameObject.FindGameObjectsWithTag("Player");
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            transforms = new Vector3[players.Length];
            for (int i = 0; i < characters.Length; i++)
            {
                Debug.Log(characters[i].name);
                transforms[i] = characters[i].transform.position;
            }
            OneOneOneOne();
        }

        private void ResetCharacters()
        {
            for (int i = 0; i < characters.Length; i++)
            {
                characters[i].transform.position = transforms[i];
            }
        }
        private void ActivePlayer()
        {
            foreach (var player in players)
            {
                player.SetActive(true);
            }
        }

        private void OneTwoOne()
        {
            ActivePlayer();
            ResetCharacters();
            players[0].transform.position = new Vector2(-2, 0);
            players[1].transform.position = new Vector2(0, 1);
            players[2].transform.position = new Vector2(0, -1);
            players[3].transform.position = new Vector2(2, 0);
        }

        private void OneOneOneOne()
        {
            ActivePlayer();
            ResetCharacters();
            for (int i = 0; i < players.Length; i++)
            {
                players[i].transform.position = new Vector2(i-1.5f,transform.position.y); 
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
            
            ClickedPlayer.GetComponent<SpriteRenderer>().material.color = Color.white;
            ClickedCharacter.GetComponent<SpriteRenderer>().material.color = Color.white;
            
            ClickedPlayer = null;
            ClickedCharacter = null;
            hasCharacter = false;
            hasPlayer = false;
        }
    }
}
