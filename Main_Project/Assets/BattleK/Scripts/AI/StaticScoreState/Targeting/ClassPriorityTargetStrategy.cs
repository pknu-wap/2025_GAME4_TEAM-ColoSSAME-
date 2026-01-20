using System.Collections.Generic;
using UnityEngine;

namespace BattleK.Scripts.AI.StaticScoreState.Targeting
{

    public class ClassPriorityTargetStrategy : IStaticTargetingStrategy
    {
        private readonly Collider2D[] _results;
        private readonly LayerMask _targetLayer;
        private readonly List<UnitClass> _priorityList;

        public ClassPriorityTargetStrategy(LayerMask targetLayer, List<UnitClass> priorityList, int maxBufferSize = 15)
        {
            _targetLayer = targetLayer;
            _priorityList = priorityList;
            _results = new Collider2D[maxBufferSize];
        }

        public Transform FindTarget(StaticAICore ai)
        {
            var size = Physics2D.OverlapCircleNonAlloc(ai.transform.position, ai.Stat.SightRange, _results, _targetLayer);
            var myPos = ai.transform.position;

            foreach (var targetClass in _priorityList)
            {
                Transform bestTargetInClass = null;
                var closestDistSqr = Mathf.Infinity;

                for (var i = 0; i < size; i++)
                {
                    var col = _results[i];
                    if (col.transform == ai.transform) continue;

                    var targetAI = col.GetComponent<StaticAICore>();
                    if (!targetAI || targetAI.IsDead || targetAI.Stat.UnitClass != targetClass) continue;

                    var distSqr = (col.transform.position - myPos).sqrMagnitude;
                    if (!(distSqr < closestDistSqr)) continue;
                    closestDistSqr = distSqr;
                    bestTargetInClass = col.transform;
                }
                if (bestTargetInClass) return bestTargetInClass;
            }
            
            Transform fallbackTarget = null;
            var minFallbackDist = Mathf.Infinity;

            for (var i = 0; i < size; i++)
            {
                var col = _results[i];
                if (col.transform == ai.transform) continue;

                var targetAI = col.GetComponent<StaticAICore>();
                if (targetAI && targetAI.IsDead) continue;

                var distSqr = (col.transform.position - myPos).sqrMagnitude;
                if (!(distSqr < minFallbackDist)) continue;
                minFallbackDist = distSqr;
                fallbackTarget = col.transform;
            }

            return fallbackTarget;
        }
    }
}