using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class DeathState : IState
{
    private readonly AICore ai;

    public DeathState(AICore ai)
    {
        this.ai = ai;
    }
    public void Enter()
    {
        ai.State = State.Death;
        Debug.Log($"{ai.gameObject.name} is death");
        ai.player.SetStateAnimationIndex(PlayerState.DEATH, 0);
        ai.player.PlayStateAnimation(PlayerState.DEATH);
    }

    public IEnumerator Execute()
    {
        yield return new WaitForSeconds(0.4f);
        
        switch (ai.gameObject.layer)
        {
            case (int)TeamUnit.Player:
                ai.aiManager.playerUnits.Remove(ai);
                break;
            case (int)TeamUnit.Enemy:
                ai.aiManager.EnemyUnits.Remove(ai);
                break;
        }
        ai.gameObject.SetActive(false);
        yield return null;
    }

    public void Exit()
    {
        
    }
}
