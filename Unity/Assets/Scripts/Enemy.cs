using UnityEngine;
using System.Collections;

public class Enemy : MonoBehaviour
{
    public Transform target; // 따라갈 대상
    public float speed = 5f;
    public float attackRange = 3f;
    private Rigidbody2D rb;
    private bool isMoving = true; // 이동 상태 체크

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            Debug.LogError("Rigidbody2D가 없습니다! Player 오브젝트에 Rigidbody2D를 추가하세요.");
            return;
        }

        StartCoroutine(FollowTarget()); // 코루틴 시작
    }

    IEnumerator FollowTarget()
    {
        while (true) // 무한 루프 (게임이 끝날 때까지 반복)
        {
            if (target != null)
            {
                int layerMask = LayerMask.GetMask("Player"); // "Player" 레이어만 감지
                Vector2 direction = (target.position - transform.position).normalized;
                RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, Mathf.Infinity, layerMask);

                if (hit.collider != null) // Player 감지됨
                {
                    Debug.Log("Enemy 충돌 거리: " + hit.distance);

                    if (hit.distance > attackRange) // Player가 멀리 있으면 이동
                    {
                        rb.velocity = direction * speed;
                        isMoving = true;
                    }
                    else // 가까이 있으면 멈춤
                    {
                        rb.velocity = Vector2.zero;
                        isMoving = false;
                    }
                }
                else // Player를 감지하지 못하면 계속 이동
                {
                    rb.velocity = direction * speed;
                    isMoving = true;
                }
            }

            yield return new WaitForFixedUpdate(); // FixedUpdate()와 동기화하여 실행
        }
    }
}