using UnityEngine;
using System;

public class FormationManager : MonoBehaviour
{
    public RectTransform[] unitImages; // PreviewSlots
    public FormationType currentFormation = FormationType.Square;
    public StrategyDB strategyDB;

    // 포메이션 적용 시 알림
    public event Action<FormationType, Vector2[]> FormationApplied;

    // 마지막 적용 좌표 캐시
    public Vector2[] LastAppliedPositions { get; private set; }

    public void ApplyFormation(FormationType type)
    {
        currentFormation = type;

        var positions = strategyDB.GetFormationPositions(type);
        if (positions == null || unitImages == null || unitImages.Length == 0)
        {
            Debug.LogWarning($"[FormationManager] {type} 좌표가 없거나 unitImages 비어있음.");
            return;
        }

        int count = Mathf.Min(unitImages.Length, positions.Length);
        for (int i = 0; i < count; i++)
        {
            if (unitImages[i] == null) continue;
            unitImages[i].anchoredPosition = positions[i];
        }

        LastAppliedPositions = positions;
        FormationApplied?.Invoke(type, positions);
    }

    public void ApplySquare()           => ApplyFormation(FormationType.Square);
    public void ApplyWedge()            => ApplyFormation(FormationType.Wedge);
    public void ApplyInvertedTriangle() => ApplyFormation(FormationType.InvertedTriangle);
}