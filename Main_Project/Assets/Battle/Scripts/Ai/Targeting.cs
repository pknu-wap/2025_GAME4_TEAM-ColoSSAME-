using UnityEngine;

namespace Battle.Scripts.Ai
{
    public class Targeting
    {
        private BattleAI ai;
        private Collider2D[] results = new Collider2D[10];

        public Targeting(BattleAI Ai)
        {
            this.ai = Ai;
        }

        public Transform FindNearestEnemy()
        {
            int count = Physics2D.OverlapCircleNonAlloc(ai.transform.position, ai.sightRange, results);

            float shortestDistance = Mathf.Infinity;
            Transform nearestTarget = null;

            for (int i = 0; i < count; i++)
            {
                BattleAI other = results[i].GetComponent<BattleAI>();
                if (other != null && other.team != ai.team && other.gameObject.activeInHierarchy)
                {
                    float distance = Vector2.Distance(ai.transform.position, other.transform.position);
                    if (distance < shortestDistance)
                    {
                        shortestDistance = distance;
                        nearestTarget = other.transform;
                    }
                }
            }

            if (nearestTarget != null)
            { 
                ai.CurrentTarget = nearestTarget; 
                ai.destinationSetter.target = nearestTarget;
            }

            return nearestTarget;
        }
    }
}
