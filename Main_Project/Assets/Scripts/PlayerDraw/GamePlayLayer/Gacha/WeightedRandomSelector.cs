using System;
using System.Collections.Generic;
using UnityEngine;

namespace Colosseum.GamePlay.Gacha
{
    /// 가중치 누적합 방식의 범용 랜덤 선택기
    /// 어떤 확률 데이터가 들어오든(직업, 성급 등) 동일하게 재사용
    public static class WeightedRandomSelector
    {
        public static T Select<T>(IReadOnlyList<T> entries, Func<T, float> weightSelector)
        {
            if (entries == null || entries.Count == 0)
                throw new ArgumentException("확률 테이블 is null");

            float totalWeight = 0f;
            for (int i = 0; i < entries.Count; i++)
                totalWeight += weightSelector(entries[i]);

            float roll = UnityEngine.Random.value * totalWeight;

            float cumulative = 0f;
            for (int i = 0; i < entries.Count; i++)
            {
                cumulative += weightSelector(entries[i]);
                if (roll < cumulative)
                    return entries[i];
            }

            return entries[entries.Count - 1]; // 오차 대비
        }
    }
}
