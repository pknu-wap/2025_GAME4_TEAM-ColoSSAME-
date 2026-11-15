using BattleK.Scripts.AI;

namespace BattleK.Scripts.Combat.Damage
{
    public readonly struct DamageRequest
    {
        public readonly StaticAICore Attacker;
        public readonly StaticAICore Target;
        public readonly int Amount;
        public readonly DamageType Type;
        public readonly DamageFlags Flags;
        public readonly IDamageSourceProfile Profile;
        public readonly object Origin;
        public readonly int ChainDepth;

        public DamageDelivery Delivery =>
            ChainDepth > 0 ? DamageDelivery.Derived : Profile.Delivery;

        public DamageRequest(
            StaticAICore attacker, StaticAICore target, int amount,
            DamageType type, DamageFlags extraFlags,
            IDamageSourceProfile profile, object origin, int chainDepth)
        {
            Profile = profile ?? SimpleDamageSourceProfile.Unknown;
            Attacker = attacker;
            Target = target;
            Amount = amount;
            Type = type;
            Flags = extraFlags | Profile.DefaultFlags;
            Origin = origin;
            ChainDepth = chainDepth;
        }

        public bool HasFlag(DamageFlags flag) => (Flags & flag) != 0;
    }
}