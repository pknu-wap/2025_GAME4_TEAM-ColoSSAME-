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

    void OnEnable()
    {
        if (autoSyncOnApply && formationManager != null)
            formationManager.FormationApplied += OnFormationApplied;
    }

    void OnDisable()
    {
        if (formationManager != null)
            formationManager.FormationApplied -= OnFormationApplied;
    }

    private void OnFormationApplied(FormationType type, Vector2[] positions)
    {
        if (positions == null || slots == null) return;
        int count = Mathf.Min(slots.Length, positions.Length);
        for (int i = 0; i < count; i++)
        {
            if (slots[i] == null) continue;
            slots[i].anchoredPosition = positions[i];
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
    }
}