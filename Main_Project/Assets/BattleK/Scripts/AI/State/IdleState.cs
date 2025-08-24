using System.Collections;
using UnityEngine;

public class IdleState : IState
{
    private readonly AICore ai;

    public IdleState(AICore ai)
    {
        this.ai = ai;
    }

    public void Enter()
    {
        ai.State = State.Idle;
        ai.player.SetStateAnimationIndex(PlayerState.IDLE, 0);
        ai.player.PlayStateAnimation(PlayerState.IDLE);
    }

    public IEnumerator Execute()
    {
        yield return null;

        if (ai.target == null || ai.target.GetComponent<AICore>().State == State.Death)
        {
            ai.StateMachine.ChangeState(new TargetingState(ai));
            yield break;
        }

        // 스킬 우선
        if (ai.TryUseSkill())
        {
            SkillData skill = ai.skillDatabase.GetSkill(ai.unitClass, 0);
            ai.StateMachine.ChangeState(new SkillState(ai, skill, ai.target));
            yield break;
        }

        ai.StateMachine.ChangeState(new MoveState(ai));
    }

    public void Exit() { }
}