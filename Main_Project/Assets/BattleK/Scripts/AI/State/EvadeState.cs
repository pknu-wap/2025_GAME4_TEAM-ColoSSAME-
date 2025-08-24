using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EvadeState : IState
{
    private readonly AICore ai;
    public EvadeState(AICore ai)
    {
        this.ai = ai;
    }
    
    public void Enter()
    {
    }

    public IEnumerator Execute()
    {
        ai.StateMachine.ChangeState(new IdleState(ai));
        yield return null;
    }

    public void Exit()
    {
        
    }
}
