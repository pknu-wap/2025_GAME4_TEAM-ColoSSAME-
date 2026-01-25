// AI_Manager.cs

using System;
using System.Collections.Generic;
using BattleK.Scripts.AI;
using UnityEngine;

namespace BattleK.Scripts.Manager
{
    public class AI_Manager : MonoBehaviour
    {
        public static AI_Manager Instance { get; private set; }
        public static event Action<AI_Manager> OnReady;

        [Header("Roots")]
        [Tooltip("0: Player 부모 Transform, 1: Enemy 부모 Transform")]
        public List<Transform> unitPool = new();

        [Header("Lists")]
        public List<StaticAICore> playerUnits = new();
        public List<StaticAICore> enemyUnits  = new();

        public bool IsAlreadyDone = false;
        
        [Header("Layer Settings")]
        public string playerLayerName = "Player";
        public string enemyLayerName  = "Enemy";
        public bool forceLayerBySide = true;
        
        [Header("리그매니저")]
        [SerializeField] private LeagueSceneManager _leagueSceneManager;

        private int _playerLayer = -1;
        private int _enemyLayer  = -1;
        
        private void Awake()
        {
            if (Instance && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            ResolveLayers();
        }

        private void OnEnable()
        {
            OnReady?.Invoke(this);
        }

        public void RegisterUnit(StaticAICore unit, int sideIndex)
        {
            if (!unit) return;

            if (unitPool.Count > sideIndex && unitPool[sideIndex])
            {
                unit.transform.SetParent(unitPool[sideIndex]);
            }

            var go = unit.gameObject;
            if (forceLayerBySide)
            {
                var targetLayer = (sideIndex == 0) ? _playerLayer : _enemyLayer;
                AssignLayer(go, targetLayer);
            }

            var enemyLayerMaskIndex = (sideIndex == 0) ? _enemyLayer : _playerLayer;
            unit.TargetLayer = 1 << enemyLayerMaskIndex;

            if (sideIndex == 0)
            {
                if (!playerUnits.Contains(unit)) playerUnits.Add(unit);
            }
            else
            {
                if (!enemyUnits.Contains(unit)) enemyUnits.Add(unit);
            }
            
            unit.Initialize();
        }
        
        public void UnregisterUnit(StaticAICore unit)
        {
            if (playerUnits.Contains(unit)) playerUnits.Remove(unit);
            if (enemyUnits.Contains(unit)) enemyUnits.Remove(unit);
        }

        private void ResolveLayers()
        {
            _playerLayer = LayerMask.NameToLayer(playerLayerName);
            _enemyLayer  = LayerMask.NameToLayer(enemyLayerName);
            
            if (_playerLayer == -1) Debug.LogError($"Layer '{playerLayerName}' 가 없습니다! Project Settings 확인 필요.");
            if (_enemyLayer == -1) Debug.LogError($"Layer '{enemyLayerName}' 가 없습니다! Project Settings 확인 필요.");
        }

        private void AssignLayer(GameObject go, int layerIndex)
        {
            if (layerIndex < 0) return;
            SetLayerRecursively(go.transform, layerIndex);
        }

        private void SetLayerRecursively(Transform t, int layerIndex)
        {
            t.gameObject.layer = layerIndex;
            foreach (Transform c in t)
                SetLayerRecursively(c, layerIndex);
        }

        public void IsWinner()
        {
            if (playerUnits.Count < 1)
            {
                _leagueSceneManager.OnClickLose();
                Debug.Log("패배");
                IsAlreadyDone = true;
                KillAll();
            }

            if (enemyUnits.Count < 1)
            {
                _leagueSceneManager.OnClickWin();
                Debug.Log("승리");
                IsAlreadyDone = true;
                KillAll();
            }
        }

        private void KillAll()
        {
            foreach (var player in playerUnits)
            {
                player.OnDead();
            }
            foreach (var enemy in enemyUnits)
            {
                enemy.OnDead();
            }
        }
    }
}
