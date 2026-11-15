using System;

namespace BattleK.Scripts.Combat.Damage
{
    public enum DamageType { Physical, Magical, True }
    public enum DamageDelivery { Direct, Periodic, Derived }

    [Flags]
    public enum DamageFlags
    {
        None = 0,
        IgnoreDefense = 1 << 0,
        IgnoreEvasion = 1 << 1,
        IgnoreShield = 1 << 2,
        IgnoreInvincible = 1 << 3,
        CannotKill = 1 << 4,
        NoHitReaction = 1 << 5,
        NoProc = 1 << 6
    }
}