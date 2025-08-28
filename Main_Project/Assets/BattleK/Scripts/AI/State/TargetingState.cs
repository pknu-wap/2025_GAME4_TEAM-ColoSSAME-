using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetingState : IState
{
    private AICore ai;
    public TargetingState(AICore ai)
    {
        this.ai = ai;
    }

    public void Enter()
    {
        if (ai == null || ai.IsDead)
        {
            return;
        }

        ai.State = State.Targeting;

        // 기존 타겟이 죽었거나 없으면 새로 찾기
        bool need = ai.target == null;
        if (!need)
        {
            var tc = ai.target.GetComponent<AICore>();
            need = (tc == null || tc.IsDead || tc.State == State.Death || !ai.target.gameObject.activeInHierarchy);
        }

        if (need)
        {
            ai.target = ai.targeting.GetTarget(ai);
            ai.destinationSetter.target = ai.target;
        }

        ai.StateMachine.ChangeState(new MoveState(ai));
    }

    public IEnumerator Execute()
    {
        yield return null;
    }

    public void Exit() { }
}