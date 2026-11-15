namespace BattleK.Scripts.Combat.Damage.Stages
{
    public class ValidateStage : IDamageStage
    {
        public int Order => 100;

        public void Process(DamageContext ctx)
        {
            var target = ctx.Request.Target;
            if (!target || target.IsDead || target.runtimeStat.CurrentHP <= 0 || ctx.Request.Amount <= 0)
            {
                ctx.Ignore();
                return;
            }

            if (target.AiManager && target.AiManager.IsAlreadyDone) ctx.Ignore();
        }
    }
}