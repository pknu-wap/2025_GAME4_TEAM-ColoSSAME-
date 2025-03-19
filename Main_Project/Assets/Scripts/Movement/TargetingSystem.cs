using UnityEngine;
using System.Collections;

public class TargetingSystem : MonoBehaviour
{
    private Transform target;
    private LayerMask enemyLayer;

    public void Initialize(LayerMask enemyLayer)
    {
        this.enemyLayer = enemyLayer;
    }

    public void StartTargeting()
    {
        StartCoroutine(AutoTargeting());
    }

    private IEnumerator AutoTargeting()     //Å¸°ÙÆÃ
    {
        while (true)
        {
            if (target == null)
            {
                FindNearestTarget();
            }
            yield return new WaitForSeconds(1f);
        }
    }

    public Transform GetTarget()
    {
        return target;
    }

    private void FindNearestTarget()        //Å¸°Ù ¼³Á¤
    {
        Collider2D[] enemies = Physics2D.OverlapCircleAll(transform.position, 10f, enemyLayer);
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
            target = nearestEnemy;
            Debug.Log("Å¸°Ù ¼³Á¤µÊ: " + target.name); //Å¸°Ù ¼³Á¤ È®ÀÎ
        }
        else
        {
            Debug.Log("Å¸°Ù ¾øÀ½!");
        }
    }

    public void FaceTarget(Vector2 targetPosition)
    {
        Vector2 direction = (targetPosition - (Vector2)transform.position).normalized;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle));
    }
}
