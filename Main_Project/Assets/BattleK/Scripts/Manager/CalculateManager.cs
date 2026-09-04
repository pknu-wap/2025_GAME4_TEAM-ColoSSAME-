using System;
using System.Collections;
using System.Collections.Generic;
using BattleK.Scripts.AI;
using BattleK.Scripts.Data.Stat;
using BattleK.Scripts.Data.Type;
using UnityEngine;

namespace BattleK.Scripts.Manager
{
    public class CalculateManager : MonoBehaviour
    {
        [Header("스탯 소스 (FamilyStatsCollector)")]
        [Tooltip("씬에 존재하는 FamilyStatsCollector를 Drag&Drop. 비워두면 자동 탐색합니다.")]
        [SerializeField] private FamilyStatsCollector _statsCollector;
        [SerializeField] private PlayerStatsCollector _playerStatsCollector;

        [Header("자동 재시도(유닛이 늦게 스폰될 때 대비)")]
        [SerializeField] private bool _autoRetryIfEmpty = true;

        [Tooltip("재시도 간격(초)")]
        [SerializeField] private float _retryIntervalSeconds = 0.25f;

        [Tooltip("최대 재시도 횟수")]
        [SerializeField] private int _maxRetries = 40;

        [Header("로컬 복사본(읽기 전용 미리보기)")]
        [SerializeField] private List<CharacterStatsRow> _playerStats = new();
        [SerializeField] private List<CharacterStatsRow> _enemyStats  = new();
        [SerializeField] private List<CharacterStatsRow> _allStats    = new();

        [Tooltip("PlayerStatsCollector로 얻은, AICore 없는 미리보기 전용 결과 (로스터 UI 등에서 사용)")]
        [SerializeField] private List<CharacterStatsRow> _playerPreviewStats = new();

        [Header("보정 테이블")]
        [SerializeField] private StatCorrectionTable _correctionTable;
        private readonly Dictionary<CharacterStatsRow, StaticAICore> _rowToCore = new();

        public IReadOnlyList<CharacterStatsRow> AllStats => _allStats;
        public IReadOnlyList<CharacterStatsRow> PlayerPreviewStats => _playerPreviewStats;

        public StaticAICore GetCoreFor(CharacterStatsRow row) =>
            row != null && _rowToCore.TryGetValue(row, out var core) ? core : null;

        private void Start()
        {
            StartCoroutine(RefreshFlow());
        }

        [ContextMenu("Refresh Now")]
        public void RefreshNow()
        {
            StopAllCoroutines();
            StartCoroutine(RefreshFlow());
        }

        private IEnumerator RefreshFlow()
        {
            yield return StartCoroutine(RefreshFromCollectorCoroutine());

            if (!_autoRetryIfEmpty || !IsEmpty()) yield break;
            var tries = 0;
            while (tries < _maxRetries && IsEmpty())
            {
                tries++;
                yield return new WaitForSeconds(_retryIntervalSeconds);
                yield return StartCoroutine(RefreshFromCollectorCoroutine());
            }
        }
        
        public IEnumerator RefreshFromCollectorAndWait()
        {
            StopAllCoroutines();
            yield return StartCoroutine(RefreshFromCollectorCoroutine());
        }
        
        public void RefreshPlayerPreviewOnly()
        {
            if (_playerStatsCollector == null)
            {
                Debug.LogWarning("[CalculateManager] PlayerStatsCollector 없음");
                return;
            }

            _playerStatsCollector.CollectPlayerUnits();
            _playerPreviewStats = CalculatePreviewRows(_playerStatsCollector.PlayerStats);
        }

        private bool IsEmpty() =>
            (_playerStats == null || _playerStats.Count == 0) &&
            (_enemyStats  == null || _enemyStats.Count  == 0);

        private IEnumerator RefreshFromCollectorCoroutine()
        {
            if (_statsCollector == null)
            {
                Debug.LogWarning("[CalculateManager] statsCollector가 비어 있습니다. 갱신 불가.");
                ClearLocal();
                yield break;
            }

            _statsCollector.CollectFromBothTeams();
            yield return null;

            _rowToCore.Clear();

            _playerStats = CalculateRows(_statsCollector.PlayerStats);
            _enemyStats  = CalculateRows(_statsCollector.EnemyStats);

            _allStats = new List<CharacterStatsRow>(_playerStats.Count + _enemyStats.Count);
            _allStats.AddRange(_playerStats);
            _allStats.AddRange(_enemyStats);
        }

        public void RefreshFromCollectorOnce()
        {
            StopAllCoroutines();
            StartCoroutine(RefreshFromCollectorCoroutine());
        }

        public CharacterStatsRow FindUnit(string unitId, bool searchEnemyToo = true)
        {
            if (string.IsNullOrWhiteSpace(unitId)) return null;

            var found = _playerStats.Find(r => string.Equals(r.Unit_ID, unitId, StringComparison.Ordinal));
            if (found != null) return found;

            return searchEnemyToo
                ? _enemyStats.Find(r => string.Equals(r.Unit_ID, unitId, StringComparison.Ordinal))
                : null;
        }

        private void ClearLocal()
        {
            _playerStats = new List<CharacterStatsRow>();
            _enemyStats  = new List<CharacterStatsRow>();
            _allStats    = new List<CharacterStatsRow>();
            _rowToCore.Clear();
        }
        
        private List<CharacterStatsRow> CalculateRows(IReadOnlyList<(CharacterStatsRow Row, StaticAICore Core)> src)
        {
            var list = new List<CharacterStatsRow>(src?.Count ?? 0);
            if (src == null) return list;

            foreach (var (row, core) in src)
            {
                var baseStat = BuildBaseStat(row, core.Stat.AttackSpeed, core.Stat.SkillPoint, core.Stat.MoveSpeed, core.Stat.AttackDelay);
                var finalStat = StatCalculator.Calculate(baseStat, _correctionTable);

                var calculatedRow = new CharacterStatsRow
                {
                    Unit_ID = row.Unit_ID,
                    Unit_Name = row.Unit_Name,
                    UnitClass = row.UnitClass,
                    Level = row.Level,
                    ATK = finalStat.AttackDamage,
                    DEF = finalStat.Defense,
                    HP = finalStat.MaxHp,
                    AGI = row.AGI,
                    Tier = row.Tier,
                    CurrentInjury = row.CurrentInjury
                };

                list.Add(calculatedRow);
                _rowToCore[calculatedRow] = core;

                finalStat.ApplyTo(core.Stat);
                UnitStatRepository.Set(row.Unit_ID, row.Unit_Name, row.Tier, row.Level, row.UnitClass, core.Stat.CharacterImage, finalStat);
            }
            return list;
        }
        
        private List<CharacterStatsRow> CalculatePreviewRows(IReadOnlyList<CharacterStatsRow> src)
        {
            var list = new List<CharacterStatsRow>(src?.Count ?? 0);
            if (src == null) return list;

            foreach (var row in src)
            {
                var baseStat = BuildBaseStat(row, 0f, 0, 0f, 0f);
                var finalStat = StatCalculator.Calculate(baseStat, _correctionTable);

                UnitStatRepository.Set(row.Unit_ID, row.Unit_Name, row.Tier, row.Level, row.UnitClass, null, finalStat);

                list.Add(new CharacterStatsRow
                {
                    Unit_ID = row.Unit_ID,
                    Unit_Name = row.Unit_Name,
                    UnitClass = row.UnitClass,
                    Level = row.Level,
                    ATK = finalStat.AttackDamage,
                    DEF = finalStat.Defense,
                    HP = finalStat.MaxHp,
                    AGI = row.AGI,
                    Tier = row.Tier,
                    CurrentInjury = row.CurrentInjury
                });
            }
            return list;
        }

        private static UnitBaseStat BuildBaseStat(
            CharacterStatsRow row, float baseAttackSpeed, int baseSkillPoint, float baseMoveSpeed, float baseAttackDelay)
        {
            return new UnitBaseStat
            {
                UnitId = row.Unit_ID,
                UnitName = row.Unit_Name,
                UnitClass = row.UnitClass,
                Level = row.Level,
                Rarity = row.Tier,
                BaseAtk = row.ATK,
                BaseDef = row.DEF,
                BaseHp = row.HP,
                BaseAgi = row.AGI,
                BaseAttackSpeed = baseAttackSpeed,
                BaseSkillPoint = baseSkillPoint,
                BaseMoveSpeed = baseMoveSpeed,
                BaseAttackDelay = baseAttackDelay,
                CurrentInjury = row.CurrentInjury
            };
        }
    }
}
