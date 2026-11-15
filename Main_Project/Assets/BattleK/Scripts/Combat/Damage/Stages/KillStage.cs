namespace BattleK.Scripts.Combat.Damage.Stages
{
    public class KillStage : IDamageStage
    {
        public int Order => 800;

        public void Process(DamageContext ctx)
        {
            var target = ctx.Request.Target;
            if (target.Health.Current > 0) return;

            if (target.Revive.TryRevive(out _))
            {
                ctx.Revived = true;
                return;
            }

            ctx.Killed = true;
        }
    }
}