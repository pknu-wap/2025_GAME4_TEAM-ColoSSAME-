using System.Collections.Generic;
using System.Linq;
using BattleK.Scripts.Data.ClassInfo;
using UnityEngine;

namespace BattleK.Scripts.CharacterCreator
{
    [CreateAssetMenu(fileName = "ClassDefinitionDatabase", menuName = "Game/Class Definition Database")]
    public class ClassDefinitionDatabase : ScriptableObject
    {
        [System.Serializable]
        public class Entry
        {
            public UnitClass UnitClass;
            public ClassDefinitionSO Definition;
        }

        public List<Entry> definitions = new();

        private Dictionary<UnitClass, ClassDefinitionSO> _cache;

        public ClassDefinitionSO GetDefinition(UnitClass unitClass)
        {
            BuildCacheIfNeeded();
            return _cache.GetValueOrDefault(unitClass);
        }

        private void BuildCacheIfNeeded()
        {
            if (_cache != null) return;
            _cache = new Dictionary<UnitClass, ClassDefinitionSO>();
            foreach (var entry in definitions)
            {
                if (entry?.Definition == null) continue;
                _cache.TryAdd(entry.UnitClass, entry.Definition);
            }
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            _cache = null;

            var duplicates = definitions
                .GroupBy(e => e.UnitClass)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key)
                .ToList();

            foreach (var dup in duplicates)
            {
                Debug.LogWarning($"[ClassDefinitionDatabase:{name}] UnitClass '{dup}'가 중복 등록되어 있습니다.");
            }

            foreach (var entry in definitions)
            {
                if (entry?.Definition != null && entry.Definition.UnitClass != entry.UnitClass)
                {
                    Debug.LogWarning($"[ClassDefinitionDatabase:{name}] 슬롯의 UnitClass({entry.UnitClass})와 " +
                                      $"연결된 SO의 UnitClass({entry.Definition.UnitClass})가 일치하지 않습니다.");
                }
            }
        }
#endif
    }
}