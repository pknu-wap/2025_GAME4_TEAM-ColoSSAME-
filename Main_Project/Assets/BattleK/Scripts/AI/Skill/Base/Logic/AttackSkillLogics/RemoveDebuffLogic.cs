using BattleK.Scripts.AI.Skill.Base.Logic.LogicBase;
using UnityEngine;
using BattleK.Scripts.AI;

public class RemoveDebuffLogic : ISkillLogic
{
    public void Execute(StaticAICore owner, StaticAICore target)
    {
        if (!target) return;

        target.RemoveAllDebuffs();

        UnityEngine.Debug.Log($"{target.name} 디버프 제거 완료");
    }
}