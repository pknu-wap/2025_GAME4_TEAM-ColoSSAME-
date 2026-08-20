using BattleK.Scripts.AI;
using UnityEngine;

internal sealed class BattleLowHpStatPercentBonusHandler : IBattleItemEffectHandler
{
    public ItemEffectKind EffectKind => ItemEffectKind.LowHpStatPercentBonus;

    public void Execute(BattleItemEffectRuntime runtime, BattleItemEffectContext context)
    {
        Update(runtime, context);
    }

    public void Update(BattleItemEffectRuntime runtime, BattleItemEffectContext context)
    {
        if (runtime.Effect.effectKind != EffectKind) return;
        if (!runtime.Owner || runtime.Owner.Stat == null || runtime.Owner.IsDead) return;

        float maxHp = Mathf.Max(1, runtime.Owner.Stat.MaxHP);
        float hpRatio = runtime.Owner.Stat.CurrentHP / maxHp;
        float threshold = Mathf.Clamp01(runtime.Effect.hpThresholdRatio);
        bool shouldBeActive = hpRatio <= threshold;

        if (shouldBeActive && !runtime.IsActive && BattleItemEffectRules.CanTrigger(runtime))
        {
            foreach (StaticAICore target in context.TargetResolver.GetTargets(runtime.Owner, runtime.Effect.target))
            {
                context.StatApplier.ApplyPercentStat(target, runtime, runtime.Effect.percentValue, 1);
            }

            runtime.IsActive = true;
            runtime.TriggerCount++;
        }
        else if (!shouldBeActive && runtime.IsActive)
        {
            foreach (StaticAICore target in context.TargetResolver.GetTargets(runtime.Owner, runtime.Effect.target))
            {
                context.StatApplier.RemovePercentStat(target, runtime);
            }

            runtime.IsActive = false;
        }
    }
}
