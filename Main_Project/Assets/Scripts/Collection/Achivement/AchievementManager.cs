using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 도감(업적) 시스템 메인 매니저.
/// - DB(AchievementDatabase) 기반으로 "현재 단계"만 노출
/// - 수령 버튼이 진행도 증가 트리거
/// - 진행도는 UserSave.json(achievementProgress)에 저장
/// - 아이템 사용 누적은 usedItemCounts로 검사
/// </summary>
public class AchievementManager : MonoBehaviour
{
    [Header("DB")]
    [SerializeField] private AchievementDatabase database;

    private void Start()
    {
        Debug.Log("✅ AchievementManager.Start() 들어옴");

        if (UserManager.Instance == null) Debug.LogError("❌ UserManager.Instance 없음");
        else Debug.Log("✅ UserManager.Instance 있음");

        if (UserManager.Instance != null)
        {
            Debug.Log($"✅ UserManager.user null? {(UserManager.Instance.user == null)}");
            if (UserManager.Instance.user != null)
                Init(UserManager.Instance.user);
        }
    }

    /// <summary>
    /// 외부(UserManager 등)에서 현재 유저를 받아 쓰기 위한 참조.
    /// 프로젝트에 맞게 연결
    /// </summary>
    public User CurrentUser { get; private set; }

    /// <summary>
    /// 초기화: UserManager.LoadUser() 이후 호출
    /// </summary>
    public void Init(User user)
    {
        CurrentUser = user;

        // null 방지(EnsureDictionaries 활용)
        CurrentUser.EnsureDictionaries();
        
        // ✅ DB 기준으로 progress 키를 미리 만들어 저장
        EnsureAllProgressKeys();
    }
    /// <summary>
    /// DB에 있는 모든 groupId가 UserSave에 존재하도록 0으로 초기화.
    /// (도감 슬롯을 늘렸을 때, 새 도감이 저장 파일에도 바로 생성되게 함)
    /// </summary>
    public void EnsureAllProgressKeys()
    {
        if (CurrentUser == null) return;
        if (database == null || database.groups == null) return;

        CurrentUser.EnsureDictionaries();

        bool changed = false;

        for (int i = 0; i < database.groups.Count; i++)
        {
            var g = database.groups[i];
            if (g == null || string.IsNullOrEmpty(g.groupId)) continue;

            if (!CurrentUser.achievementProgress.ContainsKey(g.groupId))
            {
                CurrentUser.achievementProgress[g.groupId] = 0;
                changed = true;
            }
        }

        // 새 키가 생겼으면 저장까지 한 번 해준다.
        if (changed)
            SaveUser();
    }
    /// <summary>
    /// UI에서 그룹 리스트를 만들 때 사용.
    /// </summary>
    public List<AchievementGroup> GetAllGroups()
    {
        return database.groups;
    }

    /// <summary>
    /// 특정 그룹의 "현재 단계"를 가져온다.
    /// - progress가 없으면 0(1단계)로 간주
    /// - 스테이지 범위를 넘어가면 마지막 이후(=완료) 상태로 처리
    /// </summary>
    public int GetCurrentStageIndex(string groupId)
    {
        if (CurrentUser == null) return 0;

        if (!CurrentUser.achievementProgress.ContainsKey(groupId))
            CurrentUser.achievementProgress[groupId] = 0;

        return CurrentUser.achievementProgress[groupId];
    }

    /// <summary>
    /// 특정 그룹의 "현재 단계" 데이터를 가져온다.
    /// 완료 상태면 null 반환(=더 이상 표시할 단계 없음 처리 가능)
    /// </summary>
    public AchievementStage GetCurrentStage(string groupId)
    {
        AchievementGroup group = database.GetGroup(groupId);
        if (group == null) return null;

        int idx = GetCurrentStageIndex(groupId);
        if (idx < 0) idx = 0;

        // idx가 stages 범위를 넘어가면 완료 상태
        if (idx >= group.stages.Count)
            return null;

        return group.stages[idx];
    }

    /// <summary>
    /// 현재 단계 조건을 만족했는지 검사한다.
    /// </summary>
    public bool IsCurrentStageCompleted(string groupId)
    {
        AchievementStage stage = GetCurrentStage(groupId);
        if (stage == null) return false;

        // 조건이 0개면(실수) 완료 처리하지 않고 false 권장
        if (stage.conditions == null || stage.conditions.Count == 0)
            return false;

        // 기본 정책: AND (모든 조건 만족)
        for (int i = 0; i < stage.conditions.Count; i++)
        {
            if (!CheckCondition(stage.conditions[i]))
                return false;
        }

        return true;
    }

    /// <summary>
    /// 수령 버튼 눌렀을 때 호출.
    /// - 조건 만족 안 하면 false
    /// - 만족하면 골드 지급 + progress++ + 저장 후 true
    /// </summary>
    public bool ClaimReward(string groupId)
    {
        Debug.Log($"[ClaimReward] 시작 groupId={groupId}");

        if (CurrentUser == null) { Debug.LogError("[ClaimReward] CurrentUser null"); return false; }

        AchievementGroup group = database.GetGroup(groupId);
        if (group == null) { Debug.LogError("[ClaimReward] group null"); return false; }

        AchievementStage stage = GetCurrentStage(groupId);
        if (stage == null) { Debug.LogWarning("[ClaimReward] stage null(완료 상태)"); return false; }

        bool completed = IsCurrentStageCompleted(groupId);
        Debug.Log($"[ClaimReward] completed={completed}");

        if (!completed) return false;

        AddGold(stage.rewardGold);
        int idx = GetCurrentStageIndex(groupId);
        CurrentUser.achievementProgress[groupId] = idx + 1;

        SaveUser();

        Debug.Log("[ClaimReward] 성공");
        return true;
    }

    // -------------------------
    // 조건 검사 핵심
    // -------------------------

    /// <summary>
    /// 조건 1개를 검사한다.
    /// 조건 type에 따라 필요한 필드만 사용한다.
    /// </summary>
    private bool CheckCondition(AchievementCondition condition)
    {
        switch (condition.type)
        {
            case AchievementConditionType.AnyUnitReachStar:
                // 아무 유닛이 targetStar 이상이면 true
                return AnyUnitReachStar(condition.targetStar);

            case AchievementConditionType.AllUnitsReachStar:
                // 모든 유닛이 targetStar 이상이면 true
                return AllUnitsReachStar(condition.targetStar);

            case AchievementConditionType.UseItemCountAtLeast:
                // usedItemCounts[itemId] >= targetCount
                return UsedItemCountAtLeast(condition.itemId, condition.targetCount);

            default:
                return false;
        }
    }

    // -------------------------
    // 유닛 성급 조회
    // -------------------------

    /// <summary>
    /// myUnits에서 "현재 보유 유닛들의 성급"을 읽어오는 함수.
    /// 이 프로젝트 구조가 대화에 없어서, 여기는 너 코드에 맞게 확정 구현해야 한다.
    ///
    /// 반환 예시:
    /// - Key: 유닛ID(string 혹은 int를 string으로)
    /// - Value: 그 유닛의 현재 성급(int)
    /// </summary>
    private Dictionary<string, int> GetOwnedUnitStars()
    {
        var dict = new Dictionary<string, int>();

        if (CurrentUser == null || CurrentUser.myUnits == null)
            return dict;

        for (int i = 0; i < CurrentUser.myUnits.Count; i++)
        {
            Unit u = CurrentUser.myUnits[i];
            if (u == null) continue;

            // 유닛 고유 ID
            string unitKey = u.unitId;

            // 성급/레벨
            int star = u.level;

            // 중복 키 방지: 같은 유닛이 여러 번 있을 수 있으면 max로 유지
            if (dict.ContainsKey(unitKey))
                dict[unitKey] = Mathf.Max(dict[unitKey], star);
            else
                dict.Add(unitKey, star);
        }

        return dict;
    }


    private bool AnyUnitReachStar(int targetStar)
    {
        if (targetStar <= 0) return false;

        Dictionary<string, int> stars = GetOwnedUnitStars();
        foreach (var kv in stars)
        {
            if (kv.Value >= targetStar)
                return true;
        }
        return false;
    }

    private bool AllUnitsReachStar(int targetStar)
    {
        if (targetStar <= 0) return false;

        Dictionary<string, int> stars = GetOwnedUnitStars();

        // 정책: "보유 유닛이 0개면" 모든 유닛 달성으로 보지 않는다(=false)
        if (stars.Count == 0) return false;

        foreach (var kv in stars)
        {
            if (kv.Value < targetStar)
                return false;
        }
        return true;
    }

    // -------------------------
    // 아이템 사용 누적 카운트
    // -------------------------

    private bool UsedItemCountAtLeast(int itemId, int targetCount)
    {
        if (itemId <= 0) return false;
        if (targetCount <= 0) return false;

        // usedItemCounts는 key가 "문자열"이라고 했으니 그대로 맞춤
        string key = itemId.ToString();

        if (!CurrentUser.usedItemCounts.ContainsKey(key))
            return false;

        return CurrentUser.usedItemCounts[key] >= targetCount;
    }
    /// 골드 지급 연결 함수.
    private void AddGold(int amount)
    {
        if (amount <= 0) return;
        CurrentUser.AddGold(amount);
    }
    /// 저장 연결 함수.
    private void SaveUser()
    {
        UserManager.Instance.SaveUser(); // 또는 UserManager.SaveUser();
    }
    public AchievementGroup GetGroup(string groupId)
    {
        if (database == null) return null;
        return database.GetGroup(groupId);
    }
}
