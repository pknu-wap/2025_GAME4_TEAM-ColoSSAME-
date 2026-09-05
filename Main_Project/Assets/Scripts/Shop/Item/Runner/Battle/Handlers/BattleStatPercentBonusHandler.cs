using BattleK.Scripts.AI;
using Shop.Item;

internal sealed class BattleStatPercentBonusHandler : IBattleItemEffectHandler
{
    public ItemEffectKind EffectKind => ItemEffectKind.StatPercentBonus;

    public void Execute(BattleItemEffectRuntime runtime, BattleItemEffectContext context)
    {
        if (!BattleItemEffectRules.CanTrigger(runtime)) return;

        foreach (StaticAICore target in context.TargetResolver.GetTargets(runtime.Owner, runtime.Effect.target))
        {
            context.StatApplier.ApplyPercentStat(target, runtime, runtime.Effect.percentValue, 1);
        }

        runtime.TriggerCount++;
    }
}
