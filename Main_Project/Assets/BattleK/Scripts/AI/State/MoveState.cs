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
        if (ai == null || ai.IsDead) return;

        ai.State = State.MOVE;

        ai.ResumePathfinding();
        if (ai.aiPath != null) ai.aiPath.canMove = true;

        ai.player.SetStateAnimationIndex(PlayerState.MOVE, 0);
        ai.player.PlayStateAnimation(PlayerState.MOVE);
    }

    public IEnumerator Execute()
    {
        if (ai == null || ai.IsDead) yield break;

        ai.player.PlayStateAnimation(PlayerState.MOVE);

        while (true)
        {
            if (ai == null || ai.IsDead) yield break;

            // 타겟 유효 체크
            if (ai.target == null)
            {
                ai.StateMachine.ChangeState(new IdleState(ai));
                yield break;
            }
            var tc = ai.target.GetComponent<AICore>();
            if (tc == null || tc.IsDead || tc.State == State.Death)
            {
                ai.StateMachine.ChangeState(new IdleState(ai));
                yield break;
            }

            // 방향/바라보기
            Vec = ai.target.transform.position - ai.player.transform.position;
            dirVec = Vec.normalized;
            ai.FaceByDirX(dirVec.x);

            // 스킬 우선
            if (ai.TryUseSkill())
            {
                SkillData skill = ai.skillDatabase.GetSkill(ai.unitClass, 0);
                ai.StateMachine.ChangeState(new SkillState(ai, skill, ai.target));
                yield break;
            }

            // 사거리 도달 + 공격 쿨다운 준비됨 → Attack
            if (ai.attackRange >= Vec.magnitude && ai.CanAttack())
            {
                if (ai.aiPath != null) ai.aiPath.canMove = false;
                ai.StateMachine.ChangeState(new AttackState(ai));
                yield break;
            }

            yield return null;
        }
    }

    public void Exit()
    {
        if (ai == null) return;
        ai.player?._prefabs?._anim?.SetBool("1_Move", false);
    }
}
