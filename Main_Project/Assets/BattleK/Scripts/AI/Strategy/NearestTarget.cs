using BattleK.Scripts.AI;
using UnityEngine;

public class NearestTarget : ITargetingStrategy
{
    private Collider2D[] results;

    public NearestTarget(int maxColliderCount = 10)
    {
        results = new Collider2D[maxColliderCount];
    }

    public Transform FindTarget(AICore ai)
    {
        int size = Physics2D.OverlapCircleNonAlloc(ai.transform.position, ai.sightRange, results, ai.targetLayer);

        float minDistance = Mathf.Infinity;
        Transform closest = null;

        for (var i = 0; i < size; i++)
        {
            var other = results[i].GetComponent<AICore>();
            if (other != null && other.gameObject != ai.gameObject)
            {
                if (other.State == State.Death)
                    continue;
                
                float dist = Vector2.Distance(ai.transform.position, other.transform.position);
                if (dist < minDistance)
                {
                    minDistance = dist;
                    closest = other.transform;
                }
            }
        }

        if (closest != null)
        {
            Debug.Log("Nearest: " + closest.name);
        }

        return closest;
    }
}