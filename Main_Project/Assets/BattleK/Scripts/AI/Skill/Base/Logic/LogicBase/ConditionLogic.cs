using UnityEngine;

namespace BattleK.Scripts.AI.Skill.Base.Logic.LogicBase
{
    public interface IConditionLogic
    { 
        bool Evaluate(StaticAICore owner, LayerMask targetLayer, Vector2 areaSize, out Transform bestTarget);
    }
}
