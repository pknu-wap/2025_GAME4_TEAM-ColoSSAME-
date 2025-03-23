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

    public void FindNearestTarget()        //Å¸°Ù ¼³Á¤
    {
        Collider2D[] enemies = Physics2D.OverlapCircleAll(transform.position, 10f, targetingSystem.enemyLayer);
        float shortestDistance = Mathf.Infinity;
        Transform nearestEnemy = null;

        Debug.Log("Å¸°Ù Ã£´Â Áß..."); //½ÇÇà ¿©ºÎ È®ÀÎ

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
            Debug.Log("Å¸°Ù ¼³Á¤µÊ: " + targetingSystem.target.name); //Å¸°Ù ¼³Á¤ È®ÀÎ
        }
        else
        {
            Debug.Log("Å¸°Ù ¾øÀ½!");
        }
    }
}
