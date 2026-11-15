using System;

namespace BattleK.Scripts.Combat.Damage.Formulas
{
    [Serializable]
    public class OwnerSkillPointFormula : IDamageFormula
    {
        public float Ratio;

        public float Evaluate(in DamageFormulaContext ctx) => ctx.Owner ? ctx.Owner.runtimeStat.SkillPoint * Ratio : 0f;
    }
}