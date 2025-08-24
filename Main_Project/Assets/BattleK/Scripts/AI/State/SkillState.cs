using System.Collections;
using UnityEngine;

public class SkillState : IState
{
    private readonly AICore ai;
    private readonly SkillData skillData;
    private readonly Transform target;

    public SkillState(AICore ai, SkillData skillData, Transform target)
    {
        this.ai = ai;
        this.skillData = skillData;
        this.target = target;
    }

    public void Enter()
    {
        ai.State = State.Skill;
        ai.skillUse.UseSkill(skillData, ai, target);
    }

    public IEnumerator Execute()
    {
        yield return new WaitForSeconds(0.5f); // 애니메이션 시간
        ai.StateMachine.ChangeState(new IdleState(ai));
    }

    public void Exit() { }
}