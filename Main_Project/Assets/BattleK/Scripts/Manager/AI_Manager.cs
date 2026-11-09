using System;
using System.Collections.Generic;
using BattleK.Scripts.AI;
using BattleK.Scripts.HP;
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
        public List<StaticAICore> enemyUnits = new();

        public bool IsAlreadyDone = false;

        [Header("Layer Settings")]
        public string playerLayerName = "Player";
        public string enemyLayerName = "Enemy";
        public bool forceLayerBySide = true;

        [Header("매니저")]
        [SerializeField] private LeagueSceneManager _leagueSceneManager;
        [SerializeField] private UnitLoadManager _unitLoadManager;
        [SerializeField] private UserSaveManager _userSaveManager;
        [SerializeField] private League _league;
        [SerializeField] private HPManager _hpManager;

        public int PlayerLayer { get; private set; } = -1;
        public int EnemyLayer { get; private set; } = -1;
        public HPManager HPManager => _hpManager;

        private readonly Dictionary<
            int,
            (string Name, bool IsPlayer, long DamageDealt)>
            _damageResults = new();

        public IReadOnlyDictionary<
            int,
            (string Name, bool IsPlayer, long DamageDealt)>
            BattleDamageResults => _damageResults;

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
            unit.AiManager = this;
            var go = unit.gameObject;

            if (forceLayerBySide)
            {
                var targetLayer = sideIndex == 0
                    ? PlayerLayer
                    : EnemyLayer;

                AssignLayer(go, targetLayer);
            }

            var enemyLayerMaskIndex = sideIndex == 0
                ? EnemyLayer
                : PlayerLayer;

            unit.TargetLayer = 1 << enemyLayerMaskIndex;

            unit.InjectSaveDependencies(
                unitLoadManager: _unitLoadManager,
                userSaveManager: _userSaveManager,
                enemySaveManager: EnemySaveManager.Instance,
                league: LeagueManager.Instance.league);

            if (sideIndex == 0)
            {
                if (!playerUnits.Contains(unit))
                    playerUnits.Add(unit);
            }
            else
            {
                if (!enemyUnits.Contains(unit))
                    enemyUnits.Add(unit);
            }

            _hpManager?.setUnits();
        }

        public void UnregisterUnit(StaticAICore unit)
        {
            playerUnits.Remove(unit);
            enemyUnits.Remove(unit);
        }

        public void RecordUnitDamage(
            int unitId,
            string unitName,
            bool isPlayer,
            long damageDealt)
        {
            _damageResults[unitId] = (
                Name: unitName,
                IsPlayer: isPlayer,
                DamageDealt: damageDealt);
        }

        public void LogDamageResults()
        {
            Debug.Log("========== 전투 피해량 ==========");
            foreach (var result in _damageResults.Values)
            {
                string team = result.IsPlayer ? "아군" : "적군";
                Debug.Log($"[{team}] {result.Name} | " + $"입힌 피해량: {result.DamageDealt:N0}");
            }
            Debug.Log("=================================");
        }
        public void ResetBattleResults()
        {
            _damageResults.Clear();
            IsAlreadyDone = false;
        }

        private void ResolveLayers()
        {
            PlayerLayer = LayerMask.NameToLayer(playerLayerName);
            EnemyLayer = LayerMask.NameToLayer(enemyLayerName);

            if (PlayerLayer == -1)
            {
                Debug.LogError(
                    $"Layer '{playerLayerName}' 가 없습니다! " +
                    "Project Settings 확인 필요.");
            }

            if (EnemyLayer == -1)
            {
                Debug.LogError(
                    $"Layer '{enemyLayerName}' 가 없습니다! " +
                    "Project Settings 확인 필요.");
            }
        }

        private void AssignLayer(GameObject go, int layerIndex)
        {
            if (layerIndex < 0) return;

            SetLayerRecursively(go.transform, layerIndex);
        }

        private void SetLayerRecursively(Transform t, int layerIndex)
        {
            t.gameObject.layer = layerIndex;

            foreach (Transform child in t)
            {
                SetLayerRecursively(child, layerIndex);
            }
        }

        public void IsWinner()
        {
            if (IsAlreadyDone) return;

            if (playerUnits.Count > 0 && enemyUnits.Count > 0) return;
            var hasPlayers = playerUnits.Count > 0;
            var hasEnemies = enemyUnits.Count > 0;

            IsAlreadyDone = true;
            KillAll();
            LogDamageResults();
            switch (hasPlayers)
            {
                case true when !hasEnemies:
                    Debug.Log("승리");
                    _leagueSceneManager.OnClickWin();
                    break;
                case false when hasEnemies:
                    Debug.Log("패배");
                    _leagueSceneManager.OnClickLose();
                    break;
                default:
                    Debug.Log("무승부");
                    _leagueSceneManager.OnClickDraw();
                    break;
            }
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
                playerUnits[i].OnDead(
                    StaticAICore.DeathReason.System);
            }
        }

        public void KillEnemies()
        {
            for (var i = enemyUnits.Count - 1; i >= 0; i--)
            {
                enemyUnits[i].OnDead(
                    StaticAICore.DeathReason.System);
            }
        }
    }
}