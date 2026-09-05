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
        [SerializeField] private List<UnitBaseStat> _playerStats = new();
        [SerializeField] private List<UnitBaseStat> _enemyStats  = new();
        [SerializeField] private List<UnitBaseStat> _allStats    = new();

        [Tooltip("PlayerStatsCollector로 얻은, AICore 없는 미리보기 전용 결과 (로스터 UI 등에서 사용)")]
        [SerializeField] private List<UnitBaseStat> _playerPreviewStats = new();

        [Header("보정 테이블")]
        [SerializeField] private StatCorrectionTable _correctionTable;

        [Tooltip("직업(UnitClass)별 MoveSpeed/AttackSpeed/AttackDelay 고정값 테이블")]
        [SerializeField] private ClassBaseStatTable _classBaseStatTable;

        private readonly Dictionary<UnitBaseStat, StaticAICore> _statToCore = new();

        public IReadOnlyList<UnitBaseStat> AllStats => _allStats;
        public IReadOnlyList<UnitBaseStat> PlayerPreviewStats => _playerPreviewStats;

        public StaticAICore GetCoreFor(UnitBaseStat stat) =>
            stat != null && _statToCore.TryGetValue(stat, out var core) ? core : null;

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
            _playerPreviewStats = CalculatePreviewStats(_playerStatsCollector.PlayerStats);
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

            _statToCore.Clear();

            _playerStats = CalculateStats(_statsCollector.PlayerStats);
            _enemyStats  = CalculateStats(_statsCollector.EnemyStats);

            _allStats = new List<UnitBaseStat>(_playerStats.Count + _enemyStats.Count);
            _allStats.AddRange(_playerStats);
            _allStats.AddRange(_enemyStats);
        }

        public void RefreshFromCollectorOnce()
        {
            StopAllCoroutines();
            StartCoroutine(RefreshFromCollectorCoroutine());
        }

        public UnitBaseStat FindUnit(string unitId, bool searchEnemyToo = true)
        {
            if (string.IsNullOrWhiteSpace(unitId)) return null;

            var found = _playerStats.Find(s => string.Equals(s.UnitId, unitId, StringComparison.Ordinal));
            if (found != null) return found;

            return searchEnemyToo
                ? _enemyStats.Find(s => string.Equals(s.UnitId, unitId, StringComparison.Ordinal))
                : null;
        }

        private void ClearLocal()
        {
            _playerStats = new List<UnitBaseStat>();
            _enemyStats  = new List<UnitBaseStat>();
            _allStats    = new List<UnitBaseStat>();
            _statToCore.Clear();
        }

        // src: FamilyStatsCollector가 (FamilyCharacter + 세이브를 이미 합성한) UnitBaseStat과
        // StaticAICore를 묶어 전달한다고 가정. MoveSpeed 등 직업 고정값은 ClassBaseStatTable에서 채운다.
        private List<UnitBaseStat> CalculateStats(IReadOnlyList<(UnitBaseStat Stat, StaticAICore Core)> src)
        {
            var list = new List<UnitBaseStat>(src?.Count ?? 0);
            if (src == null) return list;

            foreach (var (stat, core) in src)
            {
                var finalStat = StatCalculator.Calculate(stat, _correctionTable, _classBaseStatTable);

                var calculatedStat = new UnitBaseStat
                {
                    UnitId = stat.UnitId,
                    UnitName = stat.UnitName,
                    UnitClass = stat.UnitClass,
                    Level = stat.Level,
                    Rarity = stat.Rarity,
                    BaseAtk = finalStat.AttackDamage,
                    BaseDef = finalStat.Defense,
                    BaseHp = finalStat.MaxHp,
                    BaseAgi = stat.BaseAgi,
                    BaseEvasionRate = finalStat.EvasionRate,
                    CurrentInjury = stat.CurrentInjury
                };

                list.Add(calculatedStat);
                _statToCore[calculatedStat] = core;

                finalStat.ApplyTo(core.runtimeStat);
                UnitStatRepository.Set(stat.UnitId, stat.UnitName, stat.Rarity, stat.Level, stat.UnitClass, core.runtimeStat.CharacterImage, finalStat);
            }
            return list;
        }

        private List<UnitBaseStat> CalculatePreviewStats(IReadOnlyList<UnitBaseStat> src)
        {
            var list = new List<UnitBaseStat>(src?.Count ?? 0);
            if (src == null) return list;

            foreach (var stat in src)
            {
                var finalStat = StatCalculator.Calculate(stat, _correctionTable, _classBaseStatTable);

                UnitStatRepository.Set(stat.UnitId, stat.UnitName, stat.Rarity, stat.Level, stat.UnitClass, null, finalStat);

                list.Add(new UnitBaseStat
                {
                    UnitId = stat.UnitId,
                    UnitName = stat.UnitName,
                    UnitClass = stat.UnitClass,
                    Level = stat.Level,
                    Rarity = stat.Rarity,
                    BaseAtk = finalStat.AttackDamage,
                    BaseDef = finalStat.Defense,
                    BaseHp = finalStat.MaxHp,
                    BaseAgi = stat.BaseAgi,
                    BaseEvasionRate = finalStat.EvasionRate,
                    CurrentInjury = stat.CurrentInjury
                });
            }
            return list;
        }
    }
}