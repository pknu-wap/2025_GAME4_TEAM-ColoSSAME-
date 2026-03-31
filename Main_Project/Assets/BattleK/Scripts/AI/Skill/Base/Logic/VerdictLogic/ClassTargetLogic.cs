using BattleK.Scripts.AI.Skill.Base.Logic.LogicBase;
using UnityEngine;
using System;
using System.Collections.Generic;
using BattleK.Scripts.Data.ClassInfo;

namespace BattleK.Scripts.AI.Skill.Base.Logic.VerdictLogic
{
    [Serializable]
    public class ClassTargetLogic : IConditionLogic
    {
        private readonly Collider2D[] _results = new Collider2D[4];
        [SerializeField] private List<UnitClass> TargetClasses; 

        public bool Evaluate(StaticAICore owner, LayerMask targetLayer, Vector2 areaSize, out Transform bestTarget)
        {
            bestTarget = null;
            var size = Physics2D.OverlapBoxNonAlloc(owner.transform.position, areaSize, 0, _results, targetLayer);
        
            for (var i = 0; i < size; i++)
            {
                if (!_results[i].TryGetComponent<StaticAICore>(out var core)) continue;
                if (!TargetClasses.Contains(core.Stat.UnitClass)) continue;
                bestTarget = core.transform;
                return true;
            }
            return false;
        }
    }
}
