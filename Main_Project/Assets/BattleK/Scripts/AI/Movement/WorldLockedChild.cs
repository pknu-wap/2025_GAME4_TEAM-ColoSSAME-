using System.Collections;
using System.Collections.Generic;
using BattleK.Scripts.AI;
using UnityEngine;

public class WorldLockedChild : MonoBehaviour
{
    public Vector3 fixedWorldPosition;
    public AICore ai;

    void Start()
    {
        // 시작 시 현재 월드 위치를 저장
        fixedWorldPosition = transform.position;
    }
    
    public void FixRetreatPositionInBackwardArc()
    {
        if (ai.target == null) return;

        Vector3 selfPos = transform.position;
        Vector3 targetPos = ai.target.position;

        Vector3 toTarget = (targetPos - selfPos).normalized;
        float radius = ai.attackRange;

        int maxAttempts = 10;

        for (int i = 0; i < maxAttempts; i++)
        {
            // 뒤쪽 180도 각도 내 랜덤 방향
            float angle = Random.Range(90f, 270f);
            Vector3 dir = Quaternion.Euler(0, 0, angle) * toTarget;

            Vector3 candidatePos = targetPos + dir.normalized * radius;

            // 2D에서 벽 검사 (원형 충돌)
            Collider2D hit = Physics2D.OverlapCircle(candidatePos, 0.5f, ai.obstacleLayer);

            if (hit == null)
            {
                fixedWorldPosition = candidatePos;
                Debug.Log($"[후퇴] {ai.name} will retreat to {fixedWorldPosition}");
                return;
            }
        }

        // 후퇴 실패 → fallback
        fixedWorldPosition = selfPos;
        Debug.LogWarning($"{ai.name} failed to find a safe retreat position. Staying.");
    }


    void LateUpdate()
    {
        // 부모 움직임과 상관없이 월드 위치 고정
        transform.position = fixedWorldPosition;
    }
}
