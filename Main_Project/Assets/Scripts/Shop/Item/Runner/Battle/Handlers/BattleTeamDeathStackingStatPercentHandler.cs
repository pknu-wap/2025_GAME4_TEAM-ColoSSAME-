using BattleK.Scripts.AI;
using Shop.Item;

internal sealed class BattleTeamDeathStackingStatPercentHandler : IBattleItemEffectHandler
{
    public ItemEffectKind EffectKind => ItemEffectKind.TeamDeathStackingStatPercentBonus;

    public void Execute(BattleItemEffectRuntime runtime, BattleItemEffectContext context)
    {
        if (runtime.Effect.effectKind != EffectKind) return;
        if (!BattleItemEffectRules.CanTrigger(runtime)) return;

        runtime.TriggerCount++;

        foreach (StaticAICore target in context.TargetResolver.GetTargets(runtime.Owner, runtime.Effect.target))
        {
            context.StatApplier.ApplyPercentStat(
                target,
                runtime,
                runtime.Effect.percentValue,
                runtime.TriggerCount);
        }
    }
}
