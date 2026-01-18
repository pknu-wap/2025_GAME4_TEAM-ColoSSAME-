using Unity.VisualScripting;
using UnityEngine;

namespace BattleK.Scripts.AI.StaticScoreState.Targeting
{
    public class NearestTargetStrategy : IStaticTargetingStrategy
    {
        private readonly Collider2D[] _results;

        public NearestTargetStrategy(int maxBufferSize = 10)
        {
            _results = new Collider2D[maxBufferSize];
        }

        public Transform FindTarget(StaticAICore ai)
        {
            var size = Physics2D.OverlapCircleNonAlloc(ai.transform.position, ai.Stat.SightRange, _results, ai.TargetLayer);

            Transform bestTarget = null;
            var closestDistSqr = Mathf.Infinity;
            var myPos = ai.transform.position;

            for (var i = 0; i < size; i++)
            {
                var col = _results[i];
                
                if (col.gameObject.layer == ai.gameObject.layer) continue;

                var targetAI = col.GetComponent<StaticAICore>();
                
                if (targetAI && targetAI.IsDead) continue;

                var dir = col.transform.position - myPos;
                var distSqr = dir.sqrMagnitude;

                if (!(distSqr < closestDistSqr)) continue;
                closestDistSqr = distSqr;
                bestTarget = col.transform;
            }
            return bestTarget;
        }
    }
}