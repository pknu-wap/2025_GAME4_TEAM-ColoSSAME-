using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class FollowPhysics2D : MonoBehaviour
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
        if (target != null && rb != null) // Rigidbody2D가 있는지 확인
        {
            Vector2 direction = (target.position - transform.position).normalized;
            rb.velocity = direction * speed;
        }
        
        
    }
}