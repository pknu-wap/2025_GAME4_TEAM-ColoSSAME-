using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

/// <summary>
/// - 프리팹(씬 인스턴스)에서 FamilyID.FamilyKey, CharacterID.characterKey를 읽음
/// - Assets/BattleK/Family/{FamilyKey}/{FamilyKey}.json 파일을 열어
///   Characters[] 중 Unit_ID == characterKey 인 항목을 찾아 스탯을 수집
/// - 수집 결과를 CharacterStatsRow 리스트로 제공
/// - 기존 스크립트(User/Unit/UserManager/UserSaveManager/BattleStartUsingSlots) 수정 없이 사용 가능
/// </summary>
public class FamilyStatsCollector : MonoBehaviour
{
    [Header("플레이어/적 유닛이 배치된 월드 루트(= BattleStartUsingSlots 의 playerUnitsRoot/enemyUnitsRoot)")]
    public Transform playerUnitsRoot;
    public Transform enemyUnitsRoot;

    [Header("가문 JSON 루트 템플릿 ( {FAMILY} 토큰 치환 )")]
    [Tooltip("예: Assets/BattleK/Family/{FAMILY}/{FAMILY}.json")]
    public string familyJsonTemplate = "Assets/BattleK/Family/{FAMILY}/{FAMILY}.json";

    [Header("수집 결과 (읽기 전용)")]
    [SerializeField] private List<CharacterStatsRow> playerStats = new();
    [SerializeField] private List<CharacterStatsRow> enemyStats  = new();

    // 캐시: 같은 가문의 JSON은 한 번만 읽기
    private readonly Dictionary<string, FamilyJson> _familyCache = new();

    // ====== 외부에서 사용 가능한 읽기 전용 접근자 ======
    public IReadOnlyList<CharacterStatsRow> PlayerStats => playerStats;
    public IReadOnlyList<CharacterStatsRow> EnemyStats  => enemyStats;

    // ====== 수집 엔트리 ======

    /// <summary>
    /// 두 팀(플레이어/적) 모두 수집. 전투 시작 직후 호출 추천.
    /// </summary>
    public void CollectFromBothTeams()
    {
        playerStats = CollectFromRoot(playerUnitsRoot);
        enemyStats  = CollectFromRoot(enemyUnitsRoot);

        Debug.Log($"[FamilyStatsCollector] Player {playerStats.Count}개, Enemy {enemyStats.Count}개 수집 완료");
        // 여기서 UI 표시/로그/디버깅 등 후처리를 해도 좋음.
    }

    /// <summary>
    /// 특정 루트(팀)에서만 수집
    /// </summary>
    public List<CharacterStatsRow> CollectFromRoot(Transform unitsRoot)
    {
        var result = new List<CharacterStatsRow>();
        if (unitsRoot == null)
            return result;

        // 하위 모든 프리팹 인스턴스를 순회
        var characters = unitsRoot.GetComponentsInChildren<Transform>(includeInactive: false)
                                  .Select(t => t.gameObject)
                                  .Where(go => go.GetComponent<CharacterID>() != null)
                                  .ToList();

        foreach (var go in characters)
        {
            var family = go.GetComponent<FamilyID>();     // 외부 스크립트(이미 존재한다고 전제)
            var cid    = go.GetComponent<CharacterID>();  // 외부 스크립트(이미 존재한다고 전제)

            if (family == null || cid == null)
            {
                // FamilyID 또는 CharacterID가 없으면 스킵
                continue;
            }

            string familyKey    = (family.FamilyKey    ?? string.Empty).Trim();
            string characterKey = (cid.characterKey    ?? string.Empty).Trim();

            if (string.IsNullOrEmpty(familyKey) || string.IsNullOrEmpty(characterKey))
                continue;

            // 가문 JSON 로드(캐시 사용)
            var fj = LoadFamilyJson(familyKey);
            if (fj == null || fj.Characters == null || fj.Characters.Count == 0)
                continue;

            // Unit_ID == characterKey 와 일치하는 데이터 찾기
            var match = fj.Characters.FirstOrDefault(c => string.Equals(c.Unit_ID, characterKey, StringComparison.Ordinal));
            if (match == null)
                continue;

            // 스탯 추출(분포 키가 정확히 ATK/DEF/HP/AGI 라는 전제)
            int atk = match.Stat_Distribution?.ATK ?? 0;
            int def = match.Stat_Distribution?.DEF ?? 0;
            int hp  = match.Stat_Distribution?.HP  ?? 0;
            int agi = match.Stat_Distribution?.AGI ?? 0;

            var row = new CharacterStatsRow
            {
                Unit_ID   = match.Unit_ID,
                Unit_Name = match.Unit_Name,
                ATK = atk, DEF = def, HP = hp, AGI = agi
            };

            result.Add(row);
        }

        return result;
    }

    // ====== JSON 로드/경로 유틸 ======

    private FamilyJson LoadFamilyJson(string familyKey)
    {
        if (string.IsNullOrWhiteSpace(familyKey))
            return null;

        if (_familyCache.TryGetValue(familyKey, out var cached))
            return cached;

        string path = BuildFamilyJsonAbsolutePath(familyKey);
        if (string.IsNullOrEmpty(path) || !File.Exists(path))
        {
            Debug.LogWarning($"[FamilyStatsCollector] 가문 JSON 파일을 찾지 못함: {path}");
            _familyCache[familyKey] = null;
            return null;
        }

        try
        {
            string json = File.ReadAllText(path);
            var fj = JsonConvert.DeserializeObject<FamilyJson>(json);
            _familyCache[familyKey] = fj;
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
        rel = NormalizePath(rel);

        // 절대경로면 그대로
        if (Path.IsPathRooted(rel))
            return rel;

        // "Assets/..." 상대경로 → 프로젝트 루트 기준으로 절대경로 생성
        string assetsPath  = NormalizePath(Application.dataPath); // .../<Project>/Assets
        string projectRoot = NormalizePath(Directory.GetParent(assetsPath).FullName);
        if (rel.StartsWith("Assets/", StringComparison.OrdinalIgnoreCase))
            return NormalizePath(Path.Combine(projectRoot, rel));

        // 그 외 상대경로는 Assets 기준으로 처리
        return NormalizePath(Path.Combine(assetsPath, rel));
    }

    private static string NormalizePath(string p) => string.IsNullOrEmpty(p) ? p : p.Replace('\\', '/');

    // ====== 데이터 구조 ======

    [Serializable]
    public class CharacterStatsRow
    {
        public string Unit_ID;
        public string Unit_Name;
        public int ATK, DEF, HP, AGI;
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
        public int Level;
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
