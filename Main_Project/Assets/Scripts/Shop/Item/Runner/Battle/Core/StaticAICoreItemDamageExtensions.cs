namespace BattleK.Scripts.AI
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

            global::BattleItemEffectRunner.Instance?.RecordDamageSource(target, attacker);
            target.OnTakeDamage(damage, isPenetrating);
        }
    }
}
