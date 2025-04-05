using Movement.State;
using UnityEngine;

public class WeaponPositioner : MonoBehaviour
{
    public Transform weaponColliderTransform;  // 무기 콜라이더
    public float offsetDistance = 1.0f;        // 적 방향으로 얼마나 떨어질지

    private BattleAI2 ai;

    private void Awake()
    {
        ai = GetComponent<BattleAI2>();
    }

    private void Update()
    {
        Transform target = ai.GetTarget();
        if (target == null) return;

        Vector2 directionToTarget = (target.position - transform.position).normalized;

        // 가장 가까운 4방향 중 하나로 스냅 (상하좌우)
        Vector2 snappedDir = SnapDirectionToCardinal(directionToTarget);

        weaponColliderTransform.localPosition = snappedDir * offsetDistance;
    }

    private Vector2 SnapDirectionToCardinal(Vector2 dir)
    {
        if (Mathf.Abs(dir.x) > Mathf.Abs(dir.y))
        {
            return dir.x > 0 ? Vector2.right : Vector2.left;
        }
        else
        {
            return dir.y > 0 ? Vector2.up : Vector2.down;
        }
    }
}
