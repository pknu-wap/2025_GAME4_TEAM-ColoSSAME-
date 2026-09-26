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

        [Tooltip("직업(UnitClass)별 MoveSpeed/AttackSpeed/AttackDelay 고정값 테이블")]
        [SerializeField] private ClassBaseStatTable _classBaseStatTable;

        public void ApplyToAllUnitsAndInitialize()
        {
            foreach (var stat in _calculateManager.AllStats)
            {
                var core = _calculateManager.GetCoreFor(stat);
                if (core == null) continue;
                if (!UnitStatRepository.TryGet(stat.UnitId, out var info)) continue;
                ApplyStat(core, info);
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

        private static void ApplyStat(StaticAICore ai, UnitDisplayInfo info)
        {
            ai.runtimeStat.Name = info.UnitName;
            ai.runtimeStat.CharacterImage = info.CharacterImage;
            info.Stat.ApplyTo(ai.runtimeStat);

            ai.SetInitialStats();
        }
    }
}