using System.Collections;
using BattleK.Scripts.AI;
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
        if (ai == null || ai.IsDead) return;

        ai.State = State.Skill;

        // 타겟 유효성
        if (target == null || !target.gameObject.activeInHierarchy ||
            (target.GetComponent<AICore>()?.IsDead ?? true))
        {
            ai.StateMachine.ChangeState(new TargetingState(ai));
            return;
        }

        ai.skillUse?.UseSkill(skillData, ai, target);
    }

    public IEnumerator Execute()
    {
        if (ai == null || ai.IsDead) yield break;

        yield return new WaitForSeconds(0.5f); // 애니메이션 시간
        if (ai != null && !ai.IsDead)
            ai.StateMachine.ChangeState(new IdleState(ai));
    }

    public void Exit() { }
}