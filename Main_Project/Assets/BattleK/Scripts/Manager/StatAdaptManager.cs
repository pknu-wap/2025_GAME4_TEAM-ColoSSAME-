using System.Collections;
using System.Collections.Generic;
using BattleK.Scripts.AI;
using BattleK.Scripts.Data;
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

        private Dictionary<string, CharacterStatsRow> _byUnitId;
        private Dictionary<string, CharacterStatsRow> _byUnitName;

        public void ApplyToAllUnitsAndInitialize()
        {
            foreach (var row in _calculateManager.AllStats)
            {
                if (row?.SourceUnit == null) continue;
                ApplyRow(row.SourceUnit, row);
                MarkReady(row.SourceUnit);
                row.SourceUnit.Initialize();
            }
        }

        private static string NormalizeName(string s) =>
            string.IsNullOrWhiteSpace(s) ? string.Empty : s.Trim().ToLowerInvariant();

        private void MarkReady(StaticAICore ai)
        {
            var ready = ai.GetComponent<StatsReady>();
            if (!ready) ready = ai.gameObject.AddComponent<StatsReady>();
            ready.MarkReady();
        }

        private static void ApplyRow(StaticAICore ai, CharacterStatsRow row)
        {
            var newAtk = Mathf.RoundToInt(row.ATK);
            var newDef = Mathf.RoundToInt(row.DEF);
            var newHp  = Mathf.RoundToInt(row.HP);
            var newAgi = Mathf.RoundToInt(row.AGI);

            ai.Stat.Name = row.Unit_Name;
            ai.Stat.AttackDamage = newAtk;
            ai.Stat.Defense = newDef;
            ai.Stat.MaxHP = newHp;
            ai.Stat.CurrentHP = newHp;
            ai.Stat.EvasionRate = Mathf.Min(newAgi * 0.03f, 0.35f);
            ai.Stat.AttackSpeed = newAgi * 1.05f;
            ai.SetInitialStats();
        }
    }
}
