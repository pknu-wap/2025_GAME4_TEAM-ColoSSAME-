using System.Collections.Generic;
using BattleK.Scripts.AI;
using BattleK.Scripts.AI.Skill.Base.Logic.LogicBase;
using UnityEngine;

public abstract class SkillEffect : MonoBehaviour 
{
    private StaticAICore _owner;
    private List<ISkillLogic> _logics;

    public virtual void Setup(StaticAICore owner, List<ISkillLogic> logics) 
    {
        _owner = owner;
        _logics = logics;
    }

    protected void ApplyLogicsToTarget(StaticAICore target) 
    {
        foreach (var logic in _logics) 
        {
            logic.Execute(_owner, target); 
        }
    }
}