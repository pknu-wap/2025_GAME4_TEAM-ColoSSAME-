using BattleK.Scripts.AI.Skill.Base.Logic.LogicBase;
using UnityEngine;

namespace BattleK.Scripts.AI.Skill.Base.Logic.VerdictLogic
{
    public class AlwaysTrueLogic : IConditionLogic
    {
        public bool Evaluate(StaticAICore owner, LayerMask targetLayer, Vector2 areaSize, out Transform bestTarget)
        {
            bestTarget = owner.gameObject.transform;
            return true;
        }
    }
}
