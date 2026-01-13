using BattleK.Scripts.Manager;
using UnityEngine;

public class SlotPositionSyncer : MonoBehaviour
{
    [Header("PreviewSlots (FormationManager가 움직이는 RT들)")]
    public RectTransform[] previewSlots;

    [Header("실제 Slots (배치용)")]
    public RectTransform[] slots;

    [Header("자동 동기화 옵션")]
    public FormationManager formationManager; // 있으면 이벤트 구독
    public bool autoSyncOnApply = true;

    [Header("적용 옵션")]
    [Tooltip("전달된 positions 길이 < slots 길이일 때 남은 슬롯을 (0,0)으로 초기화할지")]
    public bool resetUnusedSlotsToZero = false;

    private void OnEnable()
    {
        if (autoSyncOnApply && formationManager != null)
            formationManager.FormationApplied += OnFormationApplied; // <-- 시그니처 변경(Vector2[])
    }

    private void OnDisable()
    {
        if (formationManager != null)
            formationManager.FormationApplied -= OnFormationApplied;
    }

    // 이벤트 시그니처: Vector2[] 만 받도록 수정
    private void OnFormationApplied(Vector2[] positions)
    {
        ApplyPositionsToSlots(positions);
    }

    private void ApplyPositionsToSlots(Vector2[] positions)
    {
        if (positions == null || positions.Length == 0) return;
        if (slots == null || slots.Length == 0) return;

        int count = Mathf.Min(slots.Length, positions.Length);
        for (int i = 0; i < count; i++)
        {
            if (slots[i] == null) continue;
            slots[i].anchoredPosition = positions[i];
        }

        if (resetUnusedSlotsToZero && slots.Length > count)
        {
            for (int i = count; i < slots.Length; i++)
            {
                if (slots[i] == null) continue;
                slots[i].anchoredPosition = Vector2.zero;
            }
        }
    }

    [ContextMenu("Copy Preview → Slots (수동)")]
    public void CopyPreviewToSlots()
    {
        if (previewSlots == null || slots == null) return;
        int count = Mathf.Min(previewSlots.Length, slots.Length);
        for (int i = 0; i < count; i++)
        {
            if (previewSlots[i] == null || slots[i] == null) continue;
            slots[i].anchoredPosition = previewSlots[i].anchoredPosition;
        }

        if (resetUnusedSlotsToZero && slots.Length > count)
        {
            for (int i = count; i < slots.Length; i++)
            {
                if (slots[i] == null) continue;
                slots[i].anchoredPosition = Vector2.zero;
            }
        }
    }
}