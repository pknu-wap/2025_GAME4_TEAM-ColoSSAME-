using BattleK.Scripts.AI;

namespace BattleK.Scripts.Combat.Damage.Formulas
{
    public readonly struct DamageFormulaContext
    {
        public readonly StaticAICore Owner;
        public readonly StaticAICore Target;

        public DamageFormulaContext(StaticAICore owner, StaticAICore target)
        {
            Owner = owner;
            Target = target;
        }
    }
}