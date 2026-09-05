using BattleK.Scripts.HP;
using BattleK.Scripts.UI;

namespace BattleK.Scripts.Manager
{
    public class SpawnCompletionCoordinator
    {
        private readonly UnitSpawner _spawner;
        private readonly HPManager _hpManager;
        private readonly StatWindowManager _statWindowManager;

        public SpawnCompletionCoordinator(
            UnitSpawner spawner,
            HPManager hpManager,
            StatWindowManager statWindowManager)
        {
            _spawner = spawner;
            _hpManager = hpManager;
            _statWindowManager = statWindowManager;

            spawner.OnAllSpawnsComplete += HandleAllSpawnsComplete;
        }

        private void HandleAllSpawnsComplete()
        {
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