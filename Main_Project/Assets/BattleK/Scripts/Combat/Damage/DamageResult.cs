using BattleK.Scripts.AI;
using UnityEngine;

namespace BattleK.Scripts.Combat.Damage
{
    public readonly struct DamageResult
    {
        public readonly DamageRequest Request;
        public readonly int FinalDamage;
        public readonly int ShieldAbsorbed;
        public readonly int AppliedDamage;
        public readonly int Overkill;
        public readonly bool Evaded;
        public readonly bool Blocked;
        public readonly bool Killed;
        public readonly bool Revived;

        public StaticAICore Attacker => Request.Attacker;
        public StaticAICore Target => Request.Target;

        public DamageResult(DamageContext ctx)
        {
            Request = ctx.Request;
            var dealt = !(ctx.Evaded || ctx.Blocked || ctx.Ignored);
            FinalDamage = dealt ? Mathf.Max(0, Mathf.RoundToInt(ctx.Amount)) : 0;
            ShieldAbsorbed = ctx.ShieldAbsorbed;
            AppliedDamage = ctx.AppliedDamage;
            Overkill = ctx.Overkill;
            Evaded = ctx.Evaded;
            Blocked = ctx.Blocked;
            Killed = ctx.Killed;
            Revived = ctx.Revived;
        }
    }
}