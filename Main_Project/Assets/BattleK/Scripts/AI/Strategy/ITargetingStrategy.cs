using UnityEngine;

public interface ITargetingStrategy
{
    Transform FindTarget(AICore ai);
}