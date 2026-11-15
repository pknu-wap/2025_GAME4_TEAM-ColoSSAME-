using System;
using UnityEngine;

namespace BattleK.Scripts.Combat.Damage.Formulas
{
    [Serializable]
    public class OwnerAttackFormula : IDamageFormula
    {
        public float Multiplier = 1f;

        [Tooltip("true: 버프 반영 전 기본 공격력, false: 현재 공격력")]
        public bool UseBaseAttack;

        public float Evaluate(in DamageFormulaContext ctx)
        {
            if (!ctx.Owner) return 0f;

            var attack = UseBaseAttack ? ctx.Owner.runtimeStat.AttackDamage : ctx.Owner.CurrentAttackDamage;
            return attack * Multiplier;
        }
    }
}