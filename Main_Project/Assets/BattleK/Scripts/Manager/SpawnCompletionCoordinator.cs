using System.Collections;
using BattleK.Scripts.HP;
using BattleK.Scripts.UI;
using UnityEngine;

namespace BattleK.Scripts.Manager
{
    public class SpawnCompletionCoordinator
    {
        private readonly AI_Manager _aiManager;
        private readonly FamilyStatsCollector _statsCollector;
        private readonly CalculateManager _calculateManager;
        private readonly HPManager _hpManager;
        private readonly StatWindowManager _statWindowManager;
        private readonly UnitSpawner _spawner;

        public SpawnCompletionCoordinator(
            UnitSpawner spawner,
            AI_Manager aiManager,
            FamilyStatsCollector statsCollector,
            CalculateManager calculateManager,
            HPManager hpManager,
            StatWindowManager statWindowManager)
        {
            _spawner = spawner;
            _aiManager = aiManager;
            _statsCollector = statsCollector;
            _calculateManager = calculateManager;
            _hpManager = hpManager;
            _statWindowManager = statWindowManager;

            spawner.OnAllSpawnsComplete += HandleAllSpawnsComplete;
        }

        private void HandleAllSpawnsComplete()
        {
            CoroutineRunner.Run(NotifyManagersRoutine());
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
        
        public void Dispose()
        {
            _spawner.OnAllSpawnsComplete -= HandleAllSpawnsComplete;
        }
    }
}