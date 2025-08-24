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
        if (ai.target == null || ai.target.GetComponent<AICore>().State == State.Death)
        {
            ai.State = State.Targeting;
            ai.target = ai.targeting.GetTarget(ai);
            ai.destinationSetter.target = ai.target;
        }
        ai.StateMachine.ChangeState(new MoveState(ai));
    }

    public IEnumerator Execute()
    {
        yield return null;
    }

    public void Exit()
    {
        
    }
}
