using System;
using System.Collections.Generic;
using UnityEngine;

namespace BattleK.Scripts.Data.ClassInfo
{
    [CreateAssetMenu(fileName = "ClassIconTable", menuName = "BattleK/UI/ClassIconTable")]
    public class ClassIconTable : ScriptableObject
    {
        [Serializable]
        public class Entry
        {
            public UnitClass Class;
            public Sprite Icon;
        }

        [SerializeField] private List<Entry> entries;

        private Dictionary<UnitClass, Sprite> _cache;

        private void OnEnable()
        {
            _cache = new Dictionary<UnitClass, Sprite>();
            foreach (var e in entries)
            {
                if (!_cache.ContainsKey(e.Class))
                    _cache.Add(e.Class, e.Icon);
            }
        }

        public Sprite GetIcon(UnitClass unitClass)
        {
            if (_cache == null) OnEnable();

            if (_cache.TryGetValue(unitClass, out var sprite))
                return sprite;

            Debug.LogWarning($"[ClassIconTable] {unitClass}에 대한 아이콘이 등록되지 않았습니다.");
            return null;
        }
    }
}