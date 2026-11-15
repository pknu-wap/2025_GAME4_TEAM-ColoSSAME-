using System;

namespace BattleK.Scripts.Combat.Damage.Formulas
{
    [Serializable]
    public class FlatValueFormula : IDamageFormula
    {
        public float Value;

        public float Evaluate(in DamageFormulaContext ctx) => Value;
    }
}