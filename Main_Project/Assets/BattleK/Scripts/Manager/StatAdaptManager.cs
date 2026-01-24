using System;
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
        [SerializeField] private BattleStartUsingSlots _battleStart;
    
        [Header("필수 참조")]
        [SerializeField] private CalculateManager _calculateManager;

        private Dictionary<string, CharacterStatsRow> _byUnitId;
        private Dictionary<string, CharacterStatsRow> _byUnitName;

        private void Start()
        {
            StartCoroutine(WatchAndReapplyLoop());
        }

        private IEnumerator WatchAndReapplyLoop()
        {
            while (_calculateManager.AllStats == null || _calculateManager.AllStats.Count == 0)
            {
                yield return null;
            }
            
            RebuildIndexIfNeeded(true);
            ApplyToAllUnits();
        }
        
        private void RebuildIndexIfNeeded(bool force = false)
        {
            if (!force && _byUnitId != null && _byUnitName != null) return;

            _byUnitId   = new Dictionary<string, CharacterStatsRow>(StringComparer.Ordinal);
            _byUnitName = new Dictionary<string, CharacterStatsRow>(StringComparer.OrdinalIgnoreCase);

            var rows = _calculateManager?.AllStats;
            if (rows == null) return;

            foreach (var r in rows)
            {
                if (r == null) continue;

                if (!string.IsNullOrEmpty(r.Unit_ID))
                    _byUnitId[r.Unit_ID] = r;

                if (string.IsNullOrEmpty(r.Unit_Name)) continue;
                var k = NormalizeName(r.Unit_Name);
                _byUnitName.TryAdd(k, r);
            }
        }

        private static string NormalizeName(string s)
        {
            return string.IsNullOrWhiteSpace(s) ? string.Empty : s.Trim().ToLowerInvariant();
        }

        private static int ComputeStampSafe(CalculateManager cm)
        {
            return cm.AllStats == null ? 0 : ComputeStamp(cm.AllStats);
        }

        private static int ComputeStamp(IReadOnlyList<CharacterStatsRow> list)
        {
            if (list == null) return 0;
            unchecked
            {
                var acc = 17 ^ list.Count;
                foreach (var r in list)
                {
                    if (r == null) continue;
                    acc ^= r.Unit_ID?.GetHashCode() ?? 0;
                    acc ^= r.Level * 397;
                    acc ^= r.ATK   * 131;
                    acc ^= r.DEF   *  67;
                    acc ^= r.HP    *  31;
                    acc ^= r.AGI   *   7;
                }
                return acc;
            }
        }
        
        private void ApplyToAllUnits()
        {
            if (_calculateManager.AllStats.Count == 0)
            {
                Debug.LogWarning("[StatAdaptManager] CalculateManager 데이터가 비었습니다.");
                return;
            }

            if (_byUnitId == null) RebuildIndexIfNeeded(force: true);

            var units = FindObjectsOfType<StaticAICore>(false);

            foreach (var ai in units)
            {
                if (!ai) continue;

                var tried = new List<string>(6);
                var row = FindRowFor(ai, tried);

                if (row == null) continue;

                ApplyRow(ai, row);
                _battleStart.CheckSpawnComplete();
                var ready = ai.GetComponent<StatsReady>();
                if (!ready) ready = ai.gameObject.AddComponent<StatsReady>();
                ready.MarkReady();
            }
            ComputeStampSafe(_calculateManager);
        }

        private CharacterStatsRow FindRowFor(StaticAICore ai, List<string> triedKeysCollector = null)
        {
            var cid = ai.GetComponent<CharacterID>();
            if (!cid || string.IsNullOrWhiteSpace(cid.characterKey)) return null;
            triedKeysCollector?.Add($"cid.characterKey='{cid.characterKey}'");
            return _byUnitId.GetValueOrDefault(cid.characterKey);
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
            ai.Stat.AttackSpeed =  newAgi * 1.05f;
        }
    }
}
