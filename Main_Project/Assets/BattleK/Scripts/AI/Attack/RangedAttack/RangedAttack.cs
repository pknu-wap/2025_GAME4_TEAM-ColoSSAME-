using System.Collections;
using UnityEngine;
using UnityEngine.Pool;

[RequireComponent(typeof(Collider2D))]
public class RangedAttack : MonoBehaviour
{
    [Header("Owner / Team")]
    public LayerMask teammateLayer;     // 아군 레이어(피격 무시용)
    public LayerMask hitLayers;         // 타격 가능한 레이어

    [Header("Projectile")]
    public Projectile projectilePrefab; // 투사체 프리팹
    public Transform firePoint;         // 발사 지점

    [Header("Stats")]
    public float prepareTime = 0.15f;   // 발사 전 윈드업
    public float cooldown = 0.6f;       // 발사 쿨다운
    public Vector3 projectileScale = Vector3.one;
    public float projectileSpeed = 10f;
    public float maxRange = 8f;
    public int baseDamage = 10;

    [Header("Behavior")]
    public bool snapshotAimOnRelease = true; // 발사 시점 방향 스냅샷
    public bool blockOutOfRange = true;      // 사거리 밖이면 발사 안 함

    float _lastShotTime = -999f;
    Collider2D _ownerCollider;
    IObjectPool<Projectile> _pool;

    void Awake()
    {
        _ownerCollider = GetComponent<Collider2D>();
        if (!firePoint) firePoint = transform;

        _pool = new ObjectPool<Projectile>(
            createFunc: () =>
            {
                var p = Instantiate(projectilePrefab);
                p.gameObject.SetActive(false);
                p.OnDespawnRequested = ReturnToPool;
                return p;
            },
            actionOnGet: p => p.gameObject.SetActive(true),
            actionOnRelease: p => p.gameObject.SetActive(false),
            actionOnDestroy: p => Destroy(p.gameObject),
            collectionCheck: false, defaultCapacity: 16, maxSize: 256
        );
    }

    public bool TryAttack(Transform target)
    {
        if (!CanFireNow()) return false;
        if (target == null) return false;

        float dist = Vector2.Distance(transform.position, target.position);
        if (blockOutOfRange && dist > maxRange) return false;

        StartCoroutine(FireRoutine(target));
        _lastShotTime = Time.time;
        return true;
    }

    bool CanFireNow() => Time.time >= _lastShotTime + cooldown;

    IEnumerator FireRoutine(Transform target)
    {
        if (prepareTime > 0f)
            yield return new WaitForSeconds(prepareTime);

        Vector2 dir = ((Vector2)target.position - (Vector2)firePoint.position).normalized;

        // 스냅샷 조준이 아니면, 발사 프레임에서 다시 계산할 수도 있지만
        // 현재 프레임에서 계산한 값으로 사용 (필요 시 보간 구현 가능)
        SpawnAndFire(dir);
    }

    void SpawnAndFire(Vector2 dir)
    {
        var proj = _pool.Get();
        proj.transform.position = firePoint.position;
        proj.transform.rotation = Quaternion.FromToRotation(Vector3.right, dir);
        proj.transform.localScale = projectileScale;

        proj.IgnoreCollider(_ownerCollider);
        proj.SetAllyMask(teammateLayer);

        proj.Launch(new Projectile.Params
        {
            speed = projectileSpeed,
            damage = baseDamage,
            maxTravelDistance = maxRange,
            hitLayers = hitLayers,
            attacker = gameObject,
        });
    }

    void ReturnToPool(Projectile p) => _pool.Release(p);

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1, 0.8f, 0f, 0.25f);
        Gizmos.DrawWireSphere(firePoint ? firePoint.position : transform.position, 0.1f);
        Gizmos.color = new Color(0, 1, 0.2f, 0.25f);
        Gizmos.DrawWireSphere(transform.position, maxRange);
    }
}
