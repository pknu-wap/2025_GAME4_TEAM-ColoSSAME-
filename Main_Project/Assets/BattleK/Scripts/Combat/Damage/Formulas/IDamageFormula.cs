namespace BattleK.Scripts.Combat.Damage.Formulas
{
    public interface IDamageFormula
    {
        float Evaluate(in DamageFormulaContext ctx);
    }
}