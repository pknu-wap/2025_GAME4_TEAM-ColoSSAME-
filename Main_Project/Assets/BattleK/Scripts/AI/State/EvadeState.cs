using System.Collections;
using System.Collections.Generic;
using BattleK.Scripts.AI;
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
        if (ai == null || ai.IsDead) return;
    }

    public IEnumerator Execute()
    {
        if (ai == null || ai.IsDead) yield break;
        ai.StateMachine.ChangeState(new IdleState(ai));
        yield return null;
    }

    public void Exit() { }
}