using BattleK.Scripts.AI;
using Shop.Item;
using UnityEngine;

internal sealed class BattlePeriodicHealMaxHpPercentHandler : IBattleItemEffectHandler
{
    public ItemEffectKind EffectKind => ItemEffectKind.PeriodicHealMaxHpPercent;

    public void Execute(BattleItemEffectRuntime runtime, BattleItemEffectContext context)
    {
        Tick(runtime, context, Time.deltaTime);
    }

    public void Tick(BattleItemEffectRuntime runtime, BattleItemEffectContext context, float deltaTime)
    {
        if (runtime.Effect.effectKind != EffectKind) return;
        if (!BattleItemEffectRules.CanTrigger(runtime)) return;

        float interval = Mathf.Max(0.01f, runtime.Effect.tickInterval);
        runtime.TickTimer += deltaTime;
        if (runtime.TickTimer < interval) return;

        runtime.TickTimer = 0f;

        foreach (StaticAICore target in context.TargetResolver.GetTargets(runtime.Owner, runtime.Effect.target))
        {
            if (!target || target.IsDead || target.runtimeStat == null) continue;

            float percent = BattleItemEffectRules.ResolvePercent(runtime.Effect);
            int healAmount = Mathf.RoundToInt(target.runtimeStat.MaxHP * percent);
            if (healAmount <= 0) continue;

            target.OnHeal(healAmount);
        }

        runtime.TriggerCount++;
    }
}
