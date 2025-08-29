using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

/// <summary>
/// FamilyStatsCollector가 수집한 Player/Enemy 리스트를 받아 동일 형식으로 보관/노출하는 매니저.
/// - Start에서 한 번 갱신 + 비어 있으면 자동 재시도(타이밍 이슈 대응)
/// - 필요하면 컬렉터의 CollectFromBothTeams()를 먼저 호출해서 최신화한 뒤 읽음
/// - 결과는 깊은 복사로 보관(외부 변경 영향 최소화)
/// - 복사 과정에서 ATK = 20 + (baseATK * 2 * Level) 처럼 가공 가능
/// </summary>
public class CalculateManager : MonoBehaviour
{
    [Header("스탯 소스 (FamilyStatsCollector)")]
    [Tooltip("씬에 존재하는 FamilyStatsCollector를 Drag&Drop. 비워두면 자동 탐색합니다.")]
    public FamilyStatsCollector statsCollector;

    [Header("자동 동작")]
    [Tooltip("Start에서 자동으로 링크/갱신 수행")]
    public bool refreshOnStart = true;

    [Tooltip("Refresh 시 컬렉터의 CollectFromBothTeams()를 먼저 호출")]
    public bool invokeCollectorCollectBeforeRead = true;

    [Header("자동 재시도(유닛이 늦게 스폰될 때 대비)")]
    [Tooltip("처음 갱신에 실패(리스트 0)하면 일정 간격으로 재시도")]
    public bool autoRetryIfEmpty = true;

    [Tooltip("재시도 간격(초)")]
    public float retryIntervalSeconds = 0.25f;

    [Tooltip("최대 재시도 횟수")]
    public int maxRetries = 40;

    [Header("디버그 로그")]
    public bool debugLogging = true;

    [Header("로컬 복사본(읽기 전용 미리보기)")]
    [SerializeField] private List<FamilyStatsCollector.CharacterStatsRow> _playerStats = new();
    [SerializeField] private List<FamilyStatsCollector.CharacterStatsRow> _enemyStats  = new();
    [SerializeField] private List<FamilyStatsCollector.CharacterStatsRow> _allStats    = new();

    // ===== 외부 읽기 전용 접근자 =====
    public IReadOnlyList<FamilyStatsCollector.CharacterStatsRow> PlayerStats => _playerStats;
    public IReadOnlyList<FamilyStatsCollector.CharacterStatsRow> EnemyStats  => _enemyStats;
    public IReadOnlyList<FamilyStatsCollector.CharacterStatsRow> AllStats    => _allStats;

    private void Awake()
    {
        if (statsCollector == null)
        {
            statsCollector = FindObjectOfType<FamilyStatsCollector>();
            if (statsCollector == null)
                Debug.LogWarning("[CalculateManager] FamilyStatsCollector를 씬에서 찾지 못했습니다.");
        }
    }

    private void Start()
    {
        if (refreshOnStart)
            StartCoroutine(RefreshFlow());
    }

    /// <summary>
    /// 수동 호출용 (버튼/다른 스크립트에서)
    /// </summary>
    [ContextMenu("Refresh Now")]
    public void RefreshNow()
    {
        StopAllCoroutines();
        StartCoroutine(RefreshFlow());
    }

    /// <summary>
    /// 1회 갱신 → 비어 있으면 재시도(선택) 코루틴
    /// </summary>
    private IEnumerator RefreshFlow()
    {
        yield return StartCoroutine(RefreshFromCollectorCoroutine());

        if (autoRetryIfEmpty && IsEmpty())
        {
            if (debugLogging) Debug.Log("[CalculateManager] 리스트가 비어 있음 → 자동 재시도 시작");
            int tries = 0;
            while (tries < maxRetries && IsEmpty())
            {
                tries++;
                yield return new WaitForSeconds(retryIntervalSeconds);
                yield return StartCoroutine(RefreshFromCollectorCoroutine());
            }

            if (debugLogging)
            {
                if (IsEmpty())
                    Debug.LogWarning($"[CalculateManager] 자동 재시도 종료(여전히 비어 있음). tries={tries}");
                else
                    Debug.Log($"[CalculateManager] 자동 재시도 중 채워짐. tries={tries}, Player={_playerStats.Count}, Enemy={_enemyStats.Count}");
            }
        }
    }

    private bool IsEmpty()
        => (_playerStats == null || _playerStats.Count == 0) &&
           (_enemyStats  == null || _enemyStats.Count  == 0);

    /// <summary>
    /// 컬렉터에서 최신 리스트를 읽어와 로컬 복사본을 갱신.
    /// 옵션이 켜져 있으면 먼저 CollectFromBothTeams()를 호출해서 최신화.
    /// </summary>
    private IEnumerator RefreshFromCollectorCoroutine()
    {
        if (statsCollector == null)
        {
            statsCollector = FindObjectOfType<FamilyStatsCollector>();
            if (statsCollector == null)
            {
                Debug.LogWarning("[CalculateManager] statsCollector가 비어 있습니다. 갱신 불가.");
                ClearLocal();
                yield break;
            }
        }

        if (invokeCollectorCollectBeforeRead)
        {
            // 최신 수집 요청(일부 시스템은 다음 프레임에 세팅되기도 하므로 한 프레임 양보)
            statsCollector.CollectFromBothTeams();
            yield return null; // 한 프레임 양보(안정성)
        }

        // === 깊은 복사(여기서 가공 가능) ===
        _playerStats = CloneList(statsCollector.PlayerStats);
        _enemyStats  = CloneList(statsCollector.EnemyStats);

        // 합본 리스트
        _allStats = new List<FamilyStatsCollector.CharacterStatsRow>(_playerStats.Count + _enemyStats.Count);
        _allStats.AddRange(_playerStats);
        _allStats.AddRange(_enemyStats);

        if (debugLogging)
        {
            Debug.Log($"[CalculateManager] 갱신 완료: Player={_playerStats.Count}, Enemy={_enemyStats.Count}, All={_allStats.Count}");
            if (_playerStats.Count == 0 || _enemyStats.Count == 0)
            {
                Debug.Log($"[CalculateManager] 힌트) " +
                          $"1) FamilyStatsCollector의 playerUnitsRoot/enemyUnitsRoot가 인스펙터에 바인딩되어 있는지\n" +
                          $"2) 해당 루트 하위 오브젝트에 CharacterID(필수)와 FamilyID(권장)가 붙어 있는지\n" +
                          $"3) 유닛 오브젝트가 활성화되어 있는지(includeInactive=false)\n" +
                          $"4) CharacterImageLoader가 characterKey를 올바로 세팅했는지 확인하세요.");
            }
        }
    }

    /// <summary>
    /// 외부에서 필요하면 즉시 최신화(자동 재시도 없이 1회만).
    /// </summary>
    public void RefreshFromCollectorOnce()
    {
        StopAllCoroutines();
        StartCoroutine(RefreshFromCollectorCoroutine());
    }

    /// <summary>
    /// 특정 유닛 ID(= characterKey = Unit_ID)로 플레이어/적/전체 중에서 검색
    /// </summary>
    public FamilyStatsCollector.CharacterStatsRow FindUnit(string unitId, bool searchEnemyToo = true)
    {
        if (string.IsNullOrWhiteSpace(unitId)) return null;

        var found = _playerStats.Find(r => string.Equals(r.Unit_ID, unitId, StringComparison.Ordinal));
        if (found != null) return found;

        if (searchEnemyToo)
            return _enemyStats.Find(r => string.Equals(r.Unit_ID, unitId, StringComparison.Ordinal));

        return null;
    }

    /// <summary>
    /// 현재 로컬 복사본을 JSON으로 직렬화(디버깅/로그용)
    /// </summary>
    public string ToJsonPretty(bool includeEnemy = true)
    {
        var pack = new
        {
            player = _playerStats,
            enemy  = includeEnemy ? _enemyStats : null
        };
        return JsonConvert.SerializeObject(pack, Formatting.Indented);
    }

    // ===== 내부 유틸 =====

    private void ClearLocal()
    {
        _playerStats = new List<FamilyStatsCollector.CharacterStatsRow>();
        _enemyStats  = new List<FamilyStatsCollector.CharacterStatsRow>();
        _allStats    = new List<FamilyStatsCollector.CharacterStatsRow>();
    }

    private static List<FamilyStatsCollector.CharacterStatsRow> CloneList(IReadOnlyList<FamilyStatsCollector.CharacterStatsRow> src)
    {
        var list = new List<FamilyStatsCollector.CharacterStatsRow>(src?.Count ?? 0);
        if (src == null) return list;

        for (int i = 0; i < src.Count; i++)
        {
            var s = src[i];

            // ★ 여기서 Level을 지역변수로 받아서 사용
            int level = Mathf.Max(1, s.Level);
            int baseAtk = s.ATK;
            int baseDef = s.DEF;
            int baseHp = s.HP;
            int calcAtk = 20 + (baseAtk * 2 * level);
            int calcDef = 20 + Mathf.RoundToInt(baseDef * 1.5f * level);
            int calcHp = 200 + (baseHp * 10 * level);
            
            list.Add(new FamilyStatsCollector.CharacterStatsRow
            {
                Unit_ID   = s.Unit_ID,
                Unit_Name = s.Unit_Name,
                Level     = level,
                ATK       = calcAtk,
                DEF       = calcDef,
                HP        = calcHp,
                AGI       = s.AGI,
                Rarity    = s.Rarity
            });
        }
        return list;
    }
}
