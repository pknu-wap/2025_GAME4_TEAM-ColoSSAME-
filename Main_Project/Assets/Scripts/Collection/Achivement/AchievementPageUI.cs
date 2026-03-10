using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 도감 페이지 전체(UI) 관리자.
/// - DetailPanel 아래의 슬롯들을 자동 수집
/// - 슬롯 수령 시 전체 슬롯을 즉시 갱신 (다음 단계 자동 표시)
/// </summary>
public class AchievementPageUI : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private AchievementManager achievementManager;

    [Header("Slots Root")]
    [Tooltip("DetailPanel Transform을 드래그")]
    [SerializeField] private Transform detailPanelRoot;

    private readonly List<AchievementSlotUI> slots = new List<AchievementSlotUI>();

    private void Awake()
    {
        RebuildSlots();
    }

    private void OnEnable()
    {
        RefreshAll();
    }

    /// <summary>
    /// Slot을 늘리거나 줄였을 때, DetailPanel 아래에서 Slot 스크립트를 다시 모은다.
    /// (보통 플레이 시작 전에 Slot을 배치하니까 Awake 1회로 충분)
    /// </summary>
    public void RebuildSlots()
    {
        slots.Clear();

        if (detailPanelRoot == null)
        {
            Debug.LogWarning("⚠️ AchievementPageUI: detailPanelRoot(DetailPanel) 연결 필요");
            return;
        }

        detailPanelRoot.GetComponentsInChildren(true, slots);

        for (int i = 0; i < slots.Count; i++)
        {
            if (slots[i] != null)
                slots[i].Bind(this);
        }
    }

    public void RefreshAll()
    {
        if (achievementManager == null)
        {
            Debug.LogWarning("⚠️ AchievementPageUI: achievementManager 연결 필요");
            return;
        }

        for (int i = 0; i < slots.Count; i++)
        {
            if (slots[i] != null)
                slots[i].Refresh(achievementManager);
        }
    }

    /// <summary>
    /// 슬롯에서 수령 버튼 눌렀을 때 호출됨.
    /// progress가 올라가면, 다음 단계가 자동 표시되도록 RefreshAll 한다.
    /// </summary>
    public void TryClaim(string groupId)
    {
        if (achievementManager == null) return;

        bool success = achievementManager.ClaimReward(groupId);
        Debug.Log($"🟢 [PageUI] ClaimReward: {success} / {groupId}");

        // ✅ 핵심: 성공하면 progress가 증가했으니,
        // GetCurrentStage가 다음 단계로 바뀐다 → 자동으로 다음 단계 표시됨
        RefreshAll();
    }

#if UNITY_EDITOR
    [ContextMenu("Rebuild Slots (Editor)")]
    private void EditorRebuild()
    {
        RebuildSlots();
        RefreshAll();
    }
#endif
}