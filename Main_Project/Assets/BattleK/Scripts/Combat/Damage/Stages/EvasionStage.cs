using System;
using UnityEngine;

namespace BattleK.Scripts.Combat.Damage.Stages
{
    public class EvasionStage : IDamageStage
    {
        public int Order => 300;

        private readonly Func<float> _roll;
        public EvasionStage(Func<float> roll = null)
        {
            _roll = roll ?? (() => UnityEngine.Random.Range(0f, 100f));
        }

        public void Process(DamageContext ctx)
        {
            if (ctx.Request.HasFlag(DamageFlags.IgnoreEvasion)) return;

            var target = ctx.Request.Target;
            if (_roll() >= target.CurrentEvasionRate) return;

            Debug.Log($"{target.name} is evaded");
            ctx.MarkEvaded();
        }
    }
}