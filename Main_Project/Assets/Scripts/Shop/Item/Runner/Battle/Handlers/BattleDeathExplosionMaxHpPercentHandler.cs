using BattleK.Scripts.AI;
using UnityEngine;

internal sealed class BattleDeathExplosionMaxHpPercentHandler : IBattleItemEffectHandler
{
    public ItemEffectKind EffectKind => ItemEffectKind.DeathExplosionMaxHpPercent;

    public void Execute(BattleItemEffectRuntime runtime, BattleItemEffectContext context)
    {
        if (runtime.Effect.effectKind != EffectKind) return;
        if (!BattleItemEffectRules.CanTrigger(runtime)) return;

        StaticAICore owner = runtime.Owner;
        if (!owner || owner.runtimeStat == null) return;

        float percent = BattleItemEffectRules.ResolvePercent(runtime.Effect);
        int damage = Mathf.RoundToInt(owner.runtimeStat.MaxHP * percent);
        if (damage <= 0) return;

        foreach (StaticAICore target in context.TargetResolver.GetTargets(owner, ItemEffectTarget.Enemies))
        {
            if (!target || target.IsDead || target.runtimeStat == null) continue;
            if (runtime.Effect.radius > 0f &&
                Vector2.Distance(owner.transform.position, target.transform.position) > runtime.Effect.radius)
                continue;

            target.OnTakeDamage(damage, owner, true);
        }

        runtime.TriggerCount++;
    }
}
