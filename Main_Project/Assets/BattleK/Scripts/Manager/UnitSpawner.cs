using System;
using System.Collections;
using BattleK.Scripts.AI;
using BattleK.Scripts.Data;
using BattleK.Scripts.Data.Stat;
using BattleK.Scripts.Data.Type;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace BattleK.Scripts.Manager
{
    public class UnitSpawner
    {
        private readonly AddressableUnitLoader _loader;
        private readonly UnitPresentationSetup _presentation;
        private readonly UnitMover _mover;
        private readonly AI_Manager _aiManager;
        private readonly StatCorrectionTable _correctionTable;
        private readonly ClassBaseStatTable _classBaseStatTable;
        private readonly string _playerLayerName;
        private readonly string _enemyLayerName;

        public event Action OnAllSpawnsComplete;
        private int _pending;

        public UnitSpawner(
            AddressableUnitLoader loader,
            UnitPresentationSetup presentation,
            UnitMover mover,
            AI_Manager aiManager,
            StatCorrectionTable correctionTable,
            ClassBaseStatTable classBaseStatTable,
            string playerLayerName,
            string enemyLayerName)
        {
            _loader = loader;
            _presentation = presentation;
            _mover = mover;
            _aiManager = aiManager;
            _correctionTable = correctionTable;
            _classBaseStatTable = classBaseStatTable;
            _playerLayerName = playerLayerName;
            _enemyLayerName = enemyLayerName;
        }

        public IEnumerator Spawn(UnitSpawnRequest req, AssetReferenceGameObject assetRef, Transform root, Action<GameObject> onSpawned)
        {
            GameObject spawnedInstance = null;

            yield return _loader.LoadOrGetAsync(req.logicalKey, assetRef, root, instance =>
            {
                spawnedInstance = instance;
                if (instance == null) return;

                var aiCore = instance.GetComponent<StaticAICore>();
                if (aiCore != null)
                {
                    ApplyStatsOrFallback(aiCore, req);
                    var targetLayerName = req.isPlayer ? _enemyLayerName : _playerLayerName;
                    aiCore.TargetLayer = LayerMask.GetMask(targetLayerName);
                    aiCore.SetInitialStats();
                    aiCore.Initialize();
                }
                _presentation.Apply(instance, req);
            });

            if (spawnedInstance == null) yield break;

            yield return _mover.MoveTo(spawnedInstance.transform, req.startPos, req.endPos, req.duration);

            onSpawned?.Invoke(spawnedInstance);

            var core = spawnedInstance.GetComponent<StaticAICore>();
            if (req.isPlayer) _aiManager.playerUnits.Add(core);
            else _aiManager.enemyUnits.Add(core);

            _pending--;
            if (_pending <= 0) OnAllSpawnsComplete?.Invoke();
        }

        private void ApplyStatsOrFallback(StaticAICore aiCore, UnitSpawnRequest req)
        {
            var baseStat = UnitBaseStatProvider.Get(req.logicalKey, req.isPlayer);
            if (baseStat != null)
            {
                StatCalculator.ApplyTo(aiCore.runtimeStat, baseStat, _correctionTable, _classBaseStatTable);
                return;
            }
        }

        public void BeginBatch(int count) => _pending = count;
    }
}