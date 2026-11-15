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
        private readonly string _playerLayerName;
        private readonly string _enemyLayerName;
        private int _pending;
        private int _failedCount;
        private bool _batchActive;

        public event Action OnAllSpawnsComplete;
        public event Action<int> OnBatchFailed;

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
            _playerLayerName = playerLayerName;
            _enemyLayerName = enemyLayerName;
        }

        public void BeginBatch(int count)
        {
            _pending = count;
            _failedCount = 0;
            _batchActive = true;
        }

        public IEnumerator Spawn(UnitSpawnRequest req, AssetReferenceGameObject assetRef, Transform root, Action<GameObject> onSpawned)
        {
            if (!_batchActive) yield break;
            var succeeded = false;
            try
            {
                GameObject spawnedInstance = null;
                yield return _loader.LoadOrGetAsync(req.logicalKey, assetRef, root, instance =>
                {
                    spawnedInstance = instance;
                });
                if (!spawnedInstance) yield break;
                if (!spawnedInstance.TryGetComponent<StaticAICore>(out var core))
                {
                    spawnedInstance.SetActive(false);
                    yield break;
                }
                var targetLayerName = req.isPlayer ? _enemyLayerName : _playerLayerName;
                core.TargetLayer = LayerMask.GetMask(targetLayerName);
                _presentation.Apply(spawnedInstance, req);
                yield return _mover.MoveTo(spawnedInstance.transform, req.startPos, req.endPos, req.duration);
                if (!spawnedInstance || !core) yield break;
                RegisterUnit(core, req.isPlayer);
                onSpawned?.Invoke(spawnedInstance);
                succeeded = true;
            }
            finally
            {
                CompleteRequest(succeeded);
            }
        }

        private void RegisterUnit(StaticAICore core, bool isPlayer)
        {
            if (isPlayer)
            {
                if (!_aiManager.playerUnits.Contains(core)) _aiManager.playerUnits.Add(core);
                return;
            }
            if (!_aiManager.enemyUnits.Contains(core)) _aiManager.enemyUnits.Add(core);
        }

        private void CompleteRequest(bool succeeded)
        {
            if (!_batchActive) return;
            if (!succeeded) _failedCount++;
            _pending--;
            if (_pending > 0) return;
            _batchActive = false;
            if (_failedCount > 0)
            {
                OnBatchFailed?.Invoke(_failedCount);
                return;
            }
            OnAllSpawnsComplete?.Invoke();
        }
    }
}