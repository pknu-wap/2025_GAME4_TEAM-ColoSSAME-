using UnityEngine;

public static class GachaRoller
{
    public static GachaResult Roll(GachaTableSO table, ItemDatabase itemDb)
    {
        // 테이블 자체가 없으면: 폴백
        if (table == null)
        {
            Debug.LogWarning("⚠️ GachaRoller: table이 null입니다. 폴백 골드 지급");
            return GachaResult.Money(10);
        }

        if (table.entries == null || table.entries.Count == 0)
        {
            // 폴백
            return GachaResult.Money(Mathf.Max(0, table.minGoldReward));
        }

        // 1) 유효 엔트리 가중치 총합
        int totalWeight = 0;

        for (int i = 0; i < table.entries.Count; i++)
        {
            var e = table.entries[i];
            if (e == null) continue;
            if (e.weight <= 0) continue;

            // 아이템인데 DB에 없으면 제외(옵션)
            if (e.rewardType == GachaTableSO.RewardType.Item && table.ignoreInvalidItemId)
            {
                if (itemDb == null) continue;
                if (itemDb.GetById(e.itemId) == null) continue;
            }

            totalWeight += e.weight;
        }

        if (totalWeight <= 0)
        {
            // 유효 엔트리가 없으면 폴백
            return GachaResult.Money(Mathf.Max(0, table.minGoldReward));
        }

        // 2) 랜덤 선택
        int r = Random.Range(0, totalWeight);
        int acc = 0;

        for (int i = 0; i < table.entries.Count; i++)
        {
            var e = table.entries[i];
            if (e == null) continue;
            if (e.weight <= 0) continue;

            if (e.rewardType == GachaTableSO.RewardType.Item && table.ignoreInvalidItemId)
            {
                if (itemDb == null) continue;
                if (itemDb.GetById(e.itemId) == null) continue;
            }

            acc += e.weight;

            if (r < acc)
            {
                // 결과 반환(항상 정상값 보정)
                if (e.rewardType == GachaTableSO.RewardType.Item)
                {
                    int count = Mathf.Max(1, e.itemCount);
                    return GachaResult.Item(e.itemId, count);
                }
                else
                {
                    int gold = Mathf.Max(0, e.goldAmount);
                    return GachaResult.Money(gold);
                }
            }
        }

        // 논리상 거의 안 오지만, 무조건 폴백
        return GachaResult.Money(Mathf.Max(0, table.minGoldReward));
    }
}

public struct GachaResult
{
    public bool isItem;
    public int itemId;
    public int itemCount;
    public int goldAmount;

    public static GachaResult Item(int itemId, int count)
    {
        return new GachaResult
        {
            isItem = true,
            itemId = itemId,
            itemCount = count,
            goldAmount = 0
        };
    }

    public static GachaResult Money(int gold)
    {
        return new GachaResult
        {
            isItem = false,
            itemId = -1,
            itemCount = 0,
            goldAmount = gold
        };
    }
}