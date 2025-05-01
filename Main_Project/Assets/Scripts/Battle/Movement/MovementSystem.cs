using System;
using System.Numerics;
using Movement.State;
using UnityEngine;
using Random = UnityEngine.Random;
using Vector2 = UnityEngine.Vector2;

/// <summary>
/// 이동 관련 보조 시스템
/// 아군 회피 및 후퇴 방향 계산을 담당
/// </summary>
public class MovementSystem : MonoBehaviour
{
    public LayerMask teammateLayer;     // 아군이 포함된 레이어 마스크
    public LayerMask wallLayer;
    private BattleAI2 ai;

    
    private void Awake()
    {
        ai = GetComponent<BattleAI2>();
    }

    /// <summary>
    /// 아군을 피해서 이동 방향을 조정하는 함수
    /// </summary>
    /// <param name="moveDirection">기본 이동 방향</param>
    /// <returns>조정된 이동 방향</returns>
    public Vector2 AvoidTeammates(Vector2 moveDirection)
    {
        // 현재 위치 주변의 아군 감지
        Collider2D[] teammates = Physics2D.OverlapCircleAll(transform.position, 1.5f, teammateLayer);

        foreach (Collider2D teammate in teammates)
        {
            // 아군과 반대 방향으로 회피 벡터 생성
            Vector2 avoidDirection = (transform.position - teammate.transform.position).normalized;

            // 이동 방향에 회피 벡터를 일정 비율로 더함
            moveDirection += avoidDirection * 0.5f;
        }

        // 최종 방향을 정규화해서 반환
        return moveDirection.normalized;
    }

    /// <summary>
    /// 타겟에서 멀어지는 후퇴 방향을 계산함 (아군 회피 포함)
    /// </summary>
    /// <param name="playerPosition">현재 유닛의 위치</param>
    /// <param name="targetPosition">타겟의 위치</param>
    /// <returns>후퇴 방향 벡터</returns>
    public Vector2 GetRetreatDirection(Vector2 playerPosition, Vector2 targetPosition)
    {
        // 타겟과 반대 방향으로 후퇴 벡터 계산
        Vector2 retreatDirection = (playerPosition - targetPosition).normalized;

        // 후퇴 방향 또한 벽을 피해서 조정
        return AvoidWalls(retreatDirection);
    }
    public Vector2 AvoidWalls(Vector2 moveDirection)
    {
        
        // 현재 이동 방향을 기준으로 Raycast
        RaycastHit2D hit = Physics2D.Raycast(transform.position, moveDirection.normalized, 3f, wallLayer);

        if (hit.collider != null)
        {
            float distanceToWall = hit.distance;
            Vector2 horizontalwall = hit.point - (Vector2)transform.position;
            float xDistance = horizontalwall.x;

            // 벽이 너무 가까우면 정지 + 반대 방향으로 튕김
            if (distanceToWall < 4f)
            {
                Debug.DrawRay(transform.position, moveDirection.normalized * 3f, Color.red);
                Debug.Log("벽 너무 가까움! 반대 방향으로 이동");
                if (Math.Abs(xDistance) < 4f)
                {
                    ai.Flip();
                }
                return -moveDirection.normalized; // 정반대 방향
            }
            else
            {
                // 벽을 감지했지만 피할 거리 있음: 옆으로 살짝 피하기
                Vector2 perpendicular = Vector2.Perpendicular(moveDirection).normalized;
                Vector2 avoidDir1 = (moveDirection + perpendicular * 0.5f).normalized;
                Vector2 avoidDir2 = (moveDirection - perpendicular * 0.5f).normalized;

                // 두 회피 방향 중 하나 선택 (랜덤 또는 상황 따라)
                Vector2 chosenDir = Random.value > 0.5f ? avoidDir1 : avoidDir2;

                Debug.DrawRay(transform.position, chosenDir * 3f, Color.yellow);
                Debug.Log("벽 피하기 시도: 회피 방향으로 이동");
                return chosenDir;
            }
        }

        // 충돌 없으면 원래 방향 유지
        return moveDirection;
    }

}