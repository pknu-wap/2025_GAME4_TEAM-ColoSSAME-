using UnityEngine;
using BattleK.Scripts.AI.Skill.Base.Logic.LogicBase;
using BattleK.Scripts.AI;

public class AllAllyProcessor : LogicProcessor
{
    public override void StartProcess()
    {
        var allUnits = GameObject.FindObjectsOfType<StaticAICore>();

        foreach (var unit in allUnits)
        {
            if (unit.gameObject.layer != _owner.gameObject.layer)
                continue;

            ApplyLogicsToTarget(unit);
        }
    }
}