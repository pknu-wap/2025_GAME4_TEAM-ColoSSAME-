using UnityEngine;
using System;

public class FormationManager : MonoBehaviour
{
    [Header("미리보기 슬롯(좌표 적용 대상)")]
    public RectTransform[] unitImages; // PreviewSlots

    [Header("옵션")]
    [Tooltip("positions 길이 < unitImages 길이일 때 남은 슬롯을 (0,0)으로 초기화할지")]
    public bool resetUnusedSlotsToZero = false;

    // 포메이션 적용 시 알림: 좌표만 전달
    public event Action<Vector2[]> FormationApplied;

    // 마지막 적용 좌표 캐시
    public Vector2[] LastAppliedPositions { get; private set; }

    /// <summary>
    /// (SO 경로) FormationAsset의 UI 앵커 좌표(px)를 미리보기 슬롯에 적용
    /// </summary>
    public void ApplyFormationAsset(FormationAsset asset)
    {
        if (asset == null)
        {
            Debug.LogWarning("[FormationManager] FormationAsset이 null입니다.");
            return;
        }

        var positions = asset.uiAnchoredPositions;
        if (!ApplyPositionsInternal(positions))
        {
            Debug.LogWarning("[FormationManager] FormationAsset에 좌표가 없거나 unitImages 비어있음.");
            return;
        }

        FormationApplied?.Invoke(positions);
    }

    /// <summary>
    /// (선택) 외부에서 임의 좌표를 직접 적용하고 싶을 때 사용
    /// </summary>
    public void ApplyPositions(Vector2[] positions)
    {
        if (!ApplyPositionsInternal(positions))
        {
            Debug.LogWarning("[FormationManager] 전달된 positions가 비었거나 unitImages 비어있음.");
            return;
        }

        FormationApplied?.Invoke(positions);
    }

    /// <summary>
    /// (공통) 좌표 배열을 미리보기 슬롯에 적용
    /// </summary>
    private bool ApplyPositionsInternal(Vector2[] positions)
    {
        if (positions == null || positions.Length == 0) return false;
        if (unitImages == null || unitImages.Length == 0) return false;

        int count = Mathf.Min(unitImages.Length, positions.Length);
        for (int i = 0; i < count; i++)
        {
            if (unitImages[i] == null) continue;
            unitImages[i].anchoredPosition = positions[i];
        }

        if (resetUnusedSlotsToZero && unitImages.Length > count)
        {
            for (int i = count; i < unitImages.Length; i++)
            {
                if (unitImages[i] == null) continue;
                unitImages[i].anchoredPosition = Vector2.zero;
            }
        }

        LastAppliedPositions = positions;
        return true;
    }
}
