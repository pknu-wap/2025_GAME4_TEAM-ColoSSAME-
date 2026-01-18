using System.Collections;
using System.Collections.Generic;
using BattleK.Scripts.AI;
using Unity.VisualScripting;
using UnityEngine;

public class DeathState : IState
{
    private readonly AICore ai;
    private bool entered;

    public DeathState(AICore ai)
    {
        this.ai = ai;
    }

    public void Enter()
    {
        if (entered) return;
        entered = true;
        if (ai == null) return;

        // 비활성화 전에 확정
        ai.State = State.Death;
        ai.StopAllActionsHard(); // 공격/스킬/이동 정지

        Debug.Log($"{ai.gameObject.name} is death");
        ai.player?.SetStateAnimationIndex(PlayerState.DEATH, 0);
        ai.player?.PlayStateAnimation(PlayerState.DEATH);

        ai.StartCoroutine(Execute());
    }

    public IEnumerator Execute()
    {
        // 애니 길이 혹은 최소 대기
        float wait = 0.4f;
        var anim = ai.player?._prefabs?._anim;
        if (anim != null) wait = Mathf.Max(0.2f, anim.GetCurrentAnimatorStateInfo(0).length);
        yield return new WaitForSeconds(wait);

        // 매니저에서 제거
        // switch (ai.gameObject.layer)
        // {
        //     case (int)TeamUnit.Player:
        //         ai.aiManager?.playerUnits.Remove(ai);
        //         break;
        //     case (int)TeamUnit.Enemy:
        //         ai.aiManager?.enemyUnits.Remove(ai);
        //         break;
        // }

        if (ai.aiManager != null) ai.aiManager.IsWinner();

        // 비활성화는 마지막
        ai.gameObject.SetActive(false);
        yield return null;
    }

    public void Exit() { }
}