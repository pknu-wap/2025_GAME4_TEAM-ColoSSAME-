using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

[DisallowMultipleComponent]
[RequireComponent(typeof(Collider2D))]
public class RangedAttack : MonoBehaviour
{
    [Header("Owner / Team (자동 설정 대상)")]
    public LayerMask teammateLayer;     // 아군(자기 레이어)
    public LayerMask hitLayers;         // 적(상대 레이어)

    [Header("우선 매핑: 레이어 '인덱스'(숫자)")]
    [Tooltip("예: Player=10, Enemy=11 처럼 인덱스로 확정 매핑")]
    public int playerLayerIndex = 10;
    public int enemyLayerIndex  = 11;
    [Tooltip("인덱스가 유효(0~31)하면 이름 매핑보다 인덱스 매핑을 우선 사용")]
    public bool preferIndexMapping = true;

    [Header("보조 매핑: 레이어 '이름'(선택)")]
    public string selfLayerNameForPlayer = "Player";
    public string selfLayerNameForEnemy  = "Enemy";
    public string opponentLayerNameForPlayer = "Enemy";
    public string opponentLayerNameForEnemy  = "Player";

    [System.Serializable]
    public struct OpponentMap { public string selfLayerName; public string opponentLayerName; }
    [Tooltip("프로젝트에서 Player/Enemy 외 이름을 쓰면 여기에 직접 매핑")]
    public OpponentMap[] customOpponentMaps = new OpponentMap[0];

    [Header("Projectile")]
    public Projectile projectilePrefab; // ⚠️ 런타임 로드 시 끊기기 쉬움 → SetProjectilePrefab()으로 주입 가능
    public Transform firePoint;

    [Header("Projectile Layer Override")]
    [Tooltip("발사 시 투사체 레이어를 강제로 지정. -1이면 프리팹 원래 레이어 유지")]
    public int projectileLayerOverride = -1;
    [Tooltip("오버라이드 시 자식까지 모두 적용")]
    public bool overrideChildrenLayer = true;

    [Header("Stats")]
    public float prepareTime = 0.15f;
    public float cooldown = 0.6f;
    public Vector3 projectileScale = Vector3.one;
    public float projectileSpeed = 10f;
    public float maxRange = 8f;
    public int baseDamage = 10;

    [Header("Behavior")]
    public bool snapshotAimOnRelease = true;
    public bool blockOutOfRange = true;

    [Header("Auto Rebind")]
    [Tooltip("스폰 직후 한/두 프레임 대기 후 자동 재바인딩")]
    public bool autoRebindAfterSpawn = true;

    float _lastShotTime = -999f;
    Collider2D _ownerCollider;
    IObjectPool<Projectile> _pool;
    bool _poolReady;

    // 👉 추가: 현재 발사 코루틴 참조
    Coroutine _fireRoutine;

    void Reset()
    {
        if (!firePoint) firePoint = transform;
    }

    void Awake()
    {
        _ownerCollider = GetComponent<Collider2D>();
        if (!firePoint) firePoint = transform;

        // 풀은 projectilePrefab이 있을 때만 생성
        TryBuildPool();
    }

    void OnEnable()
    {
        if (autoRebindAfterSpawn)
            StartCoroutine(Co_RebindNextFrame());
    }

    void OnDisable()
    {
        // 👉 비활성화/죽음 시 모든 동작 정지
        CancelAll();
    }

    IEnumerator Co_RebindNextFrame()
    {
        // 스폰 후 상위/스포너의 레이어 세팅이 끝나도록 한 프레임 대기
        yield return null;
        RebindNow(force: false);

        // 주소형 로드/지연 초기화 대비로 한 프레임 더
        if (teammateLayer.value == 0 || hitLayers.value == 0 || (hitLayers.value & teammateLayer.value) != 0)
        {
            yield return null;
            RebindNow(force: false);
        }
    }

    /// 외부(스포너/유틸)에서 언제든 호출 가능. force=true면 기존 값 덮어씀.
    public void RebindNow(bool force)
    {
        AutoConfigureMasksImpl(force);
    }

    void AutoConfigureMasksImpl(bool force)
    {
        int selfLayer = gameObject.layer;

        // 1) 아군 = 자기 레이어
        if (force || teammateLayer.value == 0)
            teammateLayer = 1 << selfLayer;

        // 2) 적 = 자기 레이어 기준 상대 레이어 결정
        if (force || hitLayers.value == 0)
        {
            hitLayers = ResolveOpponentMask(selfLayer);

            // 못 찾았거나(0) 아군과 겹치면 보정
            if (hitLayers.value == 0 || (hitLayers.value & teammateLayer.value) != 0)
            {
                int mask = Physics2D.GetLayerCollisionMask(selfLayer);
                mask &= ~teammateLayer.value;
                mask &= ~(1 << selfLayer);
                hitLayers = mask;
            }
        }

#if UNITY_EDITOR
        if (hitLayers.value == 0)
        {
            Debug.LogWarning($"[RangedAttack] hitLayers=0. self '{LayerMask.LayerToName(selfLayer)}'({selfLayer}) / " +
                             $"인덱스/이름 매핑 또는 충돌 행렬을 확인하세요. {GetPath(this)}", this);
        }
#endif
    }

    LayerMask ResolveOpponentMask(int selfLayer)
    {
        // 0) 인덱스 매핑 우선
        if (preferIndexMapping && InRange(playerLayerIndex) && InRange(enemyLayerIndex))
        {
            if (selfLayer == playerLayerIndex) return 1 << enemyLayerIndex;
            if (selfLayer == enemyLayerIndex)  return 1 << playerLayerIndex;
        }

        // 1) 이름 매핑
        string selfName = LayerMask.LayerToName(selfLayer);
        if (selfName == selfLayerNameForPlayer)
        {
            int opp = LayerMask.NameToLayer(opponentLayerNameForPlayer);
            if (opp != -1) return 1 << opp;
        }
        else if (selfName == selfLayerNameForEnemy)
        {
            int opp = LayerMask.NameToLayer(opponentLayerNameForEnemy);
            if (opp != -1) return 1 << opp;
        }

        // 2) 커스텀 매핑
        if (customOpponentMaps != null)
        {
            for (int i = 0; i < customOpponentMaps.Length; i++)
            {
                if (customOpponentMaps[i].selfLayerName == selfName)
                {
                    int opp = LayerMask.NameToLayer(customOpponentMaps[i].opponentLayerName);
                    if (opp != -1) return 1 << opp;
                }
            }
        }

        // 3) 실패
        return 0;
    }

    static bool InRange(int layerIndex) => layerIndex >= 0 && layerIndex < 32;

    // ========= 핵심: 런타임 DI(주입) 경로 =========
    public void SetProjectilePrefab(Projectile prefab)
    {
        projectilePrefab = prefab;
        TryBuildPool(); // 새 프리팹으로 풀 다시 준비
    }

    void TryBuildPool()
    {
        if (projectilePrefab == null)
        {
#if UNITY_EDITOR
            Debug.LogWarning($"[RangedAttack] projectilePrefab이 비었습니다. (프리팹 런타임 로드 시 참조가 끊긴 경우가 흔합니다) : {GetPath(this)}", this);
#endif
            _poolReady = false;
            _pool = null;
            return;
        }

        // 이미 풀 있으면 파기
        _pool = new ObjectPool<Projectile>(
            createFunc: () =>
            {
                if (projectilePrefab == null)
                {
                    Debug.LogError("[RangedAttack] createFunc: projectilePrefab == null", this);
                    return null;
                }
                var p = Instantiate(projectilePrefab);
                p.gameObject.SetActive(false);
                p.OnDespawnRequested = ReturnToPool;
                return p;
            },
            actionOnGet:  p => { if (p != null) p.gameObject.SetActive(true);  },
            actionOnRelease: p => { if (p != null) p.gameObject.SetActive(false); },
            actionOnDestroy: p => { if (p != null) Destroy(p.gameObject); },
            collectionCheck: false, defaultCapacity: 16, maxSize: 256
        );
        _poolReady = true;
    }

    public bool TryAttack(Transform target)
    {
        if (!CanFireNow()) return false;
        if (target == null) return false;
        if (!_poolReady)
        {
#if UNITY_EDITOR
            Debug.LogWarning("[RangedAttack] 풀 준비가 안 됨(projectilePrefab 미지정). SetProjectilePrefab() 또는 인스펙터 지정 필요.", this);
#endif
            return false;
        }

        float dist = Vector2.Distance(transform.position, target.position);
        if (blockOutOfRange && dist > maxRange) return false;

        // 👉 기존 루틴 중복 방지
        CancelAll();
        _fireRoutine = StartCoroutine(FireRoutine(target));
        _lastShotTime = Time.time;
        return true;
    }

    bool CanFireNow() => Time.time >= _lastShotTime + cooldown;

    IEnumerator FireRoutine(Transform target)
    {
        if (prepareTime > 0f)
            yield return new WaitForSeconds(prepareTime);

        // 스냅샷/실시간 조준 옵션은 필요시 확장
        Vector2 dir = ((Vector2)target.position - (Vector2)firePoint.position).normalized;
        SpawnAndFire(dir);

        _fireRoutine = null;
    }

    void SpawnAndFire(Vector2 dir)
    {
        var proj = _pool.Get();
        if (proj == null) return;

        proj.transform.position = firePoint.position;
        proj.transform.rotation = Quaternion.FromToRotation(Vector3.right, dir);
        proj.transform.localScale = projectileScale;

        // 투사체 레이어 강제
        if (projectileLayerOverride >= 0 && projectileLayerOverride < 32)
        {
            if (overrideChildrenLayer)
                SetLayerRecursively(proj.gameObject, projectileLayerOverride);
            else
                proj.gameObject.layer = projectileLayerOverride;
        }

        // 소유자 충돌 무시 및 팀 마스크
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

    /// <summary>
    /// ▶ 모든 발사 관련 루틴/예약을 즉시 중단 (AICore.StopAllActionsHard에서 호출)
    /// </summary>
    public void CancelAll()
    {
        if (_fireRoutine != null)
        {
            try { StopCoroutine(_fireRoutine); } catch { }
            _fireRoutine = null;
        }
        CancelInvoke();
        // 투사체는 자체 수명 관리(Projectile)에서 풀 복귀
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1, 0.8f, 0f, 0.25f);
        Gizmos.DrawWireSphere(firePoint ? firePoint.position : transform.position, 0.1f);
        Gizmos.color = new Color(0, 1, 0.2f, 0.25f);
        Gizmos.DrawWireSphere(transform.position, maxRange);
    }

    // ===== 유틸 =====
    static void SetLayerRecursively(GameObject go, int layer)
    {
        go.layer = layer;
        var t = go.transform;
        for (int i = 0; i < t.childCount; i++)
            SetLayerRecursively(t.GetChild(i).gameObject, layer);
    }

    static string GetPath(Component c)
    {
        var t = c.transform;
        string path = t.name;
        while (t.parent != null) { t = t.parent; path = t.name + "/" + path; }
        return path;
    }
}
