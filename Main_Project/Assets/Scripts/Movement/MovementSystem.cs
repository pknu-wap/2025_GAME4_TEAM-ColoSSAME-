using UnityEngine;

/// <summary>
/// 이동 관련 보조 시스템
/// 아군 회피 및 후퇴 방향 계산을 담당
/// </summary>
public class MovementSystem : MonoBehaviour
{
    public LayerMask teammateLayer;     // 아군이 포함된 레이어 마스크

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

        // 후퇴 방향 또한 아군을 피해서 조정
        return AvoidTeammates(retreatDirection);
    }
}