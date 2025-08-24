using System.Collections;
using UnityEngine;

public class MoveState : IState
{
    private readonly AICore ai;
    private Vector2 Vec;
    private Vector2 dirVec;

    public MoveState(AICore ai) { this.ai = ai; }

    public void Enter()
    {
        ai.State = State.MOVE;

        // 탐색 재개 + 이동 on
        ai.ResumePathfinding();
        if (ai.aiPath != null) ai.aiPath.canMove = true;

        ai.player.SetStateAnimationIndex(PlayerState.MOVE, 0);
        ai.player.PlayStateAnimation(PlayerState.MOVE);
    }

    public IEnumerator Execute()
    {
        ai.player.PlayStateAnimation(PlayerState.MOVE);

        while (true)
        {
            // 1) 타겟 없거나 죽으면 Idle
            if (ai.target == null || ai.target.GetComponent<AICore>().State == State.Death)
            {
                ai.StateMachine.ChangeState(new IdleState(ai));
                yield break;
            }

            // 2) 타겟 방향
            Vec = ai.target.transform.position - ai.player.transform.position;
            dirVec = Vec.normalized;

            // 3) 바라보기(스케일 반전 유틸)
            ai.FaceByDirX(dirVec.x);

            // 4) 스킬 선사용
            if (ai.TryUseSkill())
            {
                SkillData skill = ai.skillDatabase.GetSkill(ai.unitClass, 0);
                ai.StateMachine.ChangeState(new SkillState(ai, skill, ai.target));
                yield break;
            }

            // 5) 사거리 도달 → AttackState
            if (ai.attackRange >= Vec.magnitude)
            {
                if (ai.aiPath != null) ai.aiPath.canMove = false;
                ai.StateMachine.ChangeState(new AttackState(ai));
                yield break;
            }

            yield return null;
        }
    }

    public void Exit() { }
}