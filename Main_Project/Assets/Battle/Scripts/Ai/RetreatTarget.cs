using UnityEngine;

namespace Battle.Scripts.Ai
{
    public class RetreatTarget : MonoBehaviour
    {
        public BattleAI ai;
        public Vector2 retreatPos;

        public void SetRetreatTarget()
        {
            const int maxAttempts = 10; // 최대 재시도 횟수
            Vector2 origin = ai.transform.position;

            for (int i = 0; i < maxAttempts; i++)
            {
                // 랜덤한 단위 방향 벡터 생성 (normalized)
                Vector2 randomDir = Random.insideUnitCircle.normalized;

                // 이동 거리 반영
                retreatPos = origin + randomDir * ai.retreatDistance;

                // 영역 제한
                retreatPos = new Vector2(
                    Mathf.Clamp(retreatPos.x, ai.retreatAreaMin.x, ai.retreatAreaMax.x),
                    Mathf.Clamp(retreatPos.y, ai.retreatAreaMin.y, ai.retreatAreaMax.y)
                );

                // 벽 판정
                if (!IsWall(origin, retreatPos))
                {
                    ai.Retreater.position = retreatPos;
                    ai.destinationSetter.target = ai.Retreater;
                    ai.aiPath.canMove = true;
                    return;
                }
            }

            // 실패 시 현재 위치 유지 (혹은 Idle 전환 등 대체 행동)
            ai.Retreater.position = origin;
            ai.destinationSetter.target = ai.Retreater;
            ai.aiPath.canMove = false;
            Debug.LogWarning($"{ai.gameObject.name}의 후퇴 실패: 유효한 위치를 찾지 못함");
        }

        private bool IsWall(Vector2 origin, Vector2 target)
        {
            Vector2 direction = target - origin;
            float distance = direction.magnitude;

            RaycastHit2D hit = Physics2D.Raycast(origin, direction.normalized, distance, ai.obstacleMask);
            return hit.collider != null;
        }
    }
}