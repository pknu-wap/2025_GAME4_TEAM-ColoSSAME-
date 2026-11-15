using System;
using System.Collections.Generic;
using BattleK.Scripts.Utils;
using UnityEngine;

namespace BattleK.Scripts.Combat.Damage.Formulas
{
    [Serializable]
    public class ProductFormula : IDamageFormula
    {
        [SerializeReference, SelectableReference]
        public List<IDamageFormula> Factors = new();

        public float Evaluate(in DamageFormulaContext ctx)
        {
            var product = 1f;
            foreach (var factor in Factors)
                if (factor != null) product *= factor.Evaluate(ctx);
            return product;
        }
    }
}