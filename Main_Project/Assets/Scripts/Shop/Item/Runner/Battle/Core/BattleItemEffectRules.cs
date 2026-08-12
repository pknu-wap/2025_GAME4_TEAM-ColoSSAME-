using BattleK.Scripts.AI;
using UnityEngine;

internal static class BattleItemEffectRules
{
    public static bool IsBattleTrigger(BattleItemEffectRuntime runtime, ItemEffectTrigger trigger)
    {
        return runtime.Effect.domain == ItemEffectDomain.Battle &&
               runtime.Effect.trigger == trigger;
    }

    public static bool CanTrigger(BattleItemEffectRuntime runtime, int naturalLimit = 0)
    {
        return naturalLimit <= 0 || runtime.TriggerCount < naturalLimit;
    }

    public static float ResolvePercent(ItemEffectDefinition effect)
    {
        if (effect.maxPercentValue > effect.percentValue)
        {
            return Random.Range(effect.percentValue, effect.maxPercentValue);
        }

        return effect.percentValue;
    }

    public static int ResolveReviveHp(StaticAICore owner, ItemEffectDefinition effect)
    {
        if (effect.flatValue > 0f)
            return Mathf.RoundToInt(effect.flatValue);

        float percent = ResolvePercent(effect);
        if (percent > 0f)
            return Mathf.RoundToInt(owner.Stat.MaxHP * percent);

        return owner.Stat.MaxHP;
    }
}
