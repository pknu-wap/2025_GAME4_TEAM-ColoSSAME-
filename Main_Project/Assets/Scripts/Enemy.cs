using UnityEngine;
using System.Collections;

public class Enemy : MonoBehaviour
{
    public float speed = 5f; // 이동 속도
    public float attackRange = 2f; // 공격 범위
    public float retreatDistance = 3f; // 후퇴 거리
    public float attackDelay = 1f; // 공격 후 대기 시간
    public LayerMask enemyLayer; // 적 레이어
    public LayerMask teammateLayer; // 팀원 레이어
    public Transform initialTarget; // 초기 타겟 설정

    private Rigidbody2D rb;
    private Transform target; // 현재 타겟
    private bool isAttacking = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        target = initialTarget; // 초기 타겟 설정
        StartCoroutine(AutoTargeting()); // 자동 타겟팅 시작
        StartCoroutine(AutoBattleAI()); // 자동 전투 AI 시작
    }

    /// <summary>
    /// 자동 전투 AI (접근 → 공격 → 후퇴 반복, 팀원 회피 추가)
    /// </summary>
    IEnumerator AutoBattleAI()
    {
        while (true)
        {
            if (target == null) // 타겟이 없으면 찾기
            {
                FindNearestTarget();
            }

            if (target != null)
            {
                float distance = Vector2.Distance(transform.position, target.position);
                Vector2 direction = (target.position - transform.position).normalized;
                direction = AvoidTeammates(direction); // 팀원을 피하는 방향 적용
                FaceTarget(target.position); // 적을 바라보도록 설정

                if (distance > attackRange) // 적과의 거리가 멀면 접근
                {
                    rb.velocity = direction * speed;
                }
                else if (!isAttacking) // 공격 범위 안에 들어오면 공격
                {
                    StartCoroutine(AttackSequence());
                }
            }
            else
            {
                rb.velocity = Vector2.zero; // 타겟이 없으면 멈춤
            }

            yield return null; // 다음 프레임까지 대기
        }
    }

    /// <summary>
    /// 공격 후 후퇴하는 동작을 실행
    /// </summary>
    IEnumerator AttackSequence()
    {
        isAttacking = true;
        rb.velocity = Vector2.zero; // 공격 시 멈춤
        FaceTarget(target.position); // 공격 시에도 적을 바라봄

        Debug.Log("공격!");
        yield return new WaitForSeconds(attackDelay); // 공격 대기 시간

        Vector2 retreatDirection = (transform.position - target.position).normalized;
        retreatDirection = AvoidTeammates(retreatDirection); // 후퇴 시에도 팀원을 피함
        float retreatTime = retreatDistance / speed; // 후퇴에 걸리는 시간 계산

        float elapsedTime = 0f;
        while (elapsedTime < retreatTime) // 후퇴 동작 수행
        {
            rb.velocity = retreatDirection * speed;
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        rb.velocity = Vector2.zero; // 후퇴 후 정지
        isAttacking = false; // 다시 접근 가능하도록 설정
    }

    /// <summary>
    /// 자동 타겟 설정 (주변에서 가장 가까운 적 찾기)
    /// </summary>
    IEnumerator AutoTargeting()
    {
        while (true)
        {
            if (target == null) // 타겟이 없을 때만 갱신
            {
                FindNearestTarget();
            }
            yield return new WaitForSeconds(1f); // 1초마다 타겟 갱신
        }
    }

    /// <summary>
    /// 주변에서 가장 가까운 적을 찾음
    /// </summary>
    private void FindNearestTarget()
    {
        Collider2D[] enemies = Physics2D.OverlapCircleAll(transform.position, 10f, enemyLayer);
        float shortestDistance = Mathf.Infinity;
        Transform nearestEnemy = null;

        foreach (Collider2D enemy in enemies)
        {
            float distance = Vector2.Distance(transform.position, enemy.transform.position);
            if (distance < shortestDistance)
            {
                shortestDistance = distance;
                nearestEnemy = enemy.transform;
            }
        }

        if (nearestEnemy != null)
        {
            target = nearestEnemy; // 가장 가까운 적을 타겟으로 설정
        }
    }

    /// <summary>
    /// 적을 바라보도록 방향을 조정하는 함수
    /// </summary>
    private void FaceTarget(Vector2 targetPosition)
    {
        Vector2 direction = (targetPosition - (Vector2)transform.position).normalized;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle)); // 정면을 바라보도록 회전
    }

    /// <summary>
    /// 팀원과의 충돌을 피하도록 방향을 조정
    /// </summary>
    private Vector2 AvoidTeammates(Vector2 moveDirection)
    {
        Collider2D[] teammates = Physics2D.OverlapCircleAll(transform.position, 1.5f, teammateLayer);
        foreach (Collider2D teammate in teammates)
        {
            Vector2 avoidDirection = (transform.position - teammate.transform.position).normalized;
            moveDirection += avoidDirection * 0.5f; // 팀원을 피하는 방향 보정
        }
        return moveDirection.normalized;
    }

    /// <summary>
    /// 현재 타겟을 시각적으로 표시 (디버그 용도)
    /// </summary>
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, 10f); // 타겟 탐색 범위
        if (target != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, target.position); // 타겟과의 선
        }
    }
}
