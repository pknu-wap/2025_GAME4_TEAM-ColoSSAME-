using UnityEngine;

namespace Battle.Scripts.Class.Close.Warrior
{
    public class Targeting
    {
        private WarriorAI ai;
        private Collider2D[] results = new Collider2D[10];

        public Targeting(WarriorAI Ai)
        {
            this.ai = Ai;
        }

        public Transform FindNearestEnemy()
        {
            int count = Physics2D.OverlapCircleNonAlloc(ai.transform.position, ai.sightRange, results);

            for (int i = 0; i < count; i++)
            {
                WarriorAI other = results[i].GetComponent<WarriorAI>();
                if (other != null && other.team != ai.team && other.gameObject.activeInHierarchy)
                {
                    ai.CurrentTarget = other.transform;
                    return other.transform;
                }
            }

            return null;
        }
    }

}
