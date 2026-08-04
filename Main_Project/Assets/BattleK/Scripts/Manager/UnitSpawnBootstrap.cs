using System.Collections.Generic;
using BattleK.Scripts.Data;
using BattleK.Scripts.Data.Type;
using BattleK.Scripts.HP;
using BattleK.Scripts.Manager.Strategy.Runtime;
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
        [SerializeField] private FamilyStatsCollector statsCollector;
        [SerializeField] private CalculateManager calculateManager;
        [SerializeField] private HPManager hpManager;
        [SerializeField] private StatWindowManager statWindowManager;
        [SerializeField] private FormationManager formationManager;
        [SerializeField] private LeagueManager leagueManager;

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

        private void Awake()
        {
            _loader = new AddressableUnitLoader();
            _mover = new UnitMover();
            _presentation = new UnitPresentationSetup(playerLayerName, enemyLayerName);
            _spawner = new UnitSpawner(_loader, _presentation, _mover, aiManager);
            _coordinator = new SpawnCompletionCoordinator(_spawner, aiManager, statsCollector, calculateManager, hpManager, statWindowManager);

            _requestBuilder = new BattleFormationRequestBuilder(
                formationManager, _addressBooks,
                PlayerUiScale, EnemyUiScale, PlayerOffset, EnemyOffset,
                PlayerAnimConfig, EnemyAnimConfig);

            if (!leagueManager) leagueManager = FindObjectOfType<LeagueManager>();

            if (_startBattleButton)
            {
                _startBattleButton.onClick.RemoveAllListeners();
                _startBattleButton.onClick.AddListener(OnClickStartBattle);
            }
            _playerBookIndex = leagueManager.league.settings.playerTeamId - 1;
            _enemyBookIndex = leagueManager.league.currentEnemyTeamId - 1;
        }

        private void OnDestroy()
        {
            _coordinator.Dispose();
            _loader.ReleaseAll();
        }

        public void SetEnemyFormationOverride(FormationAsset asset) => _requestBuilder.SetEnemyFormationOverride(asset);

        private void OnClickStartBattle()
        {
            if (_startBattleButton) _startBattleButton.interactable = false;

            if (_uiRootToDisable) _uiRootToDisable.SetActive(false);
            if (_playerUnitsRoot) foreach (Transform t in _playerUnitsRoot) t.gameObject.SetActive(false);
            if (_enemyUnitsRoot) foreach (Transform t in _enemyUnitsRoot) t.gameObject.SetActive(false);

            var requests = new List<UnitSpawnRequest>();
            var assetRefs = new List<AssetReferenceGameObject>();

            _requestBuilder.BuildPlayerRequests(_playerSlots, _playerBookIndex, requests, assetRefs);

            if (EnemyUseFactionStrategy && EnemyFaction[_enemyBookIndex])
                _requestBuilder.BuildEnemyRequests(_enemyBookIndex, EnemyFaction, _playerUnitsRoot, _enemyUnitsRoot, requests, assetRefs);

            _spawner.BeginBatch(requests.Count);
            for (var i = 0; i < requests.Count; i++)
            {
                var req = requests[i];
                var root = req.isPlayer ? _playerUnitsRoot : _enemyUnitsRoot;
                StartCoroutine(_spawner.Spawn(req, assetRefs[i], root, null));
            }
        }
    }
}