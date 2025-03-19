using UnityEngine;

public class MovementSystem : MonoBehaviour
{
    public LayerMask teammateLayer;

    public Vector2 AvoidTeammates(Vector2 moveDirection)        //ÆÀ¿ø È¸ÇÇ
    {
        Collider2D[] teammates = Physics2D.OverlapCircleAll(transform.position, 1.5f, teammateLayer);
        foreach (Collider2D teammate in teammates)
        {
            Vector2 avoidDirection = (transform.position - teammate.transform.position).normalized;
            moveDirection += avoidDirection * 0.5f;
        }
        return moveDirection.normalized;
    }

    public Vector2 GetRetreatDirection(Vector2 playerPosition, Vector2 targetPosition)      //ÈÄÅð¹æÇâ
    {
        Vector2 retreatDirection = (playerPosition - targetPosition).normalized;
        return AvoidTeammates(retreatDirection);
    }
}
