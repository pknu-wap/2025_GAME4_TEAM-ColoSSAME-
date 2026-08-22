// AI_Manager.cs

using System;
using System.Collections.Generic;
using BattleK.Scripts.AI;
using UnityEngine;
using Random = UnityEngine.Random;

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
        
        [Header("매니저")]
        [SerializeField] private LeagueSceneManager _leagueSceneManager;
        [SerializeField] private UnitLoadManager _unitLoadManager;
        [SerializeField] private UserSaveManager _userSaveManager;
        //[SerializeField] private EnemySaveManager _enemySaveManager;
        [SerializeField] private League _league;

        public int PlayerLayer { get; private set; } = -1;
        public int EnemyLayer { get; private set; } = -1;

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
                var targetLayer = (sideIndex == 0) ? PlayerLayer : EnemyLayer;
                AssignLayer(go, targetLayer);
            }

            var enemyLayerMaskIndex = (sideIndex == 0) ? EnemyLayer : PlayerLayer;
            unit.TargetLayer = 1 << enemyLayerMaskIndex;

            unit.InjectSaveDependencies(
                unitLoadManager: _unitLoadManager,
                userSaveManager: _userSaveManager,
                enemySaveManager: EnemySaveManager.Instance,
                league: LeagueManager.Instance.league);
            
            if (sideIndex == 0)
            {
                if (!playerUnits.Contains(unit)) playerUnits.Add(unit);
            }
            else
            {
                if (!enemyUnits.Contains(unit)) enemyUnits.Add(unit);
            }
        }
        
        public void UnregisterUnit(StaticAICore unit)
        {
            if (playerUnits.Contains(unit)) playerUnits.Remove(unit);
            if (enemyUnits.Contains(unit)) enemyUnits.Remove(unit);
        }

        private void ResolveLayers()
        {
            PlayerLayer = LayerMask.NameToLayer(playerLayerName);
            EnemyLayer  = LayerMask.NameToLayer(enemyLayerName);
            
            if (PlayerLayer == -1) Debug.LogError($"Layer '{playerLayerName}' 가 없습니다! Project Settings 확인 필요.");
            if (EnemyLayer == -1) Debug.LogError($"Layer '{enemyLayerName}' 가 없습니다! Project Settings 확인 필요.");
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
            switch (playerUnits.Count)
            {
                case >= 1 when enemyUnits.Count >= 1:
                    return;
                case < 1 when enemyUnits.Count >= 1:
                    _leagueSceneManager.OnClickLose();
                    Debug.Log("패배");
                    break;
                case >= 1 when enemyUnits.Count < 1:
                    _leagueSceneManager.OnClickWin();
                    Debug.Log("승리");
                    break;
                case < 1 when enemyUnits.Count < 1:
                    _leagueSceneManager.OnClickDraw();
                    Debug.Log("무승부");
                    break;
            }
            IsAlreadyDone = true;
            KillAll();
        }

        public void KillAll()
        {
            KillPlayers();
            KillEnemies();
        }

        public void KillPlayers()
        {
            for (var i = playerUnits.Count - 1; i >= 0; i--)
            {
                playerUnits[i].OnDead(StaticAICore.DeathReason.System);
            }
        }

        public void KillEnemies()
        {
            for (var i = enemyUnits.Count - 1; i >= 0; i--)
            {
                enemyUnits[i].OnDead(StaticAICore.DeathReason.System);
            }
        }
    }
}
