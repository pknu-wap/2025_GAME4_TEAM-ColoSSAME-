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

    private IEnumerator AutoTargeting()     //Ÿ����
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

    private void FindNearestTarget()        //Ÿ�� ����
    {
        Collider2D[] enemies = Physics2D.OverlapCircleAll(transform.position, 10f, enemyLayer);
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
            target = nearestEnemy;
            Debug.Log("Ÿ�� ������: " + target.name); //Ÿ�� ���� Ȯ��
        }
        else
        {
            Debug.Log("Ÿ�� ����!");
        }
    }

    public void FaceTarget(Vector2 targetPosition)
    {
        Vector2 direction = (targetPosition - (Vector2)transform.position).normalized;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle));
    }
}
