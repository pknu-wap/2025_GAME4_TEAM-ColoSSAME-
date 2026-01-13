using BattleK.Scripts.AI;
using UnityEngine;

public class Targeting
{
    private ITargetingStrategy currentStrategy;

    public Targeting(ITargetingStrategy initialStrategy)
    {
        currentStrategy = initialStrategy;
    }

    public void SetStrategy(ITargetingStrategy newStrategy)
    {
        currentStrategy = newStrategy;
    }

    public Transform GetTarget(AICore ai)
    {
        return currentStrategy.FindTarget(ai);
    }
}