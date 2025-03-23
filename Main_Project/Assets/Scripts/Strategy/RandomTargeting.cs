using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomTargeting : MonoBehaviour
{
    private TargetingSystem targetingSystem;
    int random;
    private void Start()
    {
        targetingSystem = GetComponent<TargetingSystem>();
    }

    public void FindRandomTarget()        //Ÿ�� ����
    {
        Collider2D[] enemies = Physics2D.OverlapCircleAll(transform.position, 10f, targetingSystem.enemyLayer);
        Transform RandomEnemy = null;

        Debug.Log("Ÿ�� ã�� ��..."); //���� ���� Ȯ��

        random = Random.Range(0, enemies.Length);
        Debug.Log($"���� ���� : {random}");
        Collider2D enemy = enemies[random];
        RandomEnemy = enemy.transform;

        if (RandomEnemy != null)
        {
            targetingSystem.target = RandomEnemy;
            Debug.Log("Ÿ�� ������: " + targetingSystem.target.name); //Ÿ�� ���� Ȯ��
        }
        else
        {
            Debug.Log("Ÿ�� ����!");
        }
    }
}
