using UnityEngine;

[CreateAssetMenu(menuName = "Battle/Enemy Strategy/Mirror Player", fileName = "ES_MirrorPlayer")]
public class MirrorPlayerStrategy : EnemyStrategyBase
{
    [Header("Mirror 축")]
    [Tooltip("미러 기준축 X (Enemy 로컬좌표). 비활성 시 (StartCenter.x + EndCenter.x) / 2 를 사용")]
    public bool useCustomMirrorAxisX = false;
    public float customMirrorAxisX = 0f;

    [Header("Fallback (플레이어 목표가 없을 때)")]
    [Tooltip("플레이어 타겟이 없으면 가로 라인 생성으로 폴백")]
    public bool fallbackToLineIfNoPlayer = true;

    [Tooltip("라인 간격 (px)")]
    public float fallbackCellPxX = 120f;

    public override Vector3[] BuildLocalPositions(EnemyStrategyRequest req)
    {
        // 플레이어 목표가 존재하면 미러 계산
        if (req.playerLocalTargetsInEnemySpace != null &&
            req.playerLocalTargetsInEnemySpace.Length > 0 &&
            req.unitCount > 0)
        {
            float axisX = useCustomMirrorAxisX
                ? customMirrorAxisX
                : 0.5f * (req.formationStartCenter.x + req.formationEndCenter.x);

            var src = req.playerLocalTargetsInEnemySpace;
            int n = Mathf.Min(req.unitCount, src.Length);
            var outPos = new Vector3[n];

            for (int i = 0; i < n; i++)
            {
                var p = src[i];
                float dx = axisX - p.x; // p' = axis + (axis - p)
                outPos[i] = new Vector3(axisX + dx, p.y, 0f);
            }
            return outPos;
        }

        // 플레이어 타겟이 없을 때: 폴백
        if (fallbackToLineIfNoPlayer && req.unitCount > 0)
        {
            return BuildCenteredLine(req.unitCount, req.uiToWorldScale, req.baseOffset, fallbackCellPxX);
        }

        // 아무 것도 만들 수 없으면 빈 배열
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
