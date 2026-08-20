using System.Collections.Generic;
using BattleK.Scripts.AI;

internal sealed class BattleStatFlatBonusHandler : IBattleItemEffectHandler
{
    public ItemEffectKind EffectKind => ItemEffectKind.StatFlatBonus;

    public void Execute(BattleItemEffectRuntime runtime, BattleItemEffectContext context)
    {
        if (!BattleItemEffectRules.CanTrigger(runtime)) return;

        HashSet<StaticAICore> touchedUnits = new HashSet<StaticAICore>();

        foreach (StaticAICore target in context.TargetResolver.GetTargets(runtime.Owner, runtime.Effect.target))
        {
            if (context.StatApplier.ApplyFlatStat(target, runtime.Effect))
            {
                touchedUnits.Add(target);
            }
        }

        foreach (StaticAICore touchedUnit in touchedUnits)
        {
            context.StatApplier.RefreshUnit(touchedUnit);
        }

        runtime.TriggerCount++;
    }
}
