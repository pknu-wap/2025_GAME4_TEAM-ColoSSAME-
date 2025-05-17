using System;
using System.Collections.Generic;
using Battle.Scripts.Ai;
using Battle.Scripts.Strategy;
using UnityEngine;

namespace Battle.Scripts.Value
{
    public class DataManager : MonoBehaviour
    {
        public GameObject[] Players;
        public Vector3[] playersTransform;
        public StrategyManager strategy;
        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
            strategy = FindObjectOfType<StrategyManager>();
            GetStrategy();
        }

        public void GetStrategy()
        {
            for (int i = 0; i < strategy.players.Length; i++)
            {
                Players[i] = strategy.players[i];
                playersTransform[i] = strategy.transforms[i];
            }
        }
    }
}
