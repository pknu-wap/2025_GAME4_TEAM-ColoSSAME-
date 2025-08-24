using UnityEngine;

[CreateAssetMenu(menuName = "Battle/Enemy Strategy/Grid (px)", fileName = "ES_Grid")]
public class GridStrategy : EnemyStrategyBase
{
    [Header("Grid 시작/셀 (px)")]
    public Vector2 startPx = Vector2.zero;
    public Vector2 cellPx  = new Vector2(120, 120);

    [Header("행/열")]
    public int rows = 1;
    public int cols = 4;

    [Tooltip("행 우선(true) / 열 우선(false) — 좌표 계산에는 영향 없음")]
    public bool rowMajor = true;

    public override Vector3[] BuildLocalPositions(EnemyStrategyRequest req)
    {
        int want = Mathf.Max(0, req.unitCount);

        // 폴백: rows/cols가 유효하지 않으면 1×N 라인
        int r = rows, c = cols;
        if (r <= 0 || c <= 0)
        {
            r = 1;
            c = Mathf.Max(1, want);
        }

        int capacity = Mathf.Max(0, r * c);
        int n = Mathf.Clamp(want, 0, capacity);
        var outPos = new Vector3[n];

        int idx = 0;
        for (int rr = 0; rr < r && idx < n; rr++)
        {
            for (int cc = 0; cc < c && idx < n; cc++)
            {
                Vector2 px = new Vector2(
                    startPx.x + cc * cellPx.x,
                    startPx.y + rr * cellPx.y
                );
                outPos[idx++] = new Vector3(px.x * req.uiToWorldScale, px.y * req.uiToWorldScale, 0f) + req.baseOffset;
            }
        }
        return outPos;
    }
}