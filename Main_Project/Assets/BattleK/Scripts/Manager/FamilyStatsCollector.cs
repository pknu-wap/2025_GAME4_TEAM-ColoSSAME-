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
        [Header("플레이어/적 유닛이 배치된 월드 루트(= BattleStartUsingSlots 의 playerUnitsRoot/enemyUnitsRoot)")]
        [SerializeField] private Transform _playerUnitsRoot;
        [SerializeField] private Transform _enemyUnitsRoot;

        [Header("가문 JSON 루트 템플릿 ( {FAMILY} 토큰 치환 )")]
        [Tooltip("예: Assets/BattleK/Family/{FAMILY}/{FAMILY}.json")]
        [SerializeField] private string _familyJsonTemplate = "Assets/BattleK/Family/{FAMILY}/{FAMILY}.json";

        [Header("레벨 소스 (UnitLoadManager)")]
        [Tooltip("씬에 있는 UnitLoadManager를 Drag&Drop. 비워두면 런타임에 자동 탐색합니다.")]
        [SerializeField] private UnitLoadManager _unitLoadManager;

        [Header("키 매칭 옵션")]
        [Tooltip("true면 unitId/characterKey 비교 시 대소문자 무시")]
        [SerializeField] private bool _caseInsensitiveMatch = true;

        [Header("수집 결과 (읽기 전용)")]
        [SerializeField] private List<CharacterStatsRow> _playerStats = new();
        [SerializeField] private List<CharacterStatsRow> _enemyStats  = new();

        private Dictionary<string, FamilyJson> _familyCache = new();

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
            if (unitsRoot == null) return result;

            var characters = unitsRoot.GetComponentsInChildren<Transform>(includeInactive: false)
                .Select(trans => trans.gameObject)
                .Where(characterID => characterID.GetComponent<CharacterID>())
                .ToList();

            result.AddRange(from character in characters
            let family = character.GetComponent<FamilyID>()
            let characterId = character.GetComponent<CharacterID>()
            
            where family && characterId
            let familyKey = (family.FamilyKey ?? string.Empty).Trim()
            let characterKey = (characterId.characterKey ?? string.Empty).Trim()
            
            where !string.IsNullOrEmpty(familyKey) && !string.IsNullOrEmpty(characterKey)
            let familyJson = LoadFamilyJson(familyKey)
            
            where familyJson?.Characters != null && familyJson.Characters.Count != 0
            let comparison = _caseInsensitiveMatch ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal
            let matchCharacter = familyJson.Characters.FirstOrDefault(c => string.Equals(c.Unit_ID?.Trim(), characterKey, comparison))
            
            where matchCharacter != null
            let atk = matchCharacter.Stat_Distribution?.ATK ?? 0
            let def = matchCharacter.Stat_Distribution?.DEF ?? 0
            let hp = matchCharacter.Stat_Distribution?.HP ?? 0
            let agi = matchCharacter.Stat_Distribution?.AGI ?? 0
            let rarity = matchCharacter.Rarity
            let level = ResolveLevelFromUserSave(characterKey, matchCharacter.Level)
            select new CharacterStatsRow
            {
                Unit_ID = matchCharacter.Unit_ID,
                Unit_Name = matchCharacter.Unit_Name,
                ATK = atk,
                DEF = def,
                HP = hp,
                AGI = agi,
                Rarity = rarity,
                Level = level
            });

            return result;
        }
        private int ResolveLevelFromUserSave(string characterKey, int familyJsonLevel)
        {
            var comparison = _caseInsensitiveMatch ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;

            if (_unitLoadManager == null || _unitLoadManager.LoadedUser?.myUnits == null)
                return familyJsonLevel > 0 ? familyJsonLevel : 1;
            
            var mu = _unitLoadManager.LoadedUser.myUnits.Find(u =>
                !string.IsNullOrEmpty(u.unitId) &&
                string.Equals(u.unitId.Trim(), characterKey, comparison));

            if (mu == null) return familyJsonLevel > 0 ? familyJsonLevel : 1;
            var lv = Mathf.Max(1, mu.level);
            return lv;

        }
        private FamilyJson LoadFamilyJson(string familyKey)
        {
            if (_familyCache.TryGetValue(familyKey, out var cached)) return cached;

            var path = BuildFamilyJsonAbsolutePath(familyKey);
            
            if (!JsonFileLoader.TryLoadJsonFile<FamilyJson>(path, out var familyChache, out var msg))
            {
                _familyCache[familyKey] = null;
                return null;
            }
            _familyCache[familyKey] = familyChache;
            return familyChache;
        }
        private string BuildFamilyJsonAbsolutePath(string familyKey)
        {
            var rel = (_familyJsonTemplate ?? string.Empty).Replace("{FAMILY}", familyKey);
            return BuildAbsolutePath(rel);
        }

        private static string BuildAbsolutePath(string relPath)
        {
            relPath = NormalizePath(relPath);
            if (Path.IsPathRooted(relPath)) return relPath;

            var assetsPath  = NormalizePath(Application.dataPath);
            var projectRoot = NormalizePath(Directory.GetParent(assetsPath)?.FullName);
            return NormalizePath(relPath.StartsWith("Assets/", StringComparison.OrdinalIgnoreCase) ? Path.Combine(projectRoot, relPath) : Path.Combine(assetsPath, relPath));
        }
        private static string NormalizePath(string p) => string.IsNullOrEmpty(p) ? p : p.Replace('\\', '/');
    }
}
