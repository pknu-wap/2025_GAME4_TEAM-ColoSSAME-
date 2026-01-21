using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BattleK.Scripts.Data;
using BattleK.Scripts.Data.Type;
using BattleK.Scripts.JSON;
using UnityEngine;

namespace BattleK.Scripts.Manager
{
    public class FamilyStatsCollector : MonoBehaviour
    {
        [Header("UnitRoots)")]
        [SerializeField] private Transform _playerUnitsRoot;
        [SerializeField] private Transform _enemyUnitsRoot;

        [Header("Json 설정")]
        [SerializeField] private string _familyJsonTemplate = "Assets/BattleK/Family/{FAMILY}/{FAMILY}.json";

        [Header("레벨 소스 (UnitLoadManager)")]
        [SerializeField] private UnitLoadManager _unitLoadManager;

        [Header("key setting")]
        [Tooltip("true면 unitId/characterKey 비교 시 대소문자 무시")]
        [SerializeField] private bool _caseInsensitiveMatch = true;

        [Header("수집 결과 (읽기 전용)")]
        [SerializeField] private List<CharacterStatsRow> _playerStats = new();
        [SerializeField] private List<CharacterStatsRow> _enemyStats  = new();

        private readonly Dictionary<string, FamilyJson> _familyCache = new();

        public IReadOnlyList<CharacterStatsRow> PlayerStats => _playerStats;
        public IReadOnlyList<CharacterStatsRow> EnemyStats  => _enemyStats;
        
        public void CollectFromBothTeams()
        {
            _playerStats = CollectFromRoot(_playerUnitsRoot);
            _enemyStats  = CollectFromRoot(_enemyUnitsRoot);
        }

        private List<CharacterStatsRow> CollectFromRoot(Transform unitsRoot)
        {
            var result = new List<CharacterStatsRow>();
            if (!unitsRoot) return result;

            var unitTransforms = unitsRoot.GetComponentsInChildren<Transform>(includeInactive: false);

            foreach (var unit in unitTransforms)
            {
                if(!unit.TryGetComponent(out CharacterID characterIdComp)) continue;
                if(!unit.TryGetComponent(out FamilyID familyIdComp)) continue;

                var charKey = characterIdComp.characterKey?.Trim();
                var familyKey = familyIdComp.FamilyKey?.Trim();
                
                if(string.IsNullOrEmpty(charKey) || string.IsNullOrEmpty(familyKey)) continue;
                
                var familyJson = LoadFamilyJson(familyKey);
                if(familyJson?.Characters == null) continue;
                
                var comparison = _caseInsensitiveMatch ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
                var matchData = familyJson.Characters.FirstOrDefault(c => string.Equals(c.Unit_ID?.Trim(), charKey, comparison));
                
                if(matchData == null) continue;
                
                var level = ResolveLevelFromUserSave(charKey, matchData.Level);
                
                result.Add(new CharacterStatsRow
                {
                    Unit_ID = matchData.Unit_ID,
                    Unit_Name = matchData.Unit_Name,
                    ATK = matchData.Stat_Distribution?.ATK ?? 0,
                    DEF = matchData.Stat_Distribution?.DEF ?? 0,
                    HP = matchData.Stat_Distribution?.HP ?? 0,
                    AGI = matchData.Stat_Distribution?.AGI ?? 0,
                    Rarity = matchData.Rarity,
                    Level = level
                });
            }

            return result;
        }
        private int ResolveLevelFromUserSave(string characterKey, int defaultLevel)
        {
            var baseLevel = defaultLevel > 0 ? defaultLevel : 1;

            if (!_unitLoadManager || _unitLoadManager.LoadedUser?.myUnits == null)
                return baseLevel;
            
            var comparison = _caseInsensitiveMatch ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
            var myUnit = _unitLoadManager.LoadedUser.myUnits.Find(u => string.Equals(u.unitId?.Trim(), characterKey, comparison));

            return myUnit == null ? baseLevel : Mathf.Max(1, myUnit.level);
        }
        private FamilyJson LoadFamilyJson(string familyKey)
        {
            if (_familyCache.TryGetValue(familyKey, out var cached)) return cached;

            var path = BuildFamilyJsonAbsolutePath(familyKey);
            
            if (JsonFileHandler.TryLoadJsonFile<FamilyJson>(path, out var loadedData, out var msg))
            {
                _familyCache[familyKey] = loadedData;
                return loadedData;
            }
            _familyCache[familyKey] = null;
            return null;
        }
        private string BuildFamilyJsonAbsolutePath(string familyKey)
        {
            var relativePath = _familyJsonTemplate.Replace("{FAMILY}", familyKey);
            if(Path.IsPathRooted(relativePath)) return relativePath;
            
            var basePath = Application.dataPath;
            var projectRoot = Directory.GetParent(basePath)?.FullName;
            
            return relativePath.StartsWith("Assets/", StringComparison.OrdinalIgnoreCase) ? Path.Combine(projectRoot ?? basePath, relativePath) : Path.Combine(basePath, relativePath);
        }
    }
}
