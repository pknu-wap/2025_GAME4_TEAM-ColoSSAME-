using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using BattleK.Scripts.AI;

public class NearestClassTargeting : ITargetingStrategy
{
    private Collider2D[] results;
    private List<UnitClass> targetClassPriority;

    public NearestClassTargeting(List<UnitClass> targetClassPriority, int maxColliderCount = 10)
    {
        this.targetClassPriority = targetClassPriority;
        results = new Collider2D[maxColliderCount];
    }

    public Transform FindTarget(AICore ai)
    {
        int size = Physics2D.OverlapCircleNonAlloc(ai.transform.position, ai.sightRange, results, ai.targetLayer);

        List<AICore> candidates = new List<AICore>();
        for (var i = 0; i < size; i++)
        {
            var other = results[i].GetComponent<AICore>();
            if (other != null && other.gameObject != ai.gameObject && other.State != State.Death)
            {
                candidates.Add(other);
            }
        }

        foreach (var unitClass in targetClassPriority)
        {
            AICore match = candidates
                .Where(c => c.unitClass == unitClass)
                .OrderBy(c => Vector2.Distance(ai.transform.position, c.transform.position))
                .FirstOrDefault();
            if(match != null)
                return match.transform;
        }

        AICore fallback = candidates
            .OrderBy(c => Vector2.Distance(ai.transform.position, c.transform.position))
            .FirstOrDefault();
        
        return fallback != null ? fallback.transform : null;
    }
}