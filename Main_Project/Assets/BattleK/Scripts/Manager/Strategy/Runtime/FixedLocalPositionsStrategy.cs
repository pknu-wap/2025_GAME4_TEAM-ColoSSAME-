using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(
    menuName = "Battle/Enemy Strategy/Fixed Local Positions",
    fileName = "ES_Positions")]
public class FixedLocalPositionsStrategy : EnemyStrategyBase
{
    [Header("고정 로컬 좌표 (enemyUnitsRoot 로컬 기준, z는 보통 0)")]
    public Vector3[] localPositions;

    [Header("좌표 사용 옵션")]
    [Tooltip("좌표 순서를 랜덤 셔플할지")]
    public bool shuffle = false;

    [Tooltip("좌표 개수 < 출전 수 일 때 어떻게 채울지")]
    public OverflowMode overflowMode = OverflowMode.RepeatLast;

    [Tooltip("좌표에 baseOffset(전략 요청의 기본 오프셋)을 더할지")]
    public bool addBaseOffset = false;

    [Header("폴백 (좌표가 비었을 때)")]
    [Tooltip("좌표 배열이 비어있으면 라인 생성으로 폴백할지")]
    public bool fallbackToLineIfEmpty = true;
    [Tooltip("폴백 라인의 간격(px). uiToWorldScale로 변환됨")]
    public float fallbackCellPxX = 120f;

    public enum OverflowMode
    {
        /// <summary>마지막 좌표를 계속 반복해서 채움</summary>
        RepeatLast,
        /// <summary>처음부터 순환(Loop)</summary>
        Loop
    }

    public override Vector3[] BuildLocalPositions(EnemyStrategyRequest req)
    {
        int want = Mathf.Max(0, req.unitCount);

        // 1) 고정 로컬 좌표 사용
        if (localPositions != null && localPositions.Length > 0 && want > 0)
        {
            // 소스 풀 준비 (+옵션 셔플)
            var pool = new List<Vector3>(localPositions);
            if (shuffle) Shuffle(pool);

            // baseOffset 적용 여부
            Vector3 add = addBaseOffset ? req.baseOffset : Vector3.zero;

            if (want <= pool.Count)
            {
                // 필요한 수만큼 잘라서 반환
                var outPos = new Vector3[want];
                for (int i = 0; i < want; i++) outPos[i] = pool[i] + add;
                return outPos;
            }
            else
            {
                // 좌표가 모자랄 때 오버플로 채우기
                var outPos = new Vector3[want];
                if (pool.Count == 0) return outPos; // 방어

                switch (overflowMode)
                {
                    case OverflowMode.Loop:
                        for (int i = 0; i < want; i++)
                            outPos[i] = pool[i % pool.Count] + add;
                        break;

                    case OverflowMode.RepeatLast:
                    default:
                        var last = pool[pool.Count - 1] + add;
                        int i2 = 0;
                        for (; i2 < pool.Count; i2++) outPos[i2] = pool[i2] + add;
                        for (; i2 < want; i2++) outPos[i2] = last;
                        break;
                }
                return outPos;
            }
        }

        // 2) 폴백: 좌표가 비었을 때 가운데 정렬 라인
        if (fallbackToLineIfEmpty && want > 0)
            return BuildCenteredLine(want, req.uiToWorldScale, req.baseOffset, fallbackCellPxX);

        // 3) 아무것도 못 만들면 빈 배열
        return new Vector3[0];
    }

    private void Shuffle<T>(IList<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
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
