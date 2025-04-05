using System.Collections;
using System.Collections.Generic;
using Movement.State;
using UnityEngine;

public class DeathState2 : IState
{
    private BattleAI2 ai;
    private StateMachine stateMachine;

    public DeathState2(BattleAI2 ai, StateMachine stateMachine)
    {
        this.ai = ai;
        this.stateMachine = stateMachine;
    }
    public void EnterState()
    {
        Debug.unityLogger.Log("Entered DeathState2");
        ai.GetCharAnimator().Death();
    }

    public IEnumerator ExecuteState()
    {
        Debug.unityLogger.Log("Executed DeathState2");
        yield return new WaitForSeconds(1f);
        ai.KillThis();
    }

    public void ExitState()
    {
        
    }
}
