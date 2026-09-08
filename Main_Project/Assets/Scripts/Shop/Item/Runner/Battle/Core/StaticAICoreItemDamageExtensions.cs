using BattleK.Scripts.AI;

namespace Shop.Item.Runner.Battle.Core
{
    public static class StaticAICoreItemDamageExtensions
    {
        public static void OnTakeDamage(
            this StaticAICore target,
            int damage,
            StaticAICore attacker,
            bool isPenetrating = false)
        {
            if (!target) return;

            BattleItemEffectRunner.Instance?.RecordDamageSource(target, attacker);
            target.OnTakeDamage(damage, isPenetrating);
        }
    }
}