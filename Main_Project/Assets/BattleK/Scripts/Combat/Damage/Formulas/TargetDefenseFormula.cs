using System;

namespace BattleK.Scripts.Combat.Damage.Formulas
{
    [Serializable]
    public class TargetDefenseFormula : IDamageFormula
    {
        public float Ratio;

        public float Evaluate(in DamageFormulaContext ctx) => ctx.Target ? ctx.Target.runtimeStat.Defense * Ratio : 0f;
    }
}