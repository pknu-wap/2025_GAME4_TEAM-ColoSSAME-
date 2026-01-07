using System.Collections;
using System.Collections.Generic;
using BattleK.Scripts.AI;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class MeleeAttack : MonoBehaviour
{
    public BoxCollider2D boxCollider;

    [Header("히트박스 두께(높이)")]
    public float thickness = 0.4f;

    [Header("히트박스를 몇 FixedUpdate 동안 유지할지(권장: 2~3)")]
    public int activeFixedFrames = 2;

    [Header("광역 여부 (true면 범위 내 적 전부 타격)")]
    public bool isAoE = false;

    private AICore ownerAi;
    private readonly HashSet<AICore> alreadyHit = new HashSet<AICore>();

    // 👉 추가: 현재 히트박스 유지 코루틴
    private Coroutine _activeRoutine;

    void Awake()
    {
        boxCollider = GetComponent<BoxCollider2D>();
        boxCollider.isTrigger = true;

        // 콜라이더는 로컬 기준으로 다루는 게 안정적
        boxCollider.offset = Vector2.zero;
        boxCollider.size = new Vector2(0.1f, thickness);

        // 기본은 비활성
        boxCollider.enabled = false;
    }

    void OnDisable()
    {
        // 비활성화되면 콜라이더/코루틴 정리
        CancelAll();
    }

    public void Initialize(AICore ai)
    {
        ownerAi = ai;
    }

    /// <summary>
    /// 현재 타겟과의 방향과 거리 기준으로 선형 히트박스를 생성
    /// </summary>
    public void Attack()
    {
        if (ownerAi == null || ownerAi.target == null) return;
        alreadyHit.Clear();

        Vector3 selfPos = ownerAi.transform.position;
        Vector3 targetPos = ownerAi.target.position;

        // 방향과 실제 거리
        Vector2 dir = (targetPos - selfPos).normalized;
        float distToTarget = Vector2.Distance(selfPos, targetPos);

        // 실제 커버할 길이 = min(타겟까지 거리, 공격 사거리)
        float attackRange = Mathf.Max(0f, ownerAi.attackRange);
        float coverLength = Mathf.Min(distToTarget, attackRange);

        // 최소 길이 보정(너무 짧으면 충돌 안 잡힐 수 있음)
        coverLength = Mathf.Max(coverLength, 0.05f);

        // 콜라이더 길이/두께 설정 (가로=길이, 세로=두께)
        boxCollider.size = new Vector2(coverLength, thickness);

        // 자신의 위치에서 방향으로 절반 만큼 전진한 지점이 콜라이더 중앙
        Vector3 centerWorld = selfPos + (Vector3)(dir * (coverLength * 0.5f));

        // 회전: BoxCollider2D는 Transform 회전을 따르므로, Z축 회전(2D)으로 방향 맞춤
        float angleDeg = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

        // 트랜스폼을 월드 기준으로 배치/회전
        transform.SetPositionAndRotation(centerWorld, Quaternion.Euler(0f, 0f, angleDeg));

        // 충돌 감지 시작
        boxCollider.enabled = true;
        if (_activeRoutine != null) StopCoroutine(_activeRoutine);
        _activeRoutine = StartCoroutine(DisableColliderAfterFixedFrames(activeFixedFrames));
    }

    private IEnumerator DisableColliderAfterFixedFrames(int fixedFrames)
    {
        // 물리 충돌은 FixedUpdate 주기에 처리되므로 고정 프레임 기준으로 유지
        for (int i = 0; i < Mathf.Max(1, fixedFrames); i++)
            yield return new WaitForFixedUpdate();

        boxCollider.enabled = false;
        _activeRoutine = null;
    }

    private static bool Probability(float probabilityPercent)
    {
        return Random.Range(0f, 100f) <= probabilityPercent;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // 자신은 무시
        if (ownerAi == null) return;

        AICore targetAi = other.GetComponent<AICore>();
        if (targetAi == null) return;

        // 같은 팀 무시
        if (targetAi.gameObject.layer == ownerAi.gameObject.layer) return;

        // 죽은 대상 무시
        if (targetAi.State == State.Death || targetAi.IsDead) return;

        // AoE가 아니면 "현재 타겟만" 허용
        if (!isAoE && targetAi.transform != ownerAi.target) return;

        if (alreadyHit.Contains(targetAi)) return;
        alreadyHit.Add(targetAi);

        // 회피 판정
        if (Probability(targetAi.evasionRate))
        {
            targetAi.StateMachine.ChangeState(new EvadeState(targetAi));
            return;
        }

        targetAi.StateMachine.ChangeState(new DamageState(targetAi, ownerAi.attackDamage));
    }

    /// <summary>
    /// ▶ 모든 근접 공격 루틴/히트박스 즉시 중단 (AICore.StopAllActionsHard에서 호출)
    /// </summary>
    public void CancelAll()
    {
        if (_activeRoutine != null)
        {
            try { StopCoroutine(_activeRoutine); } catch { }
            _activeRoutine = null;
        }

        if (boxCollider != null)
            boxCollider.enabled = false;

        alreadyHit.Clear();
        CancelInvoke();
        // 필요시 이펙트/사운드 정리 추가
    }
}
