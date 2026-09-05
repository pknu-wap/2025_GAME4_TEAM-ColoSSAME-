using UnityEngine;

namespace BattleK.Scripts.AI.StaticScoreState.Targeting
{
    public interface IStaticTargetingStrategy
    {
        Transform FindTarget(StaticAICore ai);
    }
    
    public enum TargetStrategy
    {
        NearestTarget,
        NearestTargetWithClass
    }
}