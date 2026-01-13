using System.Collections;
using BattleK.Scripts.AI;
using UnityEngine;

public class AttackState : IState
{
    private readonly AICore ai;

    // 원거리 전용: 너무 붙으면 후퇴 임계(사거리 * 비율)
    private const float CloseThresholdRatio = 0.6f;

    // 탐색 버퍼(유지: 미사용이지만 남겨둠)
    private static readonly Collider2D[] _results = new Collider2D[32];

    private float _postDelay; // (사용안함) 남겨둠

    public AttackState(AICore ai) { this.ai = ai; }

    public void Enter()
    {
        if (ai == null || ai.IsDead) return;

        ai.State = State.Attack;

        // 타겟 유효성/사거리/쿨다운 체크
        if (!IsTargetValid()) { ai.StateMachine.ChangeState(new TargetingState(ai)); return; }
        if (!IsInRange(ai.attackRange)) { ai.StateMachine.ChangeState(new MoveState(ai)); return; }
        if (!ai.CanAttack()) { ai.StateMachine.ChangeState(new MoveState(ai)); return; }

        // 바라보기 + 소프트 정지
        ai.FaceToward(ai.target.position);
        if (ai.aiPath != null)
        {
            ai.aiPath.canMove = false;
            try { ai.aiPath.isStopped = true; } catch { }
        }
        var rb = ai.GetComponent<Rigidbody2D>();
        if (rb) rb.velocity = Vector2.zero;

        // 실제 공격 시도
        bool launched = false;
        if (ai.isRanged)
        {
            if (ai.rangedAttack != null && ai.target != null)
                launched = ai.rangedAttack.TryAttack(ai.target);
        }
        else
        {
            if (ai.meleeAttack != null)
            {
                ai.meleeAttack.Attack();
                launched = true;
            }
        }

        if (!launched)
        {
            ai.StateMachine.ChangeState(new MoveState(ai));
            return;
        }

        // 공격 성공 → 애니 재생 + 쿨다운 스탬프
        ai.player.SetStateAnimationIndex(PlayerState.ATTACK, 0);
        ai.player.PlayStateAnimation(PlayerState.ATTACK);
        ai.StampAttackCooldown();
    }

    public IEnumerator Execute()
    {
        // 공격 애니메이션 유지 시간 동안 제자리
        float hold = Mathf.Max(0f, ai.attackAnimationDelay) + Mathf.Max(0f, (float)ai.UnitAttackDelay / 100f);
        if (hold > 0f) yield return new WaitForSeconds(hold);

        // ✅ 카이팅 분기: 원거리 + 타겟이 너무 가까우면 즉시 후퇴
        if (ai.isRanged && ai.target != null)
        {
            float distSq = (ai.target.position - ai.transform.position).sqrMagnitude;
            float close = ai.attackRange * CloseThresholdRatio;
            float closeSq = close * close;
            if (distSq <= closeSq)
            {
                ai.ResumePathfinding();
                if (ai.aiPath != null) ai.aiPath.canMove = true;
                ai.StateMachine.ChangeState(new RangedRetreatState(ai));
                yield break;
            }
        }

        // 그 외엔 Idle(softStop)로 넘겨서 다음 프레임에 삼단 분기(Idle/Move/Attack)
        ai.StateMachine.ChangeState(new IdleState(ai, 0f, softStop: true));
    }

    public void Exit()
    {
        ai?.meleeAttack?.CancelAll();
        ai?.rangedAttack?.CancelAll();
    }

    // ───────── 내부 유틸 ─────────
    private bool IsTargetValid()
    {
        if (ai.target == null || !ai.target.gameObject.activeInHierarchy) return false;
        var other = ai.target.GetComponent<AICore>();
        if (other == null || other.IsDead || other.State == State.Death) return false;
        return true;
    }

    private bool IsInRange(float range)
    {
        if (ai.target == null) return false;
        float r = range + 0.01f;
        return (ai.target.position - ai.transform.position).sqrMagnitude <= r * r;
    }
}
