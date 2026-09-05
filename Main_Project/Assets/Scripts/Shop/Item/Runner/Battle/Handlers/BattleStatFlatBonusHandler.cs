using BattleK.Scripts.AI;

namespace Shop.Item.Runner.Battle.Handlers
{
    internal sealed class BattleStatFlatBonusHandler : IBattleItemEffectHandler
    {
        public ItemEffectKind EffectKind => ItemEffectKind.StatFlatBonus;

        public void Execute(BattleItemEffectRuntime runtime, BattleItemEffectContext context)
        {
            if (!BattleItemEffectRules.CanTrigger(runtime)) return;

            foreach (StaticAICore target in context.TargetResolver.GetTargets(runtime.Owner, runtime.Effect.target))
            {
                context.StatApplier.ApplyFlatStat(target, runtime.Effect, runtime);
            }

            runtime.TriggerCount++;
        }
    }
}