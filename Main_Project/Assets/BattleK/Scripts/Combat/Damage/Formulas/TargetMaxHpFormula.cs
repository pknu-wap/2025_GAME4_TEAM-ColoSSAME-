using System;

namespace BattleK.Scripts.Combat.Damage.Formulas
{
    [Serializable]
    public class TargetMaxHpFormula : IDamageFormula
    {
        public float Ratio;

        public float Evaluate(in DamageFormulaContext ctx) => ctx.Target ? ctx.Target.runtimeStat.MaxHP * Ratio : 0f;
    }
}