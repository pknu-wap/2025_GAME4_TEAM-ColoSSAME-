using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NearestTargeting : MonoBehaviour
{
    private TargetingSystem targetingSystem;
    private void Start()
    {
        targetingSystem = GetComponent<TargetingSystem>();
    }

    public void FindNearestTarget()        //Ÿ�� ����
    {
        Collider2D[] enemies = Physics2D.OverlapCircleAll(transform.position, 10f, targetingSystem.enemyLayer);
        float shortestDistance = Mathf.Infinity;
        Transform nearestEnemy = null;

        Debug.Log("Ÿ�� ã�� ��..."); //���� ���� Ȯ��

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
            targetingSystem.target = nearestEnemy;
            Debug.Log("Ÿ�� ������: " + targetingSystem.target.name); //Ÿ�� ���� Ȯ��
        }
        else
        {
            Debug.Log("Ÿ�� ����!");
        }
    }
}
