using System;
using System.Collections.Generic;
using BattleK.Scripts.Utils;
using UnityEngine;

namespace BattleK.Scripts.Combat.Damage.Formulas
{
    [Serializable]
    public class SumFormula : IDamageFormula
    {
        [SerializeReference, SelectableReference]
        public List<IDamageFormula> Terms = new();

        public float Evaluate(in DamageFormulaContext ctx)
        {
            var sum = 0f;
            foreach (var term in Terms)
                if (term != null) sum += term.Evaluate(ctx);
            return sum;
        }
    }
}