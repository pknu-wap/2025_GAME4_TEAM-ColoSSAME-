using BattleK.Scripts.AI.Skill.Base.Logic.LogicBase;
using UnityEngine;
using BattleK.Scripts.AI;

public class AllyWithDebuffLogic : IConditionLogic
{
    public bool Evaluate(StaticAICore owner, LayerMask targetLayer, Vector2 areaSize, out Transform bestTarget)
    {
        var units = GameObject.FindObjectsOfType<StaticAICore>();

        foreach (var unit in units)
        {
            if (((1 << unit.gameObject.layer) & targetLayer) == 0)
                continue;

            if (unit.HasDebuff())
            {
                bestTarget = unit.transform;
                return true;
            }
        }

        bestTarget = null;
        return false;
    }
}