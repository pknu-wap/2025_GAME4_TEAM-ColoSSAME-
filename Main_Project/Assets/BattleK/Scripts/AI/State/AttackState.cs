using System.Collections;
using UnityEngine;

public class AttackState : IState
{
    private readonly AICore ai;
    private float delay;

    // 최근접 적이 사거리 * 이 비율 이하이면 후퇴
    private const float CloseThresholdRatio = 0.6f;

    // GC 없이 최근접 적 탐색용 버퍼
    private static readonly Collider2D[] _results = new Collider2D[32];

    public AttackState(AICore ai) { this.ai = ai; }

    public void Enter()
    {
        ai.State = State.Attack;

        // 1) 애니 전에 바라보기 고정
        if (ai.target != null) ai.FaceToward(ai.target.position);

        // 2) 공격 중 이동 완전 정지 (필요시 alsoZeroMaxSpeed:true)
        ai.StopMovementHard(alsoZeroMaxSpeed: false);

        // 3) 애니 & 공격 실행
        delay = Mathf.Clamp(1f / Mathf.Max(0.01f, ai.attackSpeed), 0.1f, 2f);
        ai.player.SetStateAnimationIndex(PlayerState.ATTACK, 0);
        ai.player.PlayStateAnimation(PlayerState.ATTACK);

        if (ai.isRanged)
        {
            if (ai.rangedAttack != null && ai.target != null)
                ai.rangedAttack.TryAttack(ai.target);
        }
        else
        {
            ai.meleeAttack?.Attack();
        }
    }

    public IEnumerator Execute()
    {
        yield return new WaitForSeconds(delay + (float)ai.UnitAttackDelay / 100f);

        // 원거리: 최근접 적이 너무 가까우면 후퇴
        if (ai.isRanged)
        {
            float nearest = GetNearestEnemyDistance();
            if (nearest >= 0f)
            {
                float closeThreshold = ai.attackRange * CloseThresholdRatio;
                if (nearest <= closeThreshold)
                {
                    ai.ResumePathfinding(); // 후퇴 상태에서 canMove 켤 수 있게
                    ai.StateMachine.ChangeState(new RangedRetreatState(ai));
                    yield break;
                }
            }
        }

        ai.ResumePathfinding();
        ai.StateMachine.ChangeState(new IdleState(ai));
    }

    public void Exit() { }

    // ───────── 내부 유틸 ─────────
    private float GetNearestEnemyDistance()
    {
        float radius = Mathf.Max(ai.sightRange, ai.attackRange) + 1f;
        int size = Physics2D.OverlapCircleNonAlloc(ai.transform.position, radius, _results, ai.targetLayer);

        float min = float.PositiveInfinity;
        var self = ai.transform;

        for (int i = 0; i < size; i++)
        {
            var col = _results[i];
            if (!col) continue;
            if (col.transform == self) continue;

            var otherAI = col.GetComponentInParent<AICore>();
            if (otherAI != null && otherAI.State == State.Death) continue;

            float d = Vector2.Distance(self.position, col.transform.position);
            if (d < min) min = d;
        }

        return float.IsInfinity(min) ? -1f : min;
    }
}
