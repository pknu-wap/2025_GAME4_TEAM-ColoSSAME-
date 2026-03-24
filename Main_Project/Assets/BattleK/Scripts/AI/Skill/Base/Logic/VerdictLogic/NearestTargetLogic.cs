using BattleK.Scripts.AI.Skill.Base.Logic.LogicBase;
using UnityEngine;

namespace BattleK.Scripts.AI.Skill.Base.Logic.VerdictLogic
{
    public class NearestTargetLogic : IConditionLogic
    {
        private readonly Collider2D[] _results = new Collider2D[4];
        public bool Evaluate(StaticAICore owner, LayerMask targetLayer, Vector2 areaSize, out Transform bestTarget)
        {
            bestTarget = null;
            var size = Physics2D.OverlapBoxNonAlloc(owner.transform.position, areaSize, 0, _results, targetLayer);
            var closestDistance = float.MaxValue;
            var ownerPos = owner.transform.position;
            for (var i = 0; i < size; i++)
            {
                if (!_results[i].TryGetComponent<StaticAICore>(out var core)) continue;

                var distSqr = (core.transform.position - ownerPos).sqrMagnitude;

                if (!(distSqr < closestDistance)) continue;
                closestDistance = distSqr;
                bestTarget = core.transform;
            }
            return bestTarget;
        }
    }
}
