using BattleK.Scripts.AI;
using BattleK.Scripts.Data.Stat;
using BattleK.Scripts.HP;
using BattleK.Scripts.UI;
using UnityEngine;

namespace BattleK.Scripts.Manager
{
    [DisallowMultipleComponent]
    public class StatAdaptManager : MonoBehaviour
    {
        [Header("Manager")]
        [SerializeField] private StatWindowManager _statWindowManager;
        [SerializeField] private HPManager _hpManager;

        [Header("필수 참조")]
        [SerializeField] private CalculateManager _calculateManager;
        [SerializeField] private StatCorrectionTable _correctionTable;

        public void ApplyToAllUnitsAndInitialize()
        {
            foreach (var stat in _calculateManager.AllStats)
            {
                var core = _calculateManager.GetCoreFor(stat);
                if (core == null) continue;

                ApplyStat(core, stat, _correctionTable);
                MarkReady(core);
                core.Initialize();
            }
        }

        private void MarkReady(StaticAICore ai)
        {
            var ready = ai.GetComponent<StatsReady>();
            if (!ready) ready = ai.gameObject.AddComponent<StatsReady>();
            ready.MarkReady();
        }

        private static void ApplyStat(StaticAICore ai, UnitBaseStat stat, StatCorrectionTable table)
        {
            ai.runtimeStat.Name = stat.UnitName;
            var finalStat = StatCalculator.Calculate(stat, table);
            finalStat.ApplyTo(ai.runtimeStat);

            ai.SetInitialStats();
        }
    }
}