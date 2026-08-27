using System.Collections;
using System.Collections.Generic;
using BattleK.Scripts.AI;
using BattleK.Scripts.Data;
using BattleK.Scripts.Data.Stat;
using BattleK.Scripts.Data.Type;
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
            foreach (var row in _calculateManager.AllStats)
            {
                var core = _calculateManager.GetCoreFor(row);
                if (core == null) continue;

                ApplyRow(core, row, _correctionTable);
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

        private static void ApplyRow(StaticAICore ai, CharacterStatsRow row, StatCorrectionTable table)
        {
            ai.Stat.Name = row.Unit_Name;

            var baseStat = new UnitBaseStat
            {
                UnitId = row.Unit_ID,
                UnitName = row.Unit_Name,
                Level = row.Level,
                Rarity = row.Rarity,
                BaseAtk = row.ATK,
                BaseDef = row.DEF,
                BaseHp = row.HP,
                BaseAgi = row.AGI,
                BaseAttackSpeed = ai.Stat.AttackSpeed,
                BaseSkillPoint = ai.Stat.SkillPoint,
                BaseMoveSpeed = ai.Stat.MoveSpeed,
                BaseAttackDelay = ai.Stat.AttackDelay,
                CurrentInjury = row.CurrentInjury
            };

            var finalStat = StatCalculator.Calculate(baseStat, table);
            finalStat.ApplyTo(ai.Stat);

            ai.SetInitialStats();
        }
    }
}