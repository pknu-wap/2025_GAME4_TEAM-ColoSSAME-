using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// CalculateManager의 가공된 스탯(플레이어/적/전체)을 AICore에 적용하는 어댑터.
/// - Start 1회 적용(옵션) + CalculateManager 변경 감지 후 재적용(옵션)
/// - CharacterID.characterKey(= Unit_ID) 우선 매칭 + 여러 보조 키 시도
/// - 적용 후 각 유닛에 StatsReady.MarkReady() 호출(발사 게이트 해제)
/// - (옵션) 자식 RangedAttack.ownerAI 명시 주입
/// </summary>
[DisallowMultipleComponent]
public class StatAdaptManager : MonoBehaviour
{
    [Header("필수 참조")]
    [Tooltip("씬에 존재하는 CalculateManager를 Drag&Drop. 비워두면 자동 탐색합니다.")]
    public CalculateManager calculateManager;

    [Header("자동 적용")]
    [Tooltip("Start에서 자동으로 전체 AICore에 1회 적용합니다.")]
    public bool applyOnStart = true;

    [Tooltip("CalculateManager 데이터 변화 감지 시 자동 재적용(폴링)")]
    public bool reapplyOnChange = true;

    [Tooltip("reapply 폴링 주기(초)")]
    [Range(0.05f, 5f)]
    public float recheckIntervalSeconds = 0.5f;

    [Tooltip("CalculateManager가 비어 있을 때 대기 최대 시간(초)")]
    public float initialWaitTimeoutSeconds = 10f;

    [Header("대상 검색 옵션")]
    [Tooltip("비활성 오브젝트까지 포함해서 AICore를 찾을지 여부")]
    public bool includeInactiveUnits = false;

    [Header("적용 후 처리")]
    [Tooltip("적용 후 각 유닛에 StatsReady.MarkReady() 호출 (RangedAttack 게이트 해제)")]
    public bool markStatsReady = true;

    [Tooltip("자식 RangedAttack.ownerAI를 명시적으로 주입")]
    public bool bindOwnerToChildRangedAttacks = true;

    [Header("스탯 매핑 스케일(필요시 조정)")]
    [Tooltip("ATK = row.ATK * atkScale")]
    public float atkScale = 1f;

    [Tooltip("DEF = row.DEF * defScale")]
    public float defScale = 1f;

    [Tooltip("HP = row.HP * hpScale (AICore에 maxHP가 없다면 현재 HP를 덮어씌웁니다)")]
    public float hpScale = 1f;

    [Tooltip("AGI = row.AGI * agiScale")]
    public float agiScale = 1f;
    
    [Tooltip("APS = row.APS * apsScale  (APS: Attacks Per Second, 초당 공격 횟수)")]
    public float apsScale = 1f;

    public enum StatMappingMode { Overwrite, Additive, Max, Min }

    [Header("매핑 모드")]
    public StatMappingMode atkMapping = StatMappingMode.Overwrite;
    public StatMappingMode defMapping = StatMappingMode.Overwrite;
    public StatMappingMode hpMapping  = StatMappingMode.Overwrite;

    // ★ 변경: APS도 다른 스탯처럼 매핑할 수 있도록 옵션 노출
    public StatMappingMode apsMapping  = StatMappingMode.Overwrite;

    [Header("디버그")]
    public bool debugLogging = true;
    public bool warnIfRowMissing = true;
    [Tooltip("매칭 실패 시 시도한 키들을 로그로 출력")]
    public bool logTriedKeysOnMiss = true;

    // ── 내부 상태 ────────────────────────────────────────────
    private int _lastStamp = 0;
    private WaitForSeconds _waitCache;

    // CalculateManager.AllStats → 빠른 매칭용 인덱스
    private Dictionary<string, FamilyStatsCollector.CharacterStatsRow> _byUnitId;   // key: Unit_ID (원본)
    private Dictionary<string, FamilyStatsCollector.CharacterStatsRow> _byUnitName; // key: Unit_Name (보조, 소문자 trim)

    private void Awake()
    {
        if (calculateManager == null)
            calculateManager = FindObjectOfType<CalculateManager>();

        _waitCache = new WaitForSeconds(Mathf.Max(0.05f, recheckIntervalSeconds));
    }

    private void Start()
    {
        if (applyOnStart)
            StartCoroutine(ApplyFlow());

        if (reapplyOnChange)
            StartCoroutine(WatchAndReapplyLoop());
    }

    /// <summary>외부 버튼/이벤트에서 즉시 1회 적용</summary>
    [ContextMenu("Apply Now")]
    public void ApplyNow()
    {
        if (!EnsureCalculateManager()) return;

        RebuildIndexIfNeeded();
        ApplyToAllUnits();
        _lastStamp = ComputeStampSafe(calculateManager);
    }

    // ── 메인 플로우 ──────────────────────────────────────────
    private IEnumerator ApplyFlow()
    {
        // 1) CM 대기
        float t = 0f;
        while (!EnsureCalculateManager() && t < initialWaitTimeoutSeconds)
        {
            yield return null;
            t += Time.unscaledDeltaTime;
        }
        if (!EnsureCalculateManager())
        {
            if (debugLogging) Debug.LogWarning("[StatAdaptManager] CalculateManager 없음 → 적용 불가");
            yield break;
        }

        // 2) 데이터 채워질 때까지 잠깐 대기(최대 타임아웃)
        t = 0f;
        while ((calculateManager.AllStats == null || calculateManager.AllStats.Count == 0) &&
               t < initialWaitTimeoutSeconds)
        {
            yield return null;
            t += Time.unscaledDeltaTime;
        }

        // 3) 인덱스 구축 + 1회 적용
        RebuildIndexIfNeeded(force: true);
        ApplyToAllUnits();
        _lastStamp = ComputeStampSafe(calculateManager);
    }

    private IEnumerator WatchAndReapplyLoop()
    {
        while (!EnsureCalculateManager())
            yield return _waitCache;

        while (true)
        {
            var stamp = ComputeStampSafe(calculateManager);
            if (stamp != 0 && stamp != _lastStamp)
            {
                if (debugLogging)
                    Debug.Log($"[StatAdaptManager] 스탯 변경 감지 → 재적용 (prev={_lastStamp}, now={stamp})");

                RebuildIndexIfNeeded(force: true);
                ApplyToAllUnits();
                _lastStamp = stamp;
            }

            yield return _waitCache;
        }
    }

    private bool EnsureCalculateManager()
    {
        if (calculateManager != null) return true;
        calculateManager = FindObjectOfType<CalculateManager>();
        return calculateManager != null;
    }

    // ── 인덱스(사전) 구축 ────────────────────────────────────
    private void RebuildIndexIfNeeded(bool force = false)
    {
        if (!force && _byUnitId != null && _byUnitName != null) return;

        _byUnitId   = new Dictionary<string, FamilyStatsCollector.CharacterStatsRow>(StringComparer.Ordinal);
        _byUnitName = new Dictionary<string, FamilyStatsCollector.CharacterStatsRow>(StringComparer.OrdinalIgnoreCase);

        var rows = calculateManager?.AllStats;
        if (rows == null) return;

        for (int i = 0; i < rows.Count; i++)
        {
            var r = rows[i];
            if (r == null) continue;

            // Unit_ID 인덱스(정규 키)
            if (!string.IsNullOrEmpty(r.Unit_ID))
                _byUnitId[r.Unit_ID] = r;

            // Unit_Name 보조 인덱스(이름 매칭 필요 시)
            if (!string.IsNullOrEmpty(r.Unit_Name))
            {
                var k = NormalizeName(r.Unit_Name);
                if (!_byUnitName.ContainsKey(k))
                    _byUnitName[k] = r;
            }
        }
    }

    private static string NormalizeName(string s)
    {
        return string.IsNullOrWhiteSpace(s) ? string.Empty : s.Trim().ToLowerInvariant();
    }

    private static int ComputeStampSafe(CalculateManager cm)
    {
        if (cm == null || cm.AllStats == null) return 0;
        return ComputeStamp(cm.AllStats);
    }

    /// <summary>간단 해시 스탬프(개수+주요 필드)</summary>
    private static int ComputeStamp(IReadOnlyList<FamilyStatsCollector.CharacterStatsRow> list)
    {
        if (list == null) return 0;
        unchecked
        {
            int acc = 17 ^ list.Count;
            for (int i = 0; i < list.Count; i++)
            {
                var r = list[i];
                if (r == null) continue;
                acc ^= (r.Unit_ID?.GetHashCode() ?? 0);
                acc ^= (r.Level * 397);
                acc ^= (r.ATK   * 131);
                acc ^= (r.DEF   *  67);
                acc ^= (r.HP    *  31);
                acc ^= (r.AGI   *   7);

                // ★ 변경: APS를 해시 스탬프에 포함(APS 변경도 감지)
                //  - 정수/실수 대응: 우선 속성/필드에서 가져오고 없으면 0 처리
                float aps = 0f;
                TryGetAPS(r, out aps);
                acc ^= Mathf.RoundToInt(aps * 1000f); // 소수 안정화를 위해 스케일
            }
            return acc;
        }
    }

    // ── 전체 적용 ────────────────────────────────────────────
    private void ApplyToAllUnits()
    {
        if (calculateManager == null || calculateManager.AllStats == null || calculateManager.AllStats.Count == 0)
        {
            if (debugLogging) Debug.LogWarning("[StatAdaptManager] CalculateManager 데이터가 비었습니다.");
            return;
        }

        if (_byUnitId == null) RebuildIndexIfNeeded(force: true);

        var units = FindObjectsOfType<AICore>(includeInactiveUnits);
        int applied = 0, missed = 0;

        for (int i = 0; i < units.Length; i++)
        {
            var ai = units[i];
            if (ai == null) continue;

            // 후보 키 생성
            var tried = new List<string>(6);
            var row = FindRowFor(ai, tried);

            if (row == null)
            {
                missed++;
                if (warnIfRowMissing)
                {
                    if (logTriedKeysOnMiss)
                        Debug.LogWarning($"[StatAdaptManager] 매칭 실패: {GetPath(ai)}  tried=[{string.Join(", ", tried)}]", ai);
                    else
                        Debug.LogWarning($"[StatAdaptManager] 매칭 실패: {GetPath(ai)}", ai);
                }
                continue;
            }

            ApplyRow(ai, row);
            applied++;

            // ▶ Ready 신호 (RangedAttack 게이트 해제)
            if (markStatsReady)
            {
                var ready = ai.GetComponent<StatsReady>();
                if (ready == null) ready = ai.gameObject.AddComponent<StatsReady>();
                ready.MarkReady();
            }

            // ▶ 자식 RangedAttack에 오너 명시 주입(선택)
            if (bindOwnerToChildRangedAttacks)
            {
                var ras = ai.GetComponentsInChildren<RangedAttack>(true);
                foreach (var ra in ras) ra.ownerAI = ai;
            }
        }

        if (debugLogging)
            Debug.Log($"[StatAdaptManager] 적용 완료 - 대상:{units.Length}, 성공:{applied}, 실패:{missed}");
    }

    // ── Row 찾기(여러 키 시도) ───────────────────────────────
    private FamilyStatsCollector.CharacterStatsRow FindRowFor(AICore ai, List<string> triedKeysCollector = null)
    {
        // 1) CharacterID.characterKey
        var cid = ai.GetComponent<CharacterID>();
        if (cid != null && !string.IsNullOrWhiteSpace(cid.characterKey))
        {
            if (triedKeysCollector != null) triedKeysCollector.Add($"cid.characterKey='{cid.characterKey}'");
            if (_byUnitId.TryGetValue(cid.characterKey, out var r1)) return r1;
        }

        // 2) CharacterID 내부에 Unit_ID/UnitId 유사 필드/프로퍼티(리플렉션)
        if (cid != null)
        {
            if (TryGetStringViaReflection(cid, out var altCidKey, "Unit_ID", "unitId", "UnitId", "unitID"))
            {
                if (triedKeysCollector != null) triedKeysCollector.Add($"cid.reflect='{altCidKey}'");
                if (!string.IsNullOrEmpty(altCidKey) && _byUnitId.TryGetValue(altCidKey, out var r2)) return r2;
            }
        }

        // 3) AICore 안의 characterKey/Unit_ID/UnitId 유사 필드/프로퍼티
        if (TryGetStringViaReflection(ai, out var coreKey, "characterKey", "Unit_ID", "unitId", "UnitId", "unitID"))
        {
            if (triedKeysCollector != null) triedKeysCollector.Add($"core.reflect='{coreKey}'");
            if (!string.IsNullOrEmpty(coreKey) && _byUnitId.TryGetValue(coreKey, out var r3)) return r3;
        }

        // 4) GameObject.name → Unit_ID 직접 매칭
        string nameKey = ai.gameObject.name;
        if (triedKeysCollector != null) triedKeysCollector.Add($"name='{nameKey}'");
        if (!string.IsNullOrEmpty(nameKey) && _byUnitId.TryGetValue(nameKey, out var r4)) return r4;

        // 5) GameObject.name → Unit_Name 보조 매칭(소문자/trim)
        string nameNorm = NormalizeName(nameKey);
        if (triedKeysCollector != null) triedKeysCollector.Add($"name~='{nameNorm}'(Unit_Name)");
        if (!string.IsNullOrEmpty(nameNorm) && _byUnitName.TryGetValue(nameNorm, out var r5)) return r5;

        return null;
    }

    private static bool TryGetStringViaReflection(Component c, out string value, params string[] names)
    {
        value = null;
        var t = c.GetType();
        for (int i = 0; i < names.Length; i++)
        {
            var f = t.GetField(names[i]);
            if (f != null && f.FieldType == typeof(string))
            {
                value = f.GetValue(c) as string;
                if (!string.IsNullOrEmpty(value)) return true;
            }
            var p = t.GetProperty(names[i]);
            if (p != null && p.PropertyType == typeof(string) && p.CanRead)
            {
                value = p.GetValue(c) as string;
                if (!string.IsNullOrEmpty(value)) return true;
            }
        }
        return !string.IsNullOrEmpty(value);
    }

    // ── 실제 매핑 ────────────────────────────────────────────
    private void ApplyRow(AICore ai, FamilyStatsCollector.CharacterStatsRow row)
    {
        // 스케일 + 반올림
        int newAtk = Mathf.RoundToInt(row.ATK * atkScale);
        int newDef = Mathf.RoundToInt(row.DEF * defScale);
        int newHp  = Mathf.RoundToInt(row.HP  * hpScale);
        int newAgi = Mathf.RoundToInt(row.AGI * agiScale);

        ai.attackDamage = MapInt(ai.attackDamage, newAtk, atkMapping);
        ai.def          = MapInt(ai.def,          newDef, defMapping);
        ai.hp           = MapInt(ai.hp,           newHp,  hpMapping);

        // 예시: AGI → 이동속/회피
        //ai.moveSpeed    = MapInt  (ai.moveSpeed,   100 + newAgi * 3, StatMappingMode.Overwrite);
        //ai.evasionRate  = MapFloat(ai.evasionRate, newAgi * 3,       StatMappingMode.Overwrite);

        // ★ 변경: APS를 attackSpeed(초당 공격 횟수)에 "그대로" 매핑
        //   - CalculateManager.CharacterStatsRow에 APS가 없거나 0/음수면 기존 값을 유지(변경 안 함).
        //   - 정수/실수 모두 지원: TryGetAPS에서 알아서 가져옴.
        float incomingAPS;
        if (TryGetAPS(row, out incomingAPS))
        {
            float scaledAPS = incomingAPS * Mathf.Max(0f, apsScale);
            if (scaledAPS > 0f)
            {
                ai.attackSpeed = MapFloat(ai.attackSpeed, scaledAPS, apsMapping);
            }
        }

        if (debugLogging)
        {
            string apsLog = TryGetAPS(row, out var apsVal) ? $"{apsVal * apsScale:0.###}" : "NA";
            Debug.Log($"[StatAdaptManager] {SafeUnitKey(ai)} ← " +
                      $"LV{row.Level} ATK:{newAtk}, DEF:{newDef}, HP:{newHp}, AGI:{row.AGI}, APS:{row.APS} " +
                      $"(APS:{apsLog})  → ms:{ai.moveSpeed}, eva:{ai.evasionRate:0.##}, aspd:{ai.attackSpeed:0.###}",
                      ai);
        }
    }

    // ★ 추가: APS를 안전하게 읽어오는 헬퍼 (정수/실수/다양한 필드명 대응)
    private static bool TryGetAPS(FamilyStatsCollector.CharacterStatsRow row, out float aps)
    {
        aps = 0f;
        if (row == null) return false;

        // 1) 대표적인 필드/프로퍼티 명으로 직접 접근(있다면)
        //    - 프로젝트에 맞게 필요한 이름을 추가하세요.
        //    - 예: APS, Aps, AttacksPerSecond, AttackPerSecond, AttackSpeedAPS ...
        var t = row.GetType();

        // 필드 우선
        string[] fieldNames =
        {
            "APS", "Aps", "AttacksPerSecond", "AttackPerSecond", "AttackPerSeconds",
            "AttackSpeedAPS", "AtkPerSec", "AtkPerSecond"
        };
        for (int i = 0; i < fieldNames.Length; i++)
        {
            var f = t.GetField(fieldNames[i]);
            if (f == null) continue;

            object v = f.GetValue(row);
            if (TryCoerceToFloat(v, out aps)) return aps > 0f;
        }

        // 프로퍼티
        for (int i = 0; i < fieldNames.Length; i++)
        {
            var p = t.GetProperty(fieldNames[i]);
            if (p == null || !p.CanRead) continue;

            object v = p.GetValue(row, null);
            if (TryCoerceToFloat(v, out aps)) return aps > 0f;
        }

        // 2) 없으면 실패
        aps = 0f;
        return false;
    }

    private static bool TryCoerceToFloat(object v, out float result)
    {
        result = 0f;
        if (v == null) return false;

        switch (v)
        {
            case float f: result = f; return true;
            case double d: result = (float)d; return true;
            case int i: result = i; return true;
            case long l: result = l; return true;
            case short s: result = s; return true;
            case byte b: result = b; return true;
            case string str:
                if (float.TryParse(str, out var parsed)) { result = parsed; return true; }
                return false;
            default:
                return false;
        }
    }

    private static int MapInt(int current, int incoming, StatMappingMode mode)
    {
        switch (mode)
        {
            case StatMappingMode.Overwrite: return incoming;
            case StatMappingMode.Additive:  return current + incoming;
            case StatMappingMode.Max:       return Mathf.Max(current, incoming);
            case StatMappingMode.Min:       return Mathf.Min(current, incoming);
            default:                        return incoming;
        }
    }

    private static float MapFloat(float current, float incoming, StatMappingMode mode)
    {
        switch (mode)
        {
            case StatMappingMode.Overwrite: return incoming;
            case StatMappingMode.Additive:  return current + incoming;
            case StatMappingMode.Max:       return Mathf.Max(current, incoming);
            case StatMappingMode.Min:       return Mathf.Min(current, incoming);
            default:                        return incoming;
        }
    }

    // ── 유틸 ────────────────────────────────────────────────
    private static string SafeUnitKey(AICore ai)
    {
        var cid = ai.GetComponent<CharacterID>();
        if (cid != null && !string.IsNullOrWhiteSpace(cid.characterKey))
            return cid.characterKey;
        return ai.gameObject.name;
    }

    private static string GetPath(Component c)
    {
        if (c == null) return "(null)";
        Transform t = c.transform;
        if (t == null) return "(no transform)";
        string path = t.name;
        for (Transform p = t.parent; p != null; p = p.parent)
            path = p.name + "/" + path;
        return path;
    }
}
