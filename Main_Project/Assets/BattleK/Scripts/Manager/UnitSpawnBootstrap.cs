using System.Collections.Generic;
using System.Linq;
using BattleK.Scripts.Data;
using BattleK.Scripts.Data.Stat;
using BattleK.Scripts.Data.Type;
using BattleK.Scripts.HP;
using BattleK.Scripts.Manager.Strategy.Runtime;
using BattleK.Scripts.Manager.Strategy.Runtime.PlayerStrategy;
using BattleK.Scripts.UI;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;

namespace BattleK.Scripts.Manager
{
    public class UnitSpawnBootstrap : MonoBehaviour
    {
        [Header("버튼")]
        [SerializeField] private Button _startBattleButton;
        [SerializeField] private GameObject _uiRootToDisable;

        [Header("Manager")]
        [SerializeField] private AI_Manager aiManager;
        [SerializeField] private HPManager hpManager;
        [SerializeField] private StatWindowManager statWindowManager;
        [SerializeField] private FormationManager formationManager;
        [SerializeField] private CalculateManager calculateManager;
        [SerializeField] private StatAdaptManager statAdaptManager;

        [Header("스탯 계산")]
        [SerializeField] private StatCorrectionTable correctionTable;

        [Tooltip("직업(UnitClass)별 MoveSpeed/AttackSpeed/AttackDelay 고정값 테이블")]
        [SerializeField] private ClassBaseStatTable classBaseStatTable;

        [Header("배틀 유닛 루트")]
        [SerializeField] private Transform _playerUnitsRoot;
        [SerializeField] private Transform _enemyUnitsRoot;

        [Header("슬롯")]
        [SerializeField] private Slot[] _playerSlots;

        [Header("Configuration")]
        [SerializeField] private string playerLayerName = "Player";
        [SerializeField] private string enemyLayerName = "Enemy";
        [SerializeField] private float PlayerUiScale = 0.01f;
        [SerializeField] private float EnemyUiScale = 0.01f;
        [SerializeField] private Vector3 PlayerOffset = Vector3.zero;
        [SerializeField] private Vector3 EnemyOffset = Vector3.zero;

        [Header("Addressable 매핑")]
        [SerializeField] private List<CharacterAddressBook> _addressBooks = new();
        [SerializeField] private int _playerBookIndex;
        [SerializeField] private int _enemyBookIndex;

        [Header("Formation Animation")]
        [SerializeField] private FormationAnimConfig PlayerAnimConfig;
        [SerializeField] private FormationAnimConfig EnemyAnimConfig;

        [Header("적 자동 배치")]
        public bool EnemyUseFactionStrategy = true;
        public List<EnemyFactionConfig> EnemyFaction = new();
        private AddressableUnitLoader _loader;
        private UnitMover _mover;
        private UnitPresentationSetup _presentation;
        private UnitSpawner _spawner;
        private SpawnCompletionCoordinator _coordinator;
        private BattleFormationRequestBuilder _requestBuilder;
        private FormationAsset _enemyFormationOverride;
        private bool _battleStarted;

        private void Awake()
        {
            InitializeSpawnServices();
            InitializeRequestBuilder();
            if (_startBattleButton)
            {
                _startBattleButton.onClick.AddListener(OnClickStartBattle);
            }
        }

        private void OnDestroy()
        {
            if (_startBattleButton)
            {
                _startBattleButton.onClick.RemoveListener(OnClickStartBattle);
            }
            _coordinator?.Dispose();
            _loader?.ReleaseAll();
        }

        public void SetEnemyFormationOverride(FormationAsset asset)
        {
            _enemyFormationOverride = asset;
            _requestBuilder?.SetEnemyFormationOverride(asset);
        }

        private void InitializeSpawnServices()
        {
            _loader = new AddressableUnitLoader();
            _mover = new UnitMover();
            _presentation = new UnitPresentationSetup(playerLayerName, enemyLayerName);
            _spawner = new UnitSpawner(_loader, _presentation, _mover, aiManager, correctionTable, classBaseStatTable, playerLayerName, enemyLayerName);
            _coordinator = new SpawnCompletionCoordinator(_spawner, hpManager, statWindowManager, calculateManager, statAdaptManager, this);
        }

        private void InitializeRequestBuilder()
        {
            _requestBuilder = new BattleFormationRequestBuilder(
                formationManager,
                _addressBooks,
                PlayerUiScale,
                EnemyUiScale,
                PlayerOffset,
                EnemyOffset,
                PlayerAnimConfig,
                EnemyAnimConfig);
            _requestBuilder.SetEnemyFormationOverride(_enemyFormationOverride);
        }

        private void OnClickStartBattle()
        {
            if (_battleStarted) return;
            if (!ValidateBattleSetup()) return;
            if (!TryUpdateTeamIndices()) return;
            if (EnemyUseFactionStrategy && !ValidateEnemyFaction()) return;
            var requests = new List<UnitSpawnRequest>();
            var assetRefs = new List<AssetReferenceGameObject>();
            _requestBuilder.BuildPlayerRequests(_playerSlots, _playerBookIndex, requests, assetRefs);
            if (requests.Count == 0) return;
            if (EnemyUseFactionStrategy)
            {
                if (EnemySaveManager.Instance == null) return;
                var allowedEnemyIds = GetCurrentEnemyRosterIds();
                var playerRequestCount = requests.Count;
                _requestBuilder.BuildEnemyRequests(_enemyBookIndex, EnemyFaction, _playerUnitsRoot, _enemyUnitsRoot, requests, assetRefs, allowedEnemyIds);
                if (requests.Count == playerRequestCount) return;
            }
            _battleStarted = true;
            PrepareBattlePresentation();
            SpawnUnits(requests, assetRefs);
        }

        private bool ValidateBattleSetup()
        {
            if (_requestBuilder == null || _spawner == null) return false;
            if (!formationManager) return false;
            if (_playerSlots == null || _playerSlots.Length == 0) return false;
            return _playerUnitsRoot && (!EnemyUseFactionStrategy || _enemyUnitsRoot);
        }

        private bool TryUpdateTeamIndices()
        {
            var leagueManager = LeagueManager.Instance;
            if (!leagueManager || leagueManager.league == null) return false;
            _playerBookIndex = leagueManager.league.settings.playerTeamId - 1;
            _enemyBookIndex = leagueManager.league.currentEnemyTeamId - 1;
            return _addressBooks != null && _playerBookIndex >= 0 && _playerBookIndex < _addressBooks.Count;
        }

        private bool ValidateEnemyFaction()
        {
            if (EnemyFaction == null || _enemyBookIndex < 0 || _enemyBookIndex >= EnemyFaction.Count) return false;
            return EnemyFaction[_enemyBookIndex];
        }

        private void PrepareBattlePresentation()
        {
            if (_startBattleButton) _startBattleButton.interactable = false;
            if (_uiRootToDisable) _uiRootToDisable.SetActive(false);

            DisableChildren(_playerUnitsRoot);
            DisableChildren(_enemyUnitsRoot);
        }

        private void DisableChildren(Transform root)
        {
            if (!root) return;
            foreach (Transform child in root)
            {
                child.gameObject.SetActive(false);
            }
        }

        private void SpawnUnits(List<UnitSpawnRequest> requests, List<AssetReferenceGameObject> assetRefs)
        {
            _spawner.BeginBatch(requests.Count);
            for (var i = 0; i < requests.Count; i++)
            {
                var request = requests[i];
                var root = request.isPlayer ? _playerUnitsRoot : _enemyUnitsRoot;
                StartCoroutine(_spawner.Spawn(request, assetRefs[i], root, null));
            }
        }

        private List<string> GetCurrentEnemyRosterIds()
        {
            var teamId = LeagueManager.Instance.league.currentEnemyTeamId;
            var team = EnemySaveManager.Instance.GetTeam(teamId);
            return team?.units?.Select(unit => unit.Id).ToList() ?? new List<string>();
        }
    }
}