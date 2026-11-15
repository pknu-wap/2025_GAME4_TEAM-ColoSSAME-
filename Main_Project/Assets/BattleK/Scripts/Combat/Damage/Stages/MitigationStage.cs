using System.Collections.Generic;
using BattleK.Scripts.Combat.Damage.StageRule;
using UnityEngine;

namespace BattleK.Scripts.Combat.Damage.Stages
{
    public class MitigationStage : IDamageStage
    {
        public int Order => 500;

        private readonly IReadOnlyDictionary<DamageType, IMitigationRule> _rules;
        private readonly IMitigationRule _fallback;

        public MitigationStage(
            IReadOnlyDictionary<DamageType, IMitigationRule> rules,
            IMitigationRule fallback)
        {
            _rules = rules;
            _fallback = fallback;
        }

        public void Process(DamageContext ctx)
        {
            if (ctx.Request.HasFlag(DamageFlags.IgnoreDefense)) return;

            var rule = _rules.TryGetValue(ctx.Request.Type, out var found) ? found : _fallback;
            var armor = ctx.Request.Target.CurrentDefense;

            ctx.Amount = (int)Mathf.Max(1f, rule.Mitigate(ctx.Amount, armor));
        }
    }
}