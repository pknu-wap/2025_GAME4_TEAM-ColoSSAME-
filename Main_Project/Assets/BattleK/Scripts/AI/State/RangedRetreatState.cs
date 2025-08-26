using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

public class RangedRetreatState : IState
{
    private readonly AICore ai;

    // 튜닝값
    private const float retreatDuration  = 0.40f;  // 물러나는 시간
    private const float retreatStep      = 2.0f;   // 한 번에 물러날 거리
    private const float repathInterval   = 0.12f;  // 후퇴 중 목적지 재계산 주기
    private const float comfortableRatio = 0.85f;  // 확보하고 싶은 거리 = 사거리 * 비율
    private const float obstacleProbe    = 0.5f;   // 장애물 체크 여유

    private float _timer, _repathT;
    private static readonly Collider2D[] _results = new Collider2D[32];

    public RangedRetreatState(AICore ai) { this.ai = ai; }

    public void Enter()
    {
        if (ai == null || ai.IsDead) return;

        ai.State = State.MOVE;

        // 카이팅 동안 타겟 자동 추적 OFF, 탐색/이동 ON
        ai.ResumePathfinding();
        if (ai.destinationSetter != null) ai.destinationSetter.target = null;
        if (ai.aiPath != null)
        {
            ai.aiPath.canMove = true;
            ai.aiPath.canSearch = true;
        }

        ai.player.SetStateAnimationIndex(PlayerState.MOVE, 0);
        ai.player.PlayStateAnimation(PlayerState.MOVE);

        // 최초 후퇴 목적지
        Vector3 first = ComputeRetreatPointByNearestEnemy();
        SetDestination(first);
    }

    public IEnumerator Execute()
    {
        if (ai == null || ai.IsDead) yield break;

        _timer = 0f;
        _repathT = 0f;

        while (_timer < retreatDuration)
        {
            if (ai == null || ai.IsDead) yield break;

            _timer += Time.deltaTime;
            _repathT += Time.deltaTime;

            // 이동 중엔 속도 방향 바라보기
            if (ai.aiPath != null) ai.FaceByVelocity(ai.aiPath.velocity);

            // 주기적 목적지 갱신
            if (_repathT >= repathInterval)
            {
                _repathT = 0f;
                Vector3 next = ComputeRetreatPointByNearestEnemy();
                SetDestination(next);
            }

            yield return null;
        }

        // 짧게 텀 후 재공격
        yield return new WaitForSeconds(0.05f);
        if (ai != null && !ai.IsDead)
            ai.StateMachine.ChangeState(new AttackState(ai));
    }

    public void Exit()
    {
        if (ai == null || ai.IsDead) return;
        // 카이팅 종료 시 다시 타겟 추적 복구
        if (ai.destinationSetter != null) ai.destinationSetter.target = ai.target;
    }

    // ───────── 내부 유틸 ─────────

    private void SetDestination(Vector3 pos)
    {
        if (ai.aiPath == null) return;
        if ((pos - ai.transform.position).sqrMagnitude < 0.01f)
            pos += ai.transform.right * 0.5f; // 너무 가까우면 경로 미생성 방지

        ai.aiPath.destination = pos;
        ai.aiPath.SearchPath();
    }

    // “가장 가까운 적” 기준으로 뒤/스트레이프/대각선 후보 중 하나를 선택
    private Vector3 ComputeRetreatPointByNearestEnemy()
    {
        Transform self = ai.transform;
        Transform nearest = FindNearestEnemy(out float nearestDist);

        if (nearest == null) return self.position + self.right * retreatStep;

        Vector2 away = ((Vector2)self.position - (Vector2)nearest.position).normalized;
        if (away.sqrMagnitude < 0.0001f) away = Vector2.right;

        Vector2 left  = new Vector2(-away.y, away.x);
        Vector2 right = -left;

        var candidates = new List<Vector2>(5)
        {
            away, (away + left).normalized, (away + right).normalized, left, right
        };

        float want = Mathf.Max(0.5f, ai.attackRange * comfortableRatio);

        foreach (var d in candidates)
        {
            Vector3 cand = self.position + (Vector3)(d * retreatStep);
            if (IsBlocked(self.position, cand)) continue;

            float predDist = nearest ? Vector2.Distance(cand, nearest.position) : Mathf.Infinity;
            if (predDist < want * 0.9f) continue;

            return cand;
        }

        return self.position + (Vector3)(away * retreatStep);
    }

    private Transform FindNearestEnemy(out float minDist)
    {
        float radius = Mathf.Max(ai.sightRange, ai.attackRange) + 1f;
        int size = Physics2D.OverlapCircleNonAlloc(ai.transform.position, radius, _results, ai.targetLayer);

        Transform nearest = null;
        Transform self = ai.transform;
        minDist = float.PositiveInfinity;

        for (int i = 0; i < size; i++)
        {
            var col = _results[i];
            if (!col) continue;
            if (col.transform == self) continue;

            var otherAI = col.GetComponentInParent<AICore>();
            if (otherAI != null && (otherAI.IsDead || otherAI.State == State.Death)) continue;

            float d = Vector2.Distance(self.position, col.transform.position);
            if (d < minDist)
            {
                minDist = d;
                nearest = col.transform;
            }
        }

        if (float.IsInfinity(minDist)) minDist = -1f;
        return nearest;
    }

    private bool IsBlocked(Vector3 from, Vector3 to)
    {
        if (ai.obstacleLayer.value == 0) return false;
        var dir = (to - from).normalized;
        var len = Vector3.Distance(from, to) + obstacleProbe;
        var hit = Physics2D.Raycast(from, dir, len, ai.obstacleLayer);
        return hit.collider != null;
    }
}
