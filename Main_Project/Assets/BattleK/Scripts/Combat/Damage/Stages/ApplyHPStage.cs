using UnityEngine;

namespace BattleK.Scripts.Combat.Damage.Stages
{
    public class ApplyHpStage : IDamageStage
    {
        public int Order => 700;

        public void Process(DamageContext ctx)
        {
            var target = ctx.Request.Target;

            var toHp = Mathf.Max(0, Mathf.RoundToInt(ctx.Amount) - ctx.ShieldAbsorbed);
            if (ctx.Request.HasFlag(DamageFlags.CannotKill)) toHp = Mathf.Min(toHp, Mathf.Max(0, target.Health.Current - 1));
            var applied = target.Health.Reduce(toHp);
            ctx.AppliedDamage = applied;
            ctx.Overkill = Mathf.Max(0, toHp - applied);
        }
    }
}