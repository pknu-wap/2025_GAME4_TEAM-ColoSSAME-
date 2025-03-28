using UnityEngine;
using System.Collections;

/// <summary>
/// 적 자동 전투 AI 클래스
/// Rigidbody2D, 타겟팅 시스템, 이동 시스템을 통해 행동을 결정함.
/// </summary>
public class BattleAI : MonoBehaviour
{
    private Rigidbody2D rb;                        // 물리 이동을 위한 Rigidbody2D
    private TargetingSystem targeting;            // 타겟팅 시스템
    private MovementSystem movement;              // 이동 관련 로직 처리 시스템
    private float attackRange;                    // 공격 사거리
    private float attackDelay;                    // 공격 후 대기 시간
    private float retreatDistance;                // 후퇴 거리
    private float speed;                          // 이동 속도
    private bool isAttacking = false;             // 공격 중인지 여부
    private Player player;
    
    public bool isFacingRight;

    /// <summary>
    /// 외부에서 초기값을 설정하는 함수
    /// </summary>
    public void Initialize(Rigidbody2D rb, TargetingSystem targeting, MovementSystem movement, float attackRange, float attackDelay, float retreatDistance, float speed)
    {
        this.rb = rb;
        this.targeting = targeting;
        this.movement = movement;
        this.attackRange = attackRange;
        this.attackDelay = attackDelay;
        this.retreatDistance = retreatDistance;
        this.speed = speed;

    }

    /// <summary>
    /// 전투 시작 시 호출. 자동 전투 AI 루프 시작
    /// </summary>
    public void StartBattle()
    {
        StartCoroutine(AutoBattleAI());
    }

    /// <summary>
    /// 자동 전투 루프 코루틴
    /// </summary>
    private IEnumerator AutoBattleAI()
    {
        while (true)
        {
            Transform target = targeting.GetTarget();     // 타겟 가져오기

            if (target == null)
            {
                targeting.StartTargeting();               // 타겟이 없으면 타겟팅 시작
            }

            if (target != null)
            {
                float distance = Vector2.Distance(transform.position, target.position);           // 타겟과의 거리 계산
                Vector2 direction = (target.position - transform.position).normalized;           // 타겟을 향한 방향 벡터 계산
                direction = movement.AvoidTeammates(direction);                                  // 아군 피하기
                FaceTargetHorizontally(target.position);                                         // 타겟 방향 바라보기

                if (distance > attackRange)
                {
                    // 사거리 밖이면 이동
                    rb.velocity = direction * speed;
                }
                else if (!isAttacking)
                {
                    // 사거리 내에 있고 공격 중이 아니라면 공격 시도
                    StartCoroutine(AttackSequence(target));
                }
            }
            else
            {
                // 타겟이 없으면 정지
                rb.velocity = Vector2.zero;
            }

            yield return null;
        }
    }
    
    // 방향 전환 함수
    private void Flip()
    {
        isFacingRight = !isFacingRight;

        Vector3 scale = transform.localScale;
        scale.x *= -1; // x만 반전
        transform.localScale = scale;
    }

    // 타겟 방향에 따라 스프라이트 좌우반전
    private void FaceTargetHorizontally(Vector3 targetPos)
    {
        if (targetPos.x < transform.position.x && isFacingRight)
        {
            // 왼쪽으로 방향 전환
            Flip();
        }
        else if (targetPos.x > transform.position.x && !isFacingRight)
        {
            // 오른쪽으로 방향 전환
            Flip();
        }
    }


    /// <summary>
    /// 공격 및 후퇴 시퀀스 처리 코루틴
    /// </summary>
    private IEnumerator AttackSequence(Transform target)
    {
        isAttacking = true;
        rb.velocity = Vector2.zero;
        FaceTargetHorizontally(target.position);             // 공격 시 타겟 바라보기

        Debug.Log("공격!");

        yield return new WaitForSeconds(attackDelay);      // 공격 후 대기

        Vector2 retreatDirection = movement.GetRetreatDirection(transform.position, target.position);   // 후퇴 방향 계산
        float retreatTime = retreatDistance / speed;       // 후퇴 시간 계산

        float elapsedTime = 0f;
        while (elapsedTime < retreatTime)
        {
            rb.velocity = retreatDirection * speed;        // 후퇴
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        rb.velocity = Vector2.zero;                        // 후퇴 후 정지
        isAttacking = false;                               // 공격 종료
    }
}
