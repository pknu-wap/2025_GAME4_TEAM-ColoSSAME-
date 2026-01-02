using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using BattleK.Scripts.Manager;
using UnityEngine;
using Newtonsoft.Json;

/// <summary>
/// - 씬 오브젝트의 FamilyID.FamilyKey, CharacterID.characterKey를 읽음
/// - Assets/BattleK/Family/{FAMILY}/{FAMILY}.json 에서 Unit_ID == characterKey 캐릭터를 조회
/// - **레벨은 UnitLoadManager.LoadedUser.myUnits 에서 unitId == characterKey를 Find로 직접 조회**
///   (없으면 Family JSON의 Level → 그래도 없으면 1)
/// - 결과: Rarity(가문 JSON), Level(UserSave), ATK/DEF/HP/AGI(가문 JSON)
/// - 기존 User/UserManager/UserSaveManager/CharacterImageLoader 수정 불필요
/// </summary>
public class FamilyStatsCollector : MonoBehaviour
{
    [Header("플레이어/적 유닛이 배치된 월드 루트(= BattleStartUsingSlots 의 playerUnitsRoot/enemyUnitsRoot)")]
    public Transform playerUnitsRoot;
    public Transform enemyUnitsRoot;

    [Header("가문 JSON 루트 템플릿 ( {FAMILY} 토큰 치환 )")]
    [Tooltip("예: Assets/BattleK/Family/{FAMILY}/{FAMILY}.json")]
    public string familyJsonTemplate = "Assets/BattleK/Family/{FAMILY}/{FAMILY}.json";

    [Header("레벨 소스 (UnitLoadManager)")]
    [Tooltip("씬에 있는 UnitLoadManager를 Drag&Drop. 비워두면 런타임에 자동 탐색합니다.")]
    public UnitLoadManager unitLoadManager;

    [Header("키 매칭 옵션")]
    [Tooltip("true면 unitId/characterKey 비교 시 대소문자 무시")]
    public bool caseInsensitiveMatch = true;

    [Header("디버그 로그")]
    public bool debugLogging = true;

    [Header("수집 결과 (읽기 전용)")]
    [SerializeField] private List<CharacterStatsRow> playerStats = new();
    [SerializeField] private List<CharacterStatsRow> enemyStats  = new();

    // 캐시: 같은 가문의 JSON은 한 번만 읽기
    private readonly Dictionary<string, FamilyJson> _familyCache = new();

    // ====== 외부에서 사용 가능한 읽기 전용 접근자 ======
    public IReadOnlyList<CharacterStatsRow> PlayerStats => playerStats;
    public IReadOnlyList<CharacterStatsRow> EnemyStats  => enemyStats;

    // ====== 퍼블릭 엔트리 ======

    /// <summary>두 팀(플레이어/적) 모두 수집. 전투 시작 직후 호출 추천.</summary>
    public void CollectFromBothTeams()
    {
        EnsureLoaderReady(); // 필요 시 자동 로드 시도

        playerStats = CollectFromRoot(playerUnitsRoot);
        enemyStats  = CollectFromRoot(enemyUnitsRoot);

        if (debugLogging)
            Debug.Log($"[FamilyStatsCollector] Player {playerStats.Count}개, Enemy {enemyStats.Count}개 수집 완료");
    }

    /// <summary>특정 루트(팀)에서만 수집</summary>
    public List<CharacterStatsRow> CollectFromRoot(Transform unitsRoot)
    {
        var result = new List<CharacterStatsRow>();
        if (unitsRoot == null) return result;

        var characters = unitsRoot.GetComponentsInChildren<Transform>(includeInactive: false)
                                  .Select(t => t.gameObject)
                                  .Where(go => go.GetComponent<CharacterID>() != null)
                                  .ToList();

        if (debugLogging) Debug.Log($"[CollectFromRoot] 루트='{unitsRoot.name}', 스캔 유닛 수={characters.Count}");

        foreach (var go in characters)
        {
            var family = go.GetComponent<FamilyID>();
            var cid    = go.GetComponent<CharacterID>();
            if (family == null || cid == null)
            {
                if (debugLogging) Debug.LogWarning($"[CollectFromRoot] '{go.name}' → FamilyID/CharacterID 없음");
                continue;
            }

            string familyKey    = (family.FamilyKey ?? string.Empty).Trim();
            string characterKey = (cid.characterKey ?? string.Empty).Trim();
            if (string.IsNullOrEmpty(familyKey) || string.IsNullOrEmpty(characterKey))
                continue;

            // 가문 JSON 로드
            var fj = LoadFamilyJson(familyKey);
            if (fj == null || fj.Characters == null || fj.Characters.Count == 0)
                continue;

            // Unit_ID 매칭 (정확 일치)
            var comparison = caseInsensitiveMatch ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
            var match = fj.Characters.FirstOrDefault(c => string.Equals(c.Unit_ID?.Trim(), characterKey, comparison));
            if (match == null)
            {
                if (debugLogging) Debug.LogWarning($"[UnitScan] Family JSON에서 Unit_ID 매칭 실패: '{characterKey}'");
                continue;
            }

            // 스탯/레어도
            int atk    = match.Stat_Distribution?.ATK ?? 0;
            int def    = match.Stat_Distribution?.DEF ?? 0;
            int hp     = match.Stat_Distribution?.HP  ?? 0;
            int agi    = match.Stat_Distribution?.AGI ?? 0;
            int rarity = match.Rarity;

            // ===== 레벨 결정: UnitLoadManager → Family JSON(Level) → 1 =====
            int level = ResolveLevelFromUserSave(characterKey, match.Level);

            if (debugLogging)
            {
                Debug.Log($"[Pick] '{match.Unit_ID}' Name='{match.Unit_Name}' " +
                          $"ATK={atk}, DEF={def}, HP={hp}, AGI={agi}, Rarity={rarity}, ChosenLv={level}");
            }

            result.Add(new CharacterStatsRow {
                Unit_ID   = match.Unit_ID,
                Unit_Name = match.Unit_Name,
                ATK = atk, DEF = def, HP = hp, AGI = agi,
                Rarity = rarity,
                Level  = level
            });
        }

        return result;
    }

    // ====== 레벨 조회 ======

    private int ResolveLevelFromUserSave(string characterKey, int familyJsonLevel)
    {
        var comparison = caseInsensitiveMatch ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;

        // 1) UnitLoadManager가 있고, 로드 완료 + myUnits 존재 → Find로 바로 조회
        if (unitLoadManager != null && unitLoadManager.LoadedUser?.myUnits != null)
        {
            var mu = unitLoadManager.LoadedUser.myUnits.Find(u =>
                !string.IsNullOrEmpty(u.unitId) &&
                string.Equals(u.unitId.Trim(), characterKey, comparison));

            if (mu != null)
            {
                int lv = Mathf.Max(1, mu.level);
                if (debugLogging) Debug.Log($"[UserSaveLevel] key='{characterKey}' → FOUND level={lv}");
                return lv;
            }

            if (debugLogging) Debug.LogWarning($"[UserSaveLevel] key='{characterKey}' → NOT FOUND (UserSave에 없음)");
        }
        else
        {
            if (debugLogging) Debug.LogWarning("[UserSaveLevel] UnitLoadManager 미탑재/미로드 상태. Family JSON 레벨로 폴백");
        }

        // 2) Family JSON의 Level 폴백
        if (familyJsonLevel > 0) return familyJsonLevel;

        // 3) 최종 폴백
        return 1;
    }

    private void EnsureLoaderReady()
    {
        if (unitLoadManager == null)
        {
            unitLoadManager = FindObjectOfType<UnitLoadManager>();
        }

        if (unitLoadManager != null && unitLoadManager.LoadedUser == null)
        {
            // UnitLoadManager가 가진 absolutePath 기준으로 직접 로드 시도
            if (!unitLoadManager.TryLoad(out _) && debugLogging)
            {
                Debug.LogWarning($"[FamilyStatsCollector] UnitLoadManager 로드 실패");
            }
        }
    }

    // ====== Family JSON 로드/경로 유틸 ======

    private FamilyJson LoadFamilyJson(string familyKey)
    {
        if (string.IsNullOrWhiteSpace(familyKey)) return null;

        if (_familyCache.TryGetValue(familyKey, out var cached))
            return cached;

        string path = BuildFamilyJsonAbsolutePath(familyKey);
        if (string.IsNullOrEmpty(path) || !File.Exists(path))
        {
            if (debugLogging) Debug.LogWarning($"[FamilyStatsCollector] 가문 JSON 파일을 찾지 못함: {path}");
            _familyCache[familyKey] = null;
            return null;
        }

        try
        {
            string json = File.ReadAllText(path);
            var fj = JsonConvert.DeserializeObject<FamilyJson>(json);
            _familyCache[familyKey] = fj;

            if (debugLogging)
                Debug.Log($"[FamilyJSON] 로드: '{path}', Characters={(fj?.Characters?.Count ?? 0)}");

            return fj;
        }
        catch (Exception ex)
        {
            Debug.LogError($"[FamilyStatsCollector] JSON 로드 예외: {path}\n{ex}");
            _familyCache[familyKey] = null;
            return null;
        }
    }

    private string BuildFamilyJsonAbsolutePath(string familyKey)
    {
        // 템플릿 치환
        string rel = (familyJsonTemplate ?? string.Empty).Replace("{FAMILY}", familyKey);
        return BuildAbsolutePath(rel);
    }

    private static string BuildAbsolutePath(string relPath)
    {
        relPath = NormalizePath(relPath);

        // 절대경로면 그대로
        if (Path.IsPathRooted(relPath))
            return relPath;

        // "Assets/..." 상대경로 → 프로젝트 루트 기준으로 절대경로 생성
        string assetsPath  = NormalizePath(Application.dataPath); // .../<Project>/Assets
        string projectRoot = NormalizePath(Directory.GetParent(assetsPath).FullName);
        if (relPath.StartsWith("Assets/", StringComparison.OrdinalIgnoreCase))
            return NormalizePath(Path.Combine(projectRoot, relPath));

        // 그 외 상대경로는 Assets 기준으로 처리
        return NormalizePath(Path.Combine(assetsPath, relPath));
    }

    private static string NormalizePath(string p) => string.IsNullOrEmpty(p) ? p : p.Replace('\\', '/');

    // ====== 데이터 구조 ======

    [Serializable]
    public class CharacterStatsRow
    {
        public string Unit_ID;
        public string Unit_Name;
        public int ATK, DEF, HP, AGI;
        public float APS;
        public int Rarity;
        public int Level;
    }

    // Family JSON 구조(필요 필드만 선언)
    [Serializable]
    private class FamilyJson
    {
        public string Family_Name;
        public string Family_Description;
        public int id;
        public List<FamilyCharacter> Characters;
    }

    [Serializable]
    private class FamilyCharacter
    {
        public string Unit_ID;
        public string Unit_Name;
        public int Rarity;
        public int Level; // JSON 폴백용
        public string Class;
        public string Description;
        public string Story;
        public StatDistribution Stat_Distribution;
        public FamilyVisuals Visuals;
        public int LV;
        public int EXP;
    }

    [Serializable]
    private class StatDistribution
    {
        public int ATK;
        public int DEF;
        public int HP;
        public int AGI;
    }

    [Serializable]
    private class FamilyVisuals
    {
        public string Portrait;
        public string Prefab;
    }
}
