using System;
using System.Collections;
using BattleK.Scripts.HP;
using BattleK.Scripts.UI;
using UnityEngine;

namespace BattleK.Scripts.Manager
{
    public class SpawnCompletionCoordinator : IDisposable
    {
        private readonly UnitSpawner _spawner;
        private readonly HPManager _hpManager;
        private readonly StatWindowManager _statWindowManager;
        private readonly CalculateManager _calculateManager;
        private readonly StatAdaptManager _statAdaptManager;
        private readonly MonoBehaviour _coroutineRunner;
        private Coroutine _initializationCoroutine;
        private bool _disposed;

        public SpawnCompletionCoordinator(
            UnitSpawner spawner,
            HPManager hpManager,
            StatWindowManager statWindowManager,
            CalculateManager calculateManager,
            StatAdaptManager statAdaptManager,
            MonoBehaviour coroutineRunner)
        {
            _spawner = spawner;
            _hpManager = hpManager;
            _statWindowManager = statWindowManager;
            _calculateManager = calculateManager;
            _statAdaptManager = statAdaptManager;
            _coroutineRunner = coroutineRunner;

            _spawner.OnAllSpawnsComplete += HandleAllSpawnsComplete;
            _spawner.OnBatchFailed += HandleBatchFailed;
        }

        private void HandleAllSpawnsComplete()
        {
            if (_disposed || _initializationCoroutine != null) return;
            if (!ValidateDependencies()) return;

            _initializationCoroutine = _coroutineRunner.StartCoroutine(RunAfterStatsReady());
        }

        private void HandleBatchFailed(int failedCount)
        {
            if (_disposed) return;
            Debug.LogError($"[SpawnCompletionCoordinator] 유닛 스폰 {failedCount}건이 실패하여 전투 초기화를 중단했습니다.");
        }

        private bool ValidateDependencies()
        {
            if (!_coroutineRunner || !_calculateManager || !_statAdaptManager || !_hpManager || !_statWindowManager) return false;
            return _coroutineRunner.gameObject.activeInHierarchy;
        }

        private IEnumerator RunAfterStatsReady()
        {
            try
            {
                yield return _calculateManager.RefreshFromCollectorAndWait();
                if (_disposed) yield break;
                _statAdaptManager.ApplyToAllUnitsAndInitialize();
                _hpManager.setUnits();
                _statWindowManager.SetStrategyList();
                _hpManager.ApplyHpToHPBar();
            }
            finally
            {
                _initializationCoroutine = null;
            }
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            _spawner.OnAllSpawnsComplete -= HandleAllSpawnsComplete;
            _spawner.OnBatchFailed -= HandleBatchFailed;
            if (_initializationCoroutine != null && _coroutineRunner)
            {
                _coroutineRunner.StopCoroutine(_initializationCoroutine);
            }
            _initializationCoroutine = null;
        }
    }
}
