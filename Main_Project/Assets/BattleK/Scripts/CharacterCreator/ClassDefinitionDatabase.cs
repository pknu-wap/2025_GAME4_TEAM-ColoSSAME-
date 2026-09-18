using System.Collections.Generic;
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
    }
}