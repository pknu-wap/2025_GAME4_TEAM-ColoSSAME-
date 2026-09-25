using BattleK.Scripts.HP;
using BattleK.Scripts.UI;
using UnityEngine;

namespace BattleK.Scripts.Manager
{
    public class SpawnCompletionCoordinator
    {
        private readonly UnitSpawner _spawner;
        private readonly HPManager _hpManager;
        private readonly StatWindowManager _statWindowManager;
        private readonly CalculateManager _calculateManager;
        private readonly StatAdaptManager _statAdaptManager;
        private readonly MonoBehaviour _coroutineRunner;

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

            spawner.OnAllSpawnsComplete += HandleAllSpawnsComplete;
        }

        private void HandleAllSpawnsComplete()
        {
            _coroutineRunner.StartCoroutine(RunAfterStatsReady());
        }

        private System.Collections.IEnumerator RunAfterStatsReady()
        {
            yield return _calculateManager.RefreshFromCollectorAndWait();
            _statAdaptManager.ApplyToAllUnitsAndInitialize();
            _hpManager.setUnits();
            _statWindowManager.SetStrategyList();
            _hpManager.ApplyHpToHPBar();
        }

        public void Dispose()
        {
            _spawner.OnAllSpawnsComplete -= HandleAllSpawnsComplete;
        }
    }
}