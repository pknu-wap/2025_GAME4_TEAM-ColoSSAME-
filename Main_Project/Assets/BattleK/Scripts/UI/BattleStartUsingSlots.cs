using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BattleK.Scripts.AI;
using BattleK.Scripts.Data;
using BattleK.Scripts.Data.Type;
using BattleK.Scripts.HP;
using BattleK.Scripts.Manager;
using BattleK.Scripts.Manager.Strategy.Runtime;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;

namespace BattleK.Scripts.UI
{
    public class BattleStartUsingSlots : MonoBehaviour
    {
        [Header("전투 준비 완료 시 스탯 수집")]
        [SerializeField] private FamilyStatsCollector _statsCollector;
        
        [Header("Manager")]
        [SerializeField] private AI_Manager _aiManager;
        [SerializeField] private HPManager _hpManager;
        [SerializeField] private StatWindowManager _statWindowManager;
        [SerializeField] private CalculateManager _calculateManager;
        [SerializeField] private FormationManager _formationManager;
        
        [Header("버튼")]
        [SerializeField] private Button _startBattleButton;

        [Header("배틀 유닛 루트")]
        [Tooltip("플레이어 유닛이 배치될 월드 트랜스폼 루트")]
        [SerializeField] private Transform _playerUnitsRoot;
        [Tooltip("적 유닛이 배치될 월드 트랜스폼 루트 (선택)")]
        [SerializeField] private Transform _enemyUnitsRoot;

        [Header("양 팀 슬롯 (신규)")]
        [Tooltip("플레이어 측 UI 슬롯들")]
        [SerializeField] private Slot[] _playerSlots;
        [Tooltip("적 측 UI 슬롯들 (옵션: 비워두면 '가문 전략' 자동 배치 사용)")]
        [SerializeField] private Slot[] _enemySlots;

        [Header("Configuration")]
        [SerializeField] private float PlayerUiScale = 0.01f;
        [SerializeField] private float EnemyUiScale = 0.01f;
        [SerializeField] private Vector3 PlayerOffset = Vector3.zero;
        [SerializeField] private Vector3 EnemyOffset = Vector3.zero;
        [SerializeField] private float MoveDuration = 0.25f;


        [Header("배틀 시작 시 비활성화할 UI 루트(버튼 부모 등)")]
        [SerializeField] private GameObject _uiRootToDisable;

        [Header("Addressable 모드: 매핑 SO 리스트")]
        [SerializeField] private List<CharacterAddressBook> _addressBooks = new();
        [SerializeField] private int _playerBookIndex;
        [SerializeField] private int _enemyBookIndex = 1;

        [Header("Formation Animation (Player/Enemy)")]
        [SerializeField] private FormationAnimConfig PlayerAnimConfig;
        [SerializeField] private FormationAnimConfig EnemyAnimConfig;

        [Header("적 자동 배치: 가문 전략")]
        public bool EnemyUseFactionStrategy = true;
        public EnemyFactionConfig EnemyFaction;
        private FormationAsset _enemyFormationOverride;

        
        private readonly Dictionary<string, GameObject> playerInstances = new();
        private readonly Dictionary<string, GameObject> enemyInstances  = new();
        private int pendingSpawns;
        private List<Vector3> lastPlayerPositions = new();

        private void Awake()
        {
            if (!_startBattleButton) return;
            _startBattleButton.onClick.RemoveAllListeners();
            _startBattleButton.onClick.AddListener(OnClickStartBattle);
        }

        private void OnClickStartBattle()
        {
            if (_startBattleButton) _startBattleButton.interactable = false;
            
            pendingSpawns = 1;
            lastPlayerPositions.Clear();
            if (_uiRootToDisable) _uiRootToDisable.SetActive(false);
            _uiRootToDisable.SetActive(false);
            
            if (_playerUnitsRoot) foreach (Transform t in _playerUnitsRoot) t.gameObject.SetActive(false);
            if (_enemyUnitsRoot) foreach (Transform t in _enemyUnitsRoot) t.gameObject.SetActive(false);
            
            SpawnPlayerTeam();
            
            if (EnemyUseFactionStrategy && EnemyFaction)
            {
                SpawnEnemyTeam();
            }
        }
        
        private void SpawnPlayerTeam()
        {
            var positions = _formationManager.CalculatePlayerPositions(_playerSlots, PlayerUiScale, PlayerOffset);
            lastPlayerPositions = positions;

            if (positions.Count == 0) return;

            var center = GetCenter(positions);
            var startPosBase = PlayerAnimConfig.useAnim ? PlayerAnimConfig.startCenter : center;
            var endPosBase   = PlayerAnimConfig.useAnim ? PlayerAnimConfig.endCenter : center;
            var duration       = PlayerAnimConfig.useAnim ? PlayerAnimConfig.travelTime : 0f;

            var posIndex = 0;
            foreach (var t in _playerSlots)
            {
                if (!t.IsOccupied) continue;
                if (posIndex >= positions.Count) break;

                var key = t.Occupant.GetComponent<CharacterID>().characterKey;
                var finalPos = positions[posIndex++];
                var offset = finalPos - center;

                RequestSpawn(new UnitSpawnRequest {
                    logicalKey = key,
                    startPos = startPosBase + offset,
                    endPos = endPosBase + offset,
                    faceLeft = true,
                    duration = duration,
                    isPlayer = true
                });
            }
        }
        
        private void SpawnEnemyTeam()
        {
            var keys = EnemyFaction.PickRosterKeys();
            if (keys == null || keys.Count == 0) return;

            List<Vector3> positions;
            
            if (_enemyFormationOverride)
            {
                positions = _formationManager.CalculatePositionsFromAsset(
                    _enemyFormationOverride,
                    EnemyUiScale,
                    EnemyOffset,
                    EnemyAnimConfig.endCenter
                );
                Debug.Log("[BattleStart] 오버라이드된 포메이션으로 적 배치 계산됨");
            }
            else
            {
                var playerPosInEnemySpace = new List<Vector3>();
                if (_enemyUnitsRoot && _playerUnitsRoot)
                {
                    playerPosInEnemySpace.AddRange(lastPlayerPositions.Select(pLocal => _playerUnitsRoot.TransformPoint(pLocal)).Select(world => _enemyUnitsRoot.InverseTransformPoint(world)));
                }
            
                positions = _formationManager.CalculateEnemyPositions(
                    EnemyFaction, keys.Count, playerPosInEnemySpace, 
                    EnemyUiScale, EnemyOffset, EnemyAnimConfig.endCenter
                );
            }
            var centerEnd = EnemyAnimConfig.endCenter;
            var centerStart = EnemyAnimConfig.useAnim ? EnemyAnimConfig.startCenter : centerEnd;
            var duration = EnemyAnimConfig.useAnim ? EnemyAnimConfig.travelTime : 0f;

            if (positions == null) return;
            for (var i = 0; i < Mathf.Min(keys.Count, positions.Count); i++)
            {
                var key = keys[i];
                var finalPos = positions[i];
                var offset = finalPos - centerEnd;

                RequestSpawn(new UnitSpawnRequest
                {
                    logicalKey = key,
                    startPos = centerStart + offset,
                    endPos = centerEnd + offset,
                    faceLeft = false,
                    duration = duration,
                    isPlayer = false
                });
            }
        }
        
        public void SetEnemyFormationOverride(FormationAsset asset)
        {
            _enemyFormationOverride = asset;
        }
        
        private void RequestSpawn(UnitSpawnRequest req)
        {
            var book = req.isPlayer ? GetBookOrNull(_playerBookIndex) : ResolveEnemyBook();
            var root = req.isPlayer ? _playerUnitsRoot : _enemyUnitsRoot;
            
            if (!book || !book.TryGet(req.logicalKey, out var ar)) return;

            pendingSpawns++;

            var instanceMap = req.isPlayer ? playerInstances : enemyInstances;

            CoroutineRunner.Run(SpawnRoutine(req, ar, root, instanceMap, () => {
                pendingSpawns--;
                CheckSpawnComplete();
            }));
        }
        
        private IEnumerator SpawnRoutine(UnitSpawnRequest req, AssetReferenceGameObject ar, Transform root, Dictionary<string, GameObject> cache, Action onComplete)
        {
            if (!cache.TryGetValue(req.logicalKey, out var go))
            {
                var handle = ar.InstantiateAsync(root);
                yield return handle;
                if (handle.Status == AsyncOperationStatus.Succeeded)
                {
                    go = handle.Result;
                    go.name = req.logicalKey;
                    cache[req.logicalKey] = go;
                }
                else
                {
                    Debug.LogError($"Failed to spawn {req.logicalKey}");
                    onComplete?.Invoke();
                    yield break;
                }
            }
            
            go.SetActive(false);
            var layerName = req.isPlayer ? _aiManager.playerLayerName : _aiManager.enemyLayerName;
            var layerIndex = LayerMask.NameToLayer(layerName);
            SetLayerRecursively(go, layerIndex);
            go.SetActive(true);

            var aiCore = go.GetComponent<StaticAICore>();
            var sideIndex = req.isPlayer ? 0 : 1;
            _aiManager.RegisterUnit(aiCore, sideIndex);
            
            var s = go.transform.localScale;
            s.x = req.faceLeft ? -Mathf.Abs(s.x) : Mathf.Abs(s.x);
            go.transform.localScale = s;
            go.transform.localPosition = req.startPos;

            var moveTime = (req.duration > 0) ? req.duration : MoveDuration;
            if (moveTime > 0)
            {
                var elapsed = 0f;
                while (elapsed < moveTime)
                {
                    elapsed += Time.deltaTime;
                    go.transform.localPosition = Vector3.Lerp(req.startPos, req.endPos, elapsed / moveTime);
                    yield return null;
                }
            }
            go.transform.localPosition = req.endPos;

            onComplete?.Invoke();
        }
        
        public void CheckSpawnComplete()
        {
            if (pendingSpawns <= 0)
            {
                CoroutineRunner.Run(NotifyManagersRoutine());
            }
        }
        
        private IEnumerator NotifyManagersRoutine()
        {
            yield return new WaitUntil(() => _aiManager.playerUnits.Count > 0 && _aiManager.enemyUnits.Count > 0);

            _statsCollector.CollectFromBothTeams();
            _calculateManager.RefreshFromCollectorOnce();
            _hpManager.setUnits();
            _statWindowManager.SetStrategyList();
            _statWindowManager.ApplyStatWindow();
            _hpManager.ApplyHpToHPBar();
        }
        private CharacterAddressBook GetBookOrNull(int idx) => (idx >= 0 && idx < _addressBooks.Count) ? _addressBooks[idx] : null;
        private CharacterAddressBook ResolveEnemyBook() => (EnemyFaction?.addressBookOverride) ? EnemyFaction.addressBookOverride : GetBookOrNull(_enemyBookIndex);
        private Vector3 GetCenter(List<Vector3> list) => list.Aggregate(Vector3.zero, (a, b) => a + b) / list.Count;
        
        private void SetLayerRecursively(GameObject obj, int newLayer)
        {
            if (!obj) return;
            obj.layer = newLayer;
            foreach (Transform child in obj.transform)
            {
                SetLayerRecursively(child.gameObject, newLayer);
            }
        }
    }
}