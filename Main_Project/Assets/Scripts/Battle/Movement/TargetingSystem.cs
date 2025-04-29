using System;
using UnityEngine;
using System.Collections;
using Character.Movement;

/// <summary>
/// 타겟팅 시스템: 전략 유형에 따라 적을 자동으로 탐색하고 타겟 설정
/// </summary>
public class TargetingSystem : MonoBehaviour
{
    public Transform target;                  // 현재 타겟
    public LayerMask enemyLayer;              // 적이 포함된 레이어
    public NearestTargeting nearest;          // 가장 가까운 적을 찾는 방식
    public RandomTargeting randomenemy;       // 무작위로 적을 찾는 방식
    public Player player;                     // 플레이어 정보 (전략 타입 포함)

    /// <summary>
    /// 외부에서 enemy 레이어를 초기화할 때 사용
    /// </summary>
    public void Initialize(LayerMask enemyLayer)
    {
        this.enemyLayer = enemyLayer;
    }

    private void Start()
    {
        player = GetComponent<Player>();
    }

    /// <summary>
    /// 플레이어의 전략 유형에 따라 타겟팅 방식 선택
    /// </summary>
    public void StartTargeting()
    {
        if (player != null)
        {
            if (player.strategyType == 1)
            {
                // 가장 가까운 적 타겟팅
                StartCoroutine(NearestTargeting());
            }
            else if (player.strategyType == 2)
            {
                // 무작위 타겟팅
                StartCoroutine(RandomTargeting());
            }
        }
    }

    /// <summary>
    /// 가장 가까운 적을 주기적으로 탐색하는 코루틴
    /// </summary>
    private IEnumerator NearestTargeting()
    {
        while (true)
        {
            if (target == null)
            {
                nearest = GetComponent<NearestTargeting>();
                nearest.FindNearestTarget();
            }
            yield return new WaitForSeconds(1f);   // 1초마다 탐색
        }
    }

    /// <summary>
    /// 무작위로 적을 주기적으로 탐색하는 코루틴
    /// </summary>
    private IEnumerator RandomTargeting()
    {
        while (true)
        {
            if (target == null)
            {
                randomenemy = GetComponent<RandomTargeting>();
                randomenemy.FindRandomTarget();
            }
            yield return new WaitForSeconds(1f);   // 1초마다 탐색
        }
    }

    /// <summary>
    /// 현재 타겟 반환
    /// </summary>
    public Transform GetTarget()
    {
        return target;
    }

    /// <summary>
    /// 타겟을 바라보도록 회전
    /// </summary>
    /// <param name="targetPosition">타겟의 위치</param>
    public void FaceTarget(Vector2 targetPosition)
    {
        Vector2 direction = (targetPosition - (Vector2)transform.position).normalized;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle));
    }

    /// <summary>
    /// 에디터에서 타겟 탐색 범위와 타겟 라인을 그려주는 디버그용 Gizmo
    /// </summary>
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, 10f); // 타겟 탐색 범위 시각화

        if (target != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, target.position); // 타겟까지 선 그리기
        }
    }
}
