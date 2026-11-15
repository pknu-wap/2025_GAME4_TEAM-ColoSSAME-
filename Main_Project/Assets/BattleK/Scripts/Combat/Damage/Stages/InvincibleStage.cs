namespace BattleK.Scripts.Combat.Damage.Stages
{
    public class InvincibleStage : IDamageStage
    {
        public int Order => 200;

        public void Process(DamageContext ctx)
        {
            if (ctx.Request.HasFlag(DamageFlags.IgnoreInvincible)) return;
            if (ctx.Request.Target.IsInvincible) ctx.MarkBlocked();
        }
    }
}