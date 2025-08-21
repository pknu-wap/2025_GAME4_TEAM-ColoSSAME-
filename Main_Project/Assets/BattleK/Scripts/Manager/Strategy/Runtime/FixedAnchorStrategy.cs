using UnityEngine;

[CreateAssetMenu(menuName = "Battle/Enemy Strategy/Fixed Anchors (px)", fileName = "ES_FixedAnchors")]
public class FixedAnchorsStrategy : EnemyStrategyBase
{
    [Tooltip("UI AnchoredPosition(px) 목록. 변환: local = px * uiToWorldScale + baseOffset")]
    public Vector2[] anchorsPx;

    [Header("Fallback")]
    [Tooltip("앵커가 비어있으면 라인으로 폴백")]
    public bool fallbackToLineIfEmpty = true;
    public float fallbackCellPxX = 120f;

    public override Vector3[] BuildLocalPositions(EnemyStrategyRequest req)
    {
        int want = Mathf.Max(0, req.unitCount);

        if (anchorsPx != null && anchorsPx.Length > 0 && want > 0)
        {
            int n = Mathf.Min(want, anchorsPx.Length);
            var outPos = new Vector3[n];
            for (int i = 0; i < n; i++)
            {
                Vector2 px = anchorsPx[i];
                outPos[i] = new Vector3(px.x * req.uiToWorldScale, px.y * req.uiToWorldScale, 0f) + req.baseOffset;
            }
            return outPos;
        }

        if (fallbackToLineIfEmpty && want > 0)
        {
            return BuildCenteredLine(want, req.uiToWorldScale, req.baseOffset, fallbackCellPxX);
        }

        return new Vector3[0];
    }

    private Vector3[] BuildCenteredLine(int count, float scale, Vector3 baseOffset, float cellPxX)
    {
        var arr = new Vector3[count];
        float cell = Mathf.Max(1f, cellPxX) * scale;
        float startX = -(count - 1) * 0.5f * cell; // 가운데 정렬
        for (int i = 0; i < count; i++)
        {
            float x = startX + i * cell;
            arr[i] = new Vector3(x, 0f, 0f) + baseOffset;
        }
        return arr;
    }
}