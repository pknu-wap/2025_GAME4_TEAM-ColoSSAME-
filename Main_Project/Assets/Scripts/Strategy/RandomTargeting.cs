using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 범위 내 적 중 무작위로 하나를 선택해 타겟으로 지정하는 클래스
/// </summary>
public class RandomTargeting : MonoBehaviour
{
    private TargetingSystem targetingSystem;
    int random;

    private void Start()
    {
        targetingSystem = GetComponent<TargetingSystem>();
    }

    /// <summary>
    /// 탐색 범위 내에서 랜덤한 적을 타겟으로 지정
    /// </summary>
    public void FindRandomTarget()
    {
        // enemyLayer에 해당하는 오브젝트들을 탐색 범위 내에서 찾음
        Collider2D[] enemies = Physics2D.OverlapCircleAll(transform.position, 10f, targetingSystem.enemyLayer);
        Transform randomEnemy = null;

        Debug.Log("타겟 찾는 중..."); // 디버그 로그

        if (enemies.Length == 0)
        {
            Debug.Log("타겟 없음!");
            return;
        }

        // 무작위 인덱스를 선택
        random = Random.Range(0, enemies.Length);
        Debug.Log($"랜덤 인덱스 선택: {random}");

        Collider2D enemy = enemies[random];
        randomEnemy = enemy.transform;

        if (randomEnemy != null)
        {
            // 선택한 적을 타겟으로 지정
            targetingSystem.target = randomEnemy;
            Debug.Log("타겟 설정됨: " + targetingSystem.target.name); // 디버그 로그
        }
        else
        {
            Debug.Log("타겟 없음!");
        }
    }
}