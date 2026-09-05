using System.Collections.Generic;
using BattleK.Scripts.Data.ClassInfo;
using UnityEngine;

namespace BattleK.Scripts.Data.Stat
{
    [CreateAssetMenu(fileName = "ClassBaseStatTable", menuName = "BattleK/Stat/Class Base Stat Table")]
    public class ClassBaseStatTable : ScriptableObject
    {
        [System.Serializable]
        public struct Entry
        {
            public UnitClass UnitClass;

            [Header("직업 고정값 (레벨/희귀도 영향 없음, 배틀 전용)")]
            public float MoveSpeed;
            public float AttackSpeed;
            public float AttackDelay;
        }

        [SerializeField] private List<Entry> entries = new();

        private Dictionary<UnitClass, Entry> _lookup;

        private void OnEnable() => RebuildLookup();
        private void OnValidate() => RebuildLookup();

        private void RebuildLookup()
        {
            _lookup = new Dictionary<UnitClass, Entry>(entries.Count);
            foreach (var entry in entries)
            {
                _lookup[entry.UnitClass] = entry;
            }
        }

        public bool TryGetEntry(UnitClass unitClass, out Entry entry)
        {
            if (_lookup == null) RebuildLookup();
            return _lookup.TryGetValue(unitClass, out entry);
        }

        public Entry GetEntryOrDefault(UnitClass unitClass)
        {
            if (TryGetEntry(unitClass, out var entry)) return entry;

            Debug.LogWarning($"[ClassBaseStatTable] {unitClass}에 대한 항목이 없습니다. 기본값(0)을 반환합니다.");
            return new Entry { UnitClass = unitClass };
        }
        
        public void ApplyTo(UnitRuntimeStat runtimeStat, UnitClass unitClass)
        {
            var entry = GetEntryOrDefault(unitClass);
            runtimeStat.MoveSpeed = entry.MoveSpeed;
            runtimeStat.AttackSpeed = entry.AttackSpeed;
            runtimeStat.AttackDelay = entry.AttackDelay;
        }
    }
}