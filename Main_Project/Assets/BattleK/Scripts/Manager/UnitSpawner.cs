using System;
using System.Collections;
using BattleK.Scripts.AI;
using BattleK.Scripts.Data;
using BattleK.Scripts.Data.Type;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace BattleK.Scripts.Manager
{
    public class UnitSpawner
    {
        public event Action OnAllSpawnsComplete;

        private int _pendingSpawns;

        private readonly AddressableUnitLoader _loader;
        private readonly UnitPresentationSetup _presentation;
        private readonly UnitMover _mover;
        private readonly AI_Manager _aiManager;

        public UnitSpawner(AddressableUnitLoader loader, UnitPresentationSetup presentation, UnitMover mover, AI_Manager aiManager)
        {
            _loader = loader;
            _presentation = presentation;
            _mover = mover;
            _aiManager = aiManager;
        }

        public void BeginBatch(int totalRequests)
        {
            _pendingSpawns = totalRequests;
        }

        public IEnumerator Spawn(UnitSpawnRequest req, AssetReferenceGameObject ar, Transform root, Action onComplete)
        {
            GameObject go = null;
            yield return _loader.LoadOrGetAsync(req.logicalKey, ar, root, result => go = result);

            if (go == null)
            {
                onComplete?.Invoke();
                DecrementAndCheck();
                yield break;
            }

            _presentation.Apply(go, req);

            var aiCore = go.GetComponent<StaticAICore>();
            var sideIndex = req.isPlayer ? 0 : 1;
            _aiManager.RegisterUnit(aiCore, sideIndex);

            var moveTime = req.duration > 0f ? req.duration : UnitMover.DefaultDuration;
            yield return _mover.MoveTo(go.transform, req.startPos, req.endPos, moveTime);

            onComplete?.Invoke();
            DecrementAndCheck();
        }

        private void DecrementAndCheck()
        {
            _pendingSpawns--;
            if (_pendingSpawns <= 0)
                OnAllSpawnsComplete?.Invoke();
        }
    }
}