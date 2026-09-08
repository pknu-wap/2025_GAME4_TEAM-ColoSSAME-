using System.Collections.Generic;
using Shop.Item;
using UnityEngine;

public static class BookItemEffectRunner
{
    public static BookItemRewardResult CalculateMatchGold(
        IEnumerable<ItemData> items,
        bool isWin,
        int baseRewardGold,
        int victoryRewardGold)
    {
        int bonusGold = 0;
        int penaltyGold = 0;

        foreach (ItemEffectDefinition effect in GetBookBattleEndEffects(items))
        {
            switch (effect.effectKind)
            {
                case ItemEffectKind.MatchWinGoldPercentBonus:
                    if (isWin)
                    {
                        bonusGold += Mathf.RoundToInt(victoryRewardGold * effect.percentValue);
                    }
                    break;

                case ItemEffectKind.MatchLoseGoldPenalty:
                    if (!isWin)
                    {
                        penaltyGold += ResolveLosePenalty(effect, victoryRewardGold);
                    }
                    break;
            }
        }

        int finalGold = Mathf.Max(0, baseRewardGold + bonusGold - penaltyGold);
        return new BookItemRewardResult(baseRewardGold, bonusGold, penaltyGold, finalGold);
    }

    public static BookItemRewardResult CalculateMatchGold(
        ItemData item,
        bool isWin,
        int baseRewardGold,
        int victoryRewardGold)
    {
        return CalculateMatchGold(ToSingleItemList(item), isWin, baseRewardGold, victoryRewardGold);
    }

    public static int CalculateFinalMatchGold(
        IEnumerable<ItemData> items,
        bool isWin,
        int baseRewardGold,
        int victoryRewardGold)
    {
        return CalculateMatchGold(items, isWin, baseRewardGold, victoryRewardGold).FinalGold;
    }

    private static IEnumerable<ItemEffectDefinition> GetBookBattleEndEffects(IEnumerable<ItemData> items)
    {
        if (items == null) yield break;

        foreach (ItemData item in items)
        {
            if (item == null) continue;

            foreach (ItemEffectDefinition effect in item.GetEffects(ItemEffectDomain.Book, ItemEffectTrigger.BattleEnd))
            {
                if (effect != null)
                    yield return effect;
            }
        }
    }

    private static IEnumerable<ItemData> ToSingleItemList(ItemData item)
    {
        if (item != null) yield return item;
    }

    private static int ResolveLosePenalty(ItemEffectDefinition effect, int victoryRewardGold)
    {
        if (effect.flatValue > 0f)
            return Mathf.RoundToInt(effect.flatValue);

        if (effect.percentValue > 0f)
            return Mathf.RoundToInt(victoryRewardGold * effect.percentValue);

        return victoryRewardGold;
    }
}

public readonly struct BookItemRewardResult
{
    public int BaseGold { get; }
    public int BonusGold { get; }
    public int PenaltyGold { get; }
    public int FinalGold { get; }

    public BookItemRewardResult(int baseGold, int bonusGold, int penaltyGold, int finalGold)
    {
        BaseGold = baseGold;
        BonusGold = bonusGold;
        PenaltyGold = penaltyGold;
        FinalGold = finalGold;
    }
}
