using UnityEngine;

public class Player : MonoBehaviour
{
    public Transform target; // 따라갈 대상
    public float speed = 5f;
    private Rigidbody2D rb; // Rigidbody2D로 변경

    void Start()
    {
        rb = GetComponent<Rigidbody2D>(); // Rigidbody2D 가져오기
        if (rb == null)
        {
            Debug.LogError("Rigidbody2D가 없습니다! Enemy 오브젝트에 Rigidbody2D를 추가하세요.");
        }
    }

    void FixedUpdate()
    {
        int layerMask = LayerMask.GetMask("Enemy"); // "Enemy" 레이어만 감지

        // 타겟 방향으로 레이캐스트
        Vector2 direction = (target.position - transform.position).normalized;
        RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, Mathf.Infinity, layerMask);

        if (hit.collider != null)
        {
            Debug.Log("Enemy 충돌 거리: " + hit.distance);

            if (hit.distance > 3) // Enemy가 멀리 있으면 이동
            {
                rb.velocity = direction * speed;
            }
            else
            {
                rb.velocity = Vector2.zero;
                
            }
        }
        else
        {
            // 레이캐스트가 아무것도 감지하지 못하면 계속 이동
            rb.velocity = direction * speed;
        }
    }
}