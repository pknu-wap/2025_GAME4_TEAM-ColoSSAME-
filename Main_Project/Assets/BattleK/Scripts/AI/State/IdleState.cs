using System.Collections;
using BattleK.Scripts.AI;
using UnityEngine;

public class IdleState : IState
{
    private readonly AICore ai;

    // 호출자가 원하는 제자리 대기 시간(초). 0이면 다음 프레임부터 판단.
    private readonly float holdSeconds;

    // true면 소프트 정지(경로 재계산 최소화), false면 StopMovementHard 사용
    private readonly bool softStop;

    // 원거리 카이팅 임계
    private const float CloseThresholdRatio = 0.6f;

    public IdleState(AICore ai, float holdSeconds = 0f, bool softStop = false)
    {
        this.ai = ai;
        this.holdSeconds = Mathf.Max(0f, holdSeconds);
        this.softStop = softStop;
    }

    public void Enter()
    {
        if (ai == null || ai.IsDead) return;

        ai.State = State.Idle;

        // Idle 애니메이션
        ai.player.SetStateAnimationIndex(PlayerState.IDLE, 0);
        ai.player.PlayStateAnimation(PlayerState.IDLE);

        // 정지 방식
        if (softStop)
        {
            if (ai.aiPath != null)
            {
                ai.aiPath.canMove = false;
                try { ai.aiPath.isStopped = true; } catch { }
            }
            var rb = ai.GetComponent<Rigidbody2D>();
            if (rb) rb.velocity = Vector2.zero;
        }
        else
        {
            ai.StopMovementHard(alsoZeroMaxSpeed: false);
        }
    }

    public IEnumerator Execute()
    {
        if (ai == null || ai.IsDead) yield break;

        // 요청된 대기 시간만큼 제자리 대기
        if (holdSeconds > 0f) yield return new WaitForSeconds(holdSeconds);
        else yield return null;

        if (ai == null || ai.IsDead) yield break;

        // 타겟 유효성
        if (!IsTargetValid())
        {
            ai.StateMachine.ChangeState(new TargetingState(ai));
            yield break;
        }

        // ✅ 우선 카이팅 판단(원거리 전용): "너무 가까우면" 후퇴
        if (ai.isRanged && IsInRange(ai.attackRange))
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

        // 스킬 우선
        if (ai.TryUseSkill())
        {
            SkillData skill = ai.skillDatabase.GetSkill(ai.unitClass, 0);
            ai.StateMachine.ChangeState(new SkillState(ai, skill, ai.target));
            yield break;
        }

        // ── 삼단 분기 ──
        bool inRange = IsInRange(ai.attackRange);
        if (inRange)
        {
            if (ai.CanAttack())
            {
                // 사거리 안 + 공격 가능 → 바로 공격
                ai.StateMachine.ChangeState(new AttackState(ai));
            }
            else
            {
                // 사거리 안 + 아직 쿨다운 → 남은 쿨다운만큼 제자리 대기(softStop)
                float remain = Mathf.Max(0.05f, ai.RemainingAttackCooldown());
                ai.StateMachine.ChangeState(new IdleState(ai, remain, softStop: true));
            }
        }
        else
        {
            // 사거리 밖 → 추격
            ai.StateMachine.ChangeState(new MoveState(ai));
        }
    }

    public void Exit()
    {
        // 다음 상태에서 이동/액션 제어.
    }

    // ───────── 유틸 ─────────
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
