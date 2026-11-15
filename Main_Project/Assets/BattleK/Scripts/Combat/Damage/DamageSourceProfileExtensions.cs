using BattleK.Scripts.AI;

namespace BattleK.Scripts.Combat.Damage
{
    public static class DamageSourceProfileExtensions
    {
        public static DamageRequest CreateRequest(
            this IDamageSourceProfile profile,
            StaticAICore attacker, StaticAICore target, int amount,
            DamageType type = DamageType.Physical,
            DamageFlags extraFlags = DamageFlags.None,
            object origin = null)
            => new DamageRequest(attacker, target, amount, type, extraFlags, profile, origin, 0);

        public static DamageRequest CreateChainedRequest(
            this IDamageSourceProfile profile,
            in DamageResult parent,
            StaticAICore attacker, StaticAICore target, int amount,
            DamageType type = DamageType.Physical,
            DamageFlags extraFlags = DamageFlags.None,
            object origin = null)
            => new DamageRequest(attacker, target, amount, type, extraFlags, profile, origin,
                parent.Request.ChainDepth + 1);
    }
}