using System.Collections;
using System.Collections.Generic;
using BattleK.Scripts.AI;
using BattleK.Scripts.HP;
using BattleK.Scripts.Manager.Battle;
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

            yield return WaitForStatsReadyOrTimeout(3f);
            BattleItemEffectRunner.EnsureInstance().StartBattle(_aiManager);
        }

        private IEnumerator WaitForStatsReadyOrTimeout(float timeoutSeconds)
        {
            float endTime = Time.time + timeoutSeconds;
            yield return new WaitUntil(() => AreAllBattleUnitsStatsReady() || Time.time >= endTime);
        }

        private bool AreAllBattleUnitsStatsReady()
        {
            return AreUnitsStatsReady(_aiManager.playerUnits) &&
                   AreUnitsStatsReady(_aiManager.enemyUnits);
        }

        private static bool AreUnitsStatsReady(IReadOnlyList<StaticAICore> units)
        {
            if (units == null || units.Count == 0) return false;

            for (int i = 0; i < units.Count; i++)
            {
                StaticAICore unit = units[i];
                if (!unit || !unit.GetComponent<StatsReady>())
                    return false;
            }

            return true;
        }
        
        public void Dispose()
        {
            _spawner.OnAllSpawnsComplete -= HandleAllSpawnsComplete;
        }
    }
}
