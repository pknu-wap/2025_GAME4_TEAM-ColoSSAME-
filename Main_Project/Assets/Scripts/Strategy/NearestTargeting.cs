using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 가장 가까운 적을 찾아 타겟으로 설정하는 클래스
/// </summary>
public class NearestTargeting : MonoBehaviour
{
    private TargetingSystem targetingSystem;

    private void Start()
    {
        targetingSystem = GetComponent<TargetingSystem>();
    }

    /// <summary>
    /// 범위 내에서 가장 가까운 적을 찾아 타겟으로 설정
    /// </summary>
    public void FindNearestTarget()
    {
        // enemyLayer에 해당하는 오브젝트들을 범위 내에서 탐색
        Collider2D[] enemies = Physics2D.OverlapCircleAll(transform.position, 10f, targetingSystem.enemyLayer);

        float shortestDistance = Mathf.Infinity;
        Transform nearestEnemy = null;

        Debug.Log("타겟 찾는 중..."); // 디버그 로그

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
            // 가장 가까운 적을 타겟으로 설정
            targetingSystem.target = nearestEnemy;
            Debug.Log("타겟 설정됨: " + targetingSystem.target.name); // 디버그 로그
        }
        else
        {
            Debug.Log("타겟 없음!"); // 범위 내 적이 없는 경우
        }
    }
}