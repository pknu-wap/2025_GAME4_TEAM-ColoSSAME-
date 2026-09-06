using System;
using System.Collections.Generic;
using System.Linq;
using BattleK.Scripts.AI;
using BattleK.Scripts.Data;
using BattleK.Scripts.Data.Stat;
using UnityEngine;

namespace BattleK.Scripts.Manager
{
    public class FamilyStatsCollector : MonoBehaviour
    {
        [Header("UnitRoots)")]
        [SerializeField] private Transform _playerUnitsRoot;
        [SerializeField] private Transform _enemyUnitsRoot;

        [Header("레벨 소스 (UnitLoadManager)")]
        [SerializeField] private UnitLoadManager _unitLoadManager;
        private EnemySaveManager _enemySaveManager;
        private League _league;
        [SerializeField] private ItemDatabase _itemDatabase;

        [Header("key setting")]
        [Tooltip("true면 unitId/characterKey 비교 시 대소문자 무시")]
        [SerializeField] private bool _caseInsensitiveMatch = true;

        [Header("수집 결과 (읽기 전용)")]
        [SerializeField] private List<(UnitBaseStat Stat, StaticAICore Core)> _playerStats = new();
        [SerializeField] private List<(UnitBaseStat Stat, StaticAICore Core)> _enemyStats  = new();

        public IReadOnlyList<(UnitBaseStat Stat, StaticAICore Core)> PlayerStats => _playerStats;
        public IReadOnlyList<(UnitBaseStat Stat, StaticAICore Core)> EnemyStats  => _enemyStats;

        private void Awake()
        {
            _league = LeagueManager.Instance.league;
            _enemySaveManager ??= EnemySaveManager.Instance;
        }

        public void CollectFromBothTeams()
        {
            _playerStats = CollectFromRoot(_playerUnitsRoot, true);
            _enemyStats  = CollectFromRoot(_enemyUnitsRoot, false);
        }

        private List<(UnitBaseStat Stat, StaticAICore Core)> CollectFromRoot(Transform unitsRoot, bool isPlayer)
        {
            var result = new List<(UnitBaseStat Stat, StaticAICore Core)>();
            if (!unitsRoot) return result;

            var unitTransforms = unitsRoot.GetComponentsInChildren<Transform>(includeInactive: false);
            var comparison = _caseInsensitiveMatch ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;

            foreach (var unit in unitTransforms)
            {
                if (!unit.TryGetComponent(out CharacterID characterIdComp)) continue;
                if (!unit.TryGetComponent(out FamilyID familyIdComp)) continue;
                if (!unit.TryGetComponent(out StaticAICore aiCore)) continue;

                var charKey = characterIdComp.characterKey?.Trim();
                var familyKey = familyIdComp.FamilyKey?.Trim();

                if (string.IsNullOrEmpty(charKey) || string.IsNullOrEmpty(familyKey)) continue;

                var familyUnits = UnitDataManager.Instance.GetFamilyUnits(familyKey);
                if (familyUnits == null) continue;

                var matchData = familyUnits.FirstOrDefault(c => string.Equals(c.Unit_ID?.Trim(), charKey, comparison));
                if (matchData == null) continue;

                var savedUnit = isPlayer ? FindUserUnit(charKey, comparison) : FindEnemyUnit(charKey, comparison);

                aiCore.runtimeStat.LoadFrom(savedUnit, _itemDatabase);

                var baseStat = UnitBaseStat.FromFamilyAndSave(matchData, savedUnit);

                result.Add((baseStat, aiCore));
            }

            return result;
        }

        private Unit FindUserUnit(string characterKey, StringComparison comparison)
        {
            var myUnits = _unitLoadManager?.LoadedUser?.myUnits;
            return myUnits?.Find(u => string.Equals(u.Id?.Trim(), characterKey, comparison));
        }

        private Unit FindEnemyUnit(string characterKey, StringComparison comparison)
        {
            var team = _enemySaveManager.GetTeam(_league.currentEnemyTeamId);
            var units = team?.units;

            return units?.Find(u => string.Equals(u.Id?.Trim(), characterKey, comparison));
        }
    }
}