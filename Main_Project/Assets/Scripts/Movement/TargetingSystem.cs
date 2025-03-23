using UnityEngine;
using System.Collections;

public class TargetingSystem : MonoBehaviour
{
    public Transform target;
    public LayerMask enemyLayer;
    public NearestTargeting nearest;
    public RandomTargeting randomenemy;
    public Player player;

    public void Initialize(LayerMask enemyLayer)
    {
        this.enemyLayer = enemyLayer;
    }

    public void StartTargeting()
    {
        player = GetComponent<Player>();
        if (player != null)
        {
            if (player.strategyType == 1)
            {
                StartCoroutine(NearestTargeting());
            }
            else if(player.strategyType == 2)
            {
                StartCoroutine(RandomTargeting());
            }
        }
    }

    private IEnumerator NearestTargeting()     //Å¸°ÙÆÃ
    {
        while (true)
        {
            if (target == null)
            {
                nearest = GetComponent<NearestTargeting>();
                nearest.FindNearestTarget();
            }
            yield return new WaitForSeconds(1f);
        }
    }

    private IEnumerator RandomTargeting()     //Å¸°ÙÆÃ
    {
        while (true)
        {
            if (target == null)
            {
                randomenemy = GetComponent<RandomTargeting>();
                randomenemy.FindRandomTarget();
            }
            yield return new WaitForSeconds(1f);
        }
    }

    public Transform GetTarget()
    {
        return target;
    }

    public void FaceTarget(Vector2 targetPosition)
    {
        Vector2 direction = (targetPosition - (Vector2)transform.position).normalized;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle));
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, 10f); // Å¸°Ù Å½»ö ¹üÀ§
        if (target != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, target.position); // Å¸°Ù°úÀÇ ¼±
        }
    }
}
