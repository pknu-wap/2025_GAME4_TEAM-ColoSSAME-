using System;
using System.Collections;
using System.Collections.Generic;
using BattleK.Scripts.AI;
using BattleK.Scripts.Data;
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

                if (!string.IsNullOrEmpty(r.Unit_ID))   //Id와 스탯 매핑
                    _byUnitId[r.Unit_ID] = r;

                if (string.IsNullOrEmpty(r.Unit_Name)) continue;    //한국어 이름 설정
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

            var units = FindObjectsOfType<AICore>(false);

            foreach (var ai in units)
            {
                if (ai == null) continue;

                var tried = new List<string>(6);
                var row = FindRowFor(ai, tried);

                if (row == null) continue;

                ApplyRow(ai, row);
                _battleStart.CheckSpawnComplete();
                var ready = ai.GetComponent<StatsReady>();
                if (ready == null) ready = ai.gameObject.AddComponent<StatsReady>();
                ready.MarkReady();
                
                var ras = ai.GetComponentsInChildren<RangedAttack>(true);
                foreach (var ra in ras) ra.ownerAI = ai;
            }
            ComputeStampSafe(_calculateManager);
        }

        private CharacterStatsRow FindRowFor(AICore ai, List<string> triedKeysCollector = null)
        {
            var cid = ai.GetComponent<CharacterID>();
            if (!cid || string.IsNullOrWhiteSpace(cid.characterKey)) return null;
            triedKeysCollector?.Add($"cid.characterKey='{cid.characterKey}'");
            return _byUnitId.GetValueOrDefault(cid.characterKey);
        }

        private static void ApplyRow(AICore ai, CharacterStatsRow row)
        {
            var newAtk = Mathf.RoundToInt(row.ATK);
            var newDef = Mathf.RoundToInt(row.DEF);
            var newHp  = Mathf.RoundToInt(row.HP);
            var newAgi = Mathf.RoundToInt(row.AGI);

            ai.Ko_Name = row.Unit_Name;
            ai.En_Name = row.Unit_ID;
            ai.attackDamage = newAtk;
            ai.def = newDef;
            ai.maxHp = newHp;
            ai.hp = newHp;
            ai.evasionRate = newAgi * 3;
            ai.attackSpeed = 100 + newAgi * 5;
        }
    }
}
