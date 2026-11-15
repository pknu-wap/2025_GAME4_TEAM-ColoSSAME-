using BattleK.Scripts.AI;
using BattleK.Scripts.Combat.Damage;
using UnityEngine;

namespace Shop.Item.Runner.Battle.Handlers
{
    internal sealed class BattleDeathExplosionMaxHpPercentHandler : IBattleItemEffectHandler
    {
        public ItemEffectKind EffectKind => ItemEffectKind.DeathExplosionMaxHpPercent;
        private static readonly IDamageSourceProfile Profile = new SimpleDamageSourceProfile("Item.DeathExplosion", "Item", DamageDelivery.Direct, DamageFlags.NoProc);

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

                target.ReceiveDamage(Profile.CreateRequest(owner, target, damage, extraFlags: DamageFlags.IgnoreDefense, origin: runtime.Effect));
            }

            runtime.TriggerCount++;
        }
    }
}
