using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BattleK.Scripts.Data;
using Newtonsoft.Json;
using UnityEngine;

namespace BattleK.Scripts.Manager
{
    public class CalculateManager : MonoBehaviour
    {
        [Header("스탯 소스 (FamilyStatsCollector)")]
        [Tooltip("씬에 존재하는 FamilyStatsCollector를 Drag&Drop. 비워두면 자동 탐색합니다.")]
        [SerializeField] private FamilyStatsCollector _statsCollector;

        [Header("자동 재시도(유닛이 늦게 스폰될 때 대비)")]
        [Tooltip("처음 갱신에 실패(리스트 0)하면 일정 간격으로 재시도")]
        [SerializeField] private bool _autoRetryIfEmpty = true;

        [Tooltip("재시도 간격(초)")]
        [SerializeField] private float _retryIntervalSeconds = 0.25f;

        [Tooltip("최대 재시도 횟수")]
        [SerializeField] private int _maxRetries = 40;

        [Header("로컬 복사본(읽기 전용 미리보기)")]
        [SerializeField] private List<CharacterStatsRow> _playerStats = new();
        [SerializeField] private List<CharacterStatsRow> _enemyStats  = new();
        [SerializeField] private List<CharacterStatsRow> _allStats    = new();

        public IReadOnlyList<CharacterStatsRow> AllStats => _allStats;

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

        private bool IsEmpty()
            => (_playerStats == null || _playerStats.Count == 0) &&
               (_enemyStats  == null || _enemyStats.Count  == 0);
        private IEnumerator RefreshFromCollectorCoroutine()
        {
            if (_statsCollector == null)
            {
                _statsCollector = FindObjectOfType<FamilyStatsCollector>();
                if (_statsCollector == null)
                {
                    Debug.LogWarning("[CalculateManager] statsCollector가 비어 있습니다. 갱신 불가.");
                    ClearLocal();
                    yield break;
                }
            }
            
            _statsCollector.CollectFromBothTeams();
            yield return null;

            _playerStats = CloneList(_statsCollector.PlayerStats);
            _enemyStats  = CloneList(_statsCollector.EnemyStats);

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

            return searchEnemyToo ? _enemyStats.Find(r => string.Equals(r.Unit_ID, unitId, StringComparison.Ordinal)) : null;
        }
        public string ToJsonPretty(bool includeEnemy = true)
        {
            var pack = new
            {
                player = _playerStats,
                enemy  = includeEnemy ? _enemyStats : null
            };
            return JsonConvert.SerializeObject(pack, Formatting.Indented);
        }

        private void ClearLocal()
        {
            _playerStats = new List<CharacterStatsRow>();
            _enemyStats  = new List<CharacterStatsRow>();
            _allStats    = new List<CharacterStatsRow>();
        }

        private static List<CharacterStatsRow> CloneList(IReadOnlyList<CharacterStatsRow> src)
        {
            var list = new List<CharacterStatsRow>(src?.Count ?? 0);
            if (src == null) return list;

            list.AddRange(from stat in src
            let level = Mathf.Max(1, stat.Level)
            let baseAtk = stat.ATK
            let baseDef = stat.DEF
            let baseHp = stat.HP
            let baseAgi = stat.AGI
            let calcAtk = 20 + (baseAtk * 2 * level)
            let calcDef = 20 + Mathf.RoundToInt(baseDef * 1.5f * level)
            let calcHp = 200 + (baseHp * 10 * level)
            let calcAPS = (float)Math.Round(1 + 3 * (float)baseAgi / ((float)baseAgi + 6), 2)
            select new CharacterStatsRow
            {
                Unit_ID = stat.Unit_ID,
                Unit_Name = stat.Unit_Name,
                Level = level,
                ATK = calcAtk,
                DEF = calcDef,
                HP = calcHp,
                AGI = stat.AGI,
                Rarity = stat.Rarity
            });
            return list;
        }
    }
}
