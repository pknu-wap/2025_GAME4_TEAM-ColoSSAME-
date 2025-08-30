using System.Collections;
using UnityEngine;
using UnityEngine.Pool;

[DisallowMultipleComponent]
[RequireComponent(typeof(Collider2D))]
public class RangedAttack : MonoBehaviour
{
    [Header("Owner / Team (자동 설정 대상)")]
    public LayerMask teammateLayer;
    public LayerMask hitLayers;

    [Header("우선 매핑: 레이어 '인덱스'(숫자)")]
    public int  playerLayerIndex = 10;
    public int  enemyLayerIndex  = 11;
    public bool preferIndexMapping = true;

    [Header("보조 매핑: 레이어 '이름'(선택)")]
    public string selfLayerNameForPlayer = "Player";
    public string selfLayerNameForEnemy  = "Enemy";
    public string opponentLayerNameForPlayer = "Enemy";
    public string opponentLayerNameForEnemy  = "Player";

    [System.Serializable] public struct OpponentMap { public string selfLayerName; public string opponentLayerName; }
    public OpponentMap[] customOpponentMaps = new OpponentMap[0];

    [Header("Projectile")]
    public Projectile projectilePrefab;
    public Transform  firePoint;

    [Header("Projectile Layer Override")]
    public int  projectileLayerOverride = -1;
    public bool overrideChildrenLayer   = true;

    [Header("Stats / Timing")]
    public float   prepareTime     = 0.15f;
    public float   cooldown        = 0.6f;
    public Vector3 projectileScale = Vector3.one;
    public float   projectileSpeed = 10f;
    public float   maxRange        = 8f;

    [Header("Damage 기본값")]
    [Tooltip("오너 연동이 꺼져있거나 실패 시 사용할 기본 데미지")]
    public int baseDamage = 10;

    [Header("Owner Link")]
    [Tooltip("오너(AICore). 비워두면 부모에서 자동 탐색")]
    public AICore ownerAI;

    [Tooltip("스탯 주입 완료(StatsReady) 이전에는 절대 발사하지 않음")]
    public bool waitForStatsReady = true;

    [Tooltip("준비 완료 시점에 owner.attackDamage를 1회 스냅샷해서 baseDamage로 저장")]
    public bool snapshotOwnerDamageOnReady = true;

    [Tooltip("스냅샷 대신, 매 발사 시 owner.attackDamage를 읽어 최종 데미지 계산")]
    public bool liveOwnerDamageOnFire = false;

    [Tooltip("최종 = round(atk * multiplier) + bonus")]
    public float ownerDamageMultiplier = 1f;
    public int   ownerDamageBonus      = 0;

    [Tooltip("최소/최대 데미지(-1은 상한 미적용)")]
    public int   minDamage = 1;
    public int   maxDamage = -1;

    [Header("Fallback Snapshot (Ready 신호가 없거나 늦을 때)")]
    [Tooltip("Ready 이벤트가 없어도 첫 발사 직전에 오너 ATK가 준비되어 있으면 1회 스냅샷")]
    public bool lazySnapshotOnFirstFire = true;

    [Tooltip("게으른 스냅샷을 허용하는 최소 오너 ATK(이상일 때만 스냅샷)")]
    public int lazySnapshotMinOwnerAtk = 1;

    [Header("Behavior")]
    public bool snapshotAimOnRelease = true;
    public bool blockOutOfRange = true;

    [Header("Auto Rebind")]
    public bool autoRebindAfterSpawn = true;

    [Header("Debug")]
    public bool debugLogging = false;

    // ── 내부 상태 ────────────────────────────────────────────
    float _lastShotTime = -999f;
    Collider2D _ownerCollider;
    IObjectPool<Projectile> _pool;
    bool _poolReady;
    Coroutine _fireRoutine;

    // Ready 게이트
    StatsReady _statsReady;
    bool _isReadyObserved; // StatsReady.IsReady 또는 OnReady 수신 완료 여부

    // 스냅샷 상태 추적
    int  _initialBaseDamage;
    bool _snapshottedByReady;   // Ready 이벤트로 스냅샷 했는가
    bool _snapshottedLazily;    // 첫 발사 직전에 폴백 스냅샷했는가

    // ── Unity lifecycle ──────────────────────────────────────
    void Reset()
    {
        if (!firePoint) firePoint = transform;
    }

    void Awake()
    {
        _ownerCollider = GetComponent<Collider2D>();
        if (!firePoint) firePoint = transform;

        _initialBaseDamage = baseDamage;

        EnsureOwnerAndHook();
        TryBuildPool();
    }

    void OnEnable()
    {
        if (autoRebindAfterSpawn)
            StartCoroutine(Co_RebindNextFrame());
    }

    void OnDisable()
    {
        UnhookReady();
        CancelAll();
    }

    void OnDestroy()
    {
        UnhookReady();
    }

    IEnumerator Co_RebindNextFrame()
    {
        yield return null;
        RebindNow(false);

        if (teammateLayer.value == 0 || hitLayers.value == 0 || (hitLayers.value & teammateLayer.value) != 0)
        {
            yield return null;
            RebindNow(false);
        }

        // 혹시 오너가 늦게 붙은 경우 다시 훅
        EnsureOwnerAndHook();
    }

    // ── 핵심: 오너/Ready 훅 ──────────────────────────────────
    void EnsureOwnerAndHook()
    {
        if (ownerAI == null)
            ownerAI = GetComponentInParent<AICore>();

        UnhookReady();
        _statsReady = ownerAI ? ownerAI.GetComponent<StatsReady>() : null;

        if (!waitForStatsReady)
        {
            _isReadyObserved = true;
            return;
        }

        if (_statsReady == null)
        {
            // ⚠️ 예전엔 여기서 게이트를 열었지만, 조기발사로 baseDamage=10 문제가 생김.
            // → 게이트는 열지 않되, lazySnapshotOnFirstFire가 켜져 있으면 첫 발사 직전 스냅샷으로 보정.
            _isReadyObserved = false; // Ready가 없으면 기본은 막는다.
            if (debugLogging)
                Debug.LogWarning($"[RangedAttack] StatsReady 미존재: Ready 전 발사 차단. (owner={ownerAI?.name ?? "null"})", this);
            return;
        }

        if (_statsReady.IsReady)
        {
            OnOwnerReady();
        }
        else
        {
            _isReadyObserved = false;
            _statsReady.OnReady += OnOwnerReady;
        }
    }

    void UnhookReady()
    {
        if (_statsReady != null)
            _statsReady.OnReady -= OnOwnerReady;
    }

    void OnOwnerReady()
    {
        _isReadyObserved = true;

        if (snapshotOwnerDamageOnReady && ownerAI != null)
        {
            baseDamage = ComputeOwnerDamage(ownerAI.attackDamage);
            _snapshottedByReady = true;

            if (debugLogging)
                Debug.Log($"[RangedAttack] Snapshot by READY: baseDamage={baseDamage} (owner={ownerAI.name}, atk={ownerAI.attackDamage})", this);
        }
        UnhookReady();
    }

    // ── 외부 API ─────────────────────────────────────────────
    public void RebindNow(bool force) => AutoConfigureMasksImpl(force);

    public bool TryAttack(Transform target)
    {
        // 1) Ready 게이트: Ready 이전이면 발사 금지
        if (waitForStatsReady && !_isReadyObserved)
        {
            // 단, 폴백 스냅샷 옵션이 켜져 있고 오너 ATK가 준비되었다면,
            // 여기서 1회 스냅샷 후 Ready 없이도 발사 허용.
            if (lazySnapshotOnFirstFire && ownerAI != null &&
                ownerAI.attackDamage >= lazySnapshotMinOwnerAtk &&
                snapshotOwnerDamageOnReady && !_snapshottedByReady && !_snapshottedLazily)
            {
                baseDamage = ComputeOwnerDamage(ownerAI.attackDamage);
                _snapshottedLazily = true;

                if (debugLogging)
                    Debug.Log($"[RangedAttack] Lazy snapshot on first fire: baseDamage={baseDamage} (owner={ownerAI.name}, atk={ownerAI.attackDamage})", this);

                // 게이트 임시 해제(이 발사만). 이후에도 Ready가 오면 정식 스냅샷 로직이 다시 동작.
            }
            else
            {
                if (debugLogging) Debug.Log($"[RangedAttack] blocked: waiting StatsReady (owner={ownerAI?.name ?? "null"})", this);
                EnsureOwnerAndHook(); // 혹시나 오너/Ready 재훅
                return false;
            }
        }

        if (!CanFireNow()) return false;
        if (target == null) return false;
        if (!_poolReady)
        {
#if UNITY_EDITOR
            Debug.LogWarning("[RangedAttack] 풀 미준비. projectilePrefab 지정 필요.", this);
#endif
            return false;
        }

        float dist = Vector2.Distance(transform.position, target.position);
        if (blockOutOfRange && dist > maxRange) return false;

        CancelAll();
        _fireRoutine = StartCoroutine(FireRoutine(target));
        _lastShotTime = Time.time;
        return true;
    }

    // ── 발사 루틴 ────────────────────────────────────────────
    bool CanFireNow() => Time.time >= _lastShotTime + cooldown;

    IEnumerator FireRoutine(Transform target)
    {
        if (prepareTime > 0f) yield return new WaitForSeconds(prepareTime);
        Vector2 dir = ((Vector2)target.position - (Vector2)firePoint.position).normalized;
        SpawnAndFire(dir);
        _fireRoutine = null;
    }

    void SpawnAndFire(Vector2 dir)
    {
        var proj = _pool.Get();
        if (proj == null) return;

        proj.transform.position   = firePoint.position;
        proj.transform.rotation   = Quaternion.FromToRotation(Vector3.right, dir);
        proj.transform.localScale = projectileScale;

        if (projectileLayerOverride >= 0 && projectileLayerOverride < 32)
        {
            if (overrideChildrenLayer) SetLayerRecursively(proj.gameObject, projectileLayerOverride);
            else                       proj.gameObject.layer = projectileLayerOverride;
        }

        proj.IgnoreCollider(_ownerCollider);
        proj.SetAllyMask(teammateLayer);

        // 최종 데미지: 실시간 모드가 켜져 있으면 오너 ATK로 계산, 아니면 baseDamage 사용
        int finalDamage = baseDamage;
        if (liveOwnerDamageOnFire && ownerAI != null)
            finalDamage = ComputeOwnerDamage(ownerAI.attackDamage);

        if (debugLogging)
        {
            string ownerName = ownerAI ? ownerAI.name : "NULL_OWNER";
            Debug.Log($"[RangedAttack] Fired → damage={finalDamage} (owner={ownerName}, base={baseDamage}, " +
                      $"readyGate={(waitForStatsReady ? (_isReadyObserved ? "READY" : "BLOCK") : "OFF")}, " +
                      $"lazySnap={_snapshottedLazily}, readySnap={_snapshottedByReady})  path={GetPath(this)}", this);
        }

        proj.Launch(new Projectile.Params
        {
            speed = projectileSpeed,
            damage = finalDamage,
            maxTravelDistance = maxRange,
            hitLayers = hitLayers,
            attacker = gameObject,
        });
    }

    int ComputeOwnerDamage(int ownerAtk)
    {
        float raw = (ownerAtk * Mathf.Max(0.0001f, ownerDamageMultiplier)) + ownerDamageBonus;
        int dmg   = Mathf.RoundToInt(raw);
        if (maxDamage > 0) dmg = Mathf.Clamp(dmg, Mathf.Max(1, minDamage), maxDamage);
        else               dmg = Mathf.Max(1, minDamage > 0 ? Mathf.Max(minDamage, dmg) : Mathf.Max(1, dmg));
        return dmg;
    }

    // ── 레이어/풀/유틸 ───────────────────────────────────────
    void AutoConfigureMasksImpl(bool force)
    {
        int selfLayer = gameObject.layer;

        if (force || teammateLayer.value == 0)
            teammateLayer = 1 << selfLayer;

        if (force || hitLayers.value == 0)
        {
            hitLayers = ResolveOpponentMask(selfLayer);

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
            Debug.LogWarning($"[RangedAttack] hitLayers=0. self '{LayerMask.LayerToName(selfLayer)}'({selfLayer}) 확인. {GetPath(this)}", this);
#endif
    }

    LayerMask ResolveOpponentMask(int selfLayer)
    {
        if (preferIndexMapping && IsValidLayerIndex(playerLayerIndex) && IsValidLayerIndex(enemyLayerIndex))
        {
            if (selfLayer == playerLayerIndex) return 1 << enemyLayerIndex;
            if (selfLayer == enemyLayerIndex)  return 1 << playerLayerIndex;
        }

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

        if (customOpponentMaps != null)
        {
            for (int i = 0; i < customOpponentMaps.Length; i++)
                if (customOpponentMaps[i].selfLayerName == selfName)
                {
                    int opp = LayerMask.NameToLayer(customOpponentMaps[i].opponentLayerName);
                    if (opp != -1) return 1 << opp;
                }
        }
        return 0;
    }

    static bool IsValidLayerIndex(int layerIndex) => layerIndex >= 0 && layerIndex < 32;

    void TryBuildPool()
    {
        if (projectilePrefab == null)
        {
#if UNITY_EDITOR
            Debug.LogWarning($"[RangedAttack] projectilePrefab 비어있음. {GetPath(this)}", this);
#endif
            _poolReady = false; _pool = null; return;
        }

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
            actionOnGet:     p => { if (p != null) p.gameObject.SetActive(true);  },
            actionOnRelease: p => { if (p != null) p.gameObject.SetActive(false); },
            actionOnDestroy: p => { if (p != null) Destroy(p.gameObject); },
            collectionCheck: false, defaultCapacity: 16, maxSize: 256
        );
        _poolReady = true;
    }

    void ReturnToPool(Projectile p) => _pool.Release(p);

    public void CancelAll()
    {
        if (_fireRoutine != null) { try { StopCoroutine(_fireRoutine); } catch { } _fireRoutine = null; }
        CancelInvoke();
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1, 0.8f, 0f, 0.25f);
        Gizmos.DrawWireSphere(firePoint ? firePoint.position : transform.position, 0.1f);
        Gizmos.color = new Color(0, 1, 0.2f, 0.25f);
        Gizmos.DrawWireSphere(transform.position, maxRange);
    }

    static void SetLayerRecursively(GameObject go, int layer)
    {
        go.layer = layer;
        var t = go.transform;
        for (int i = 0; i < t.childCount; i++)
            SetLayerRecursively(t.GetChild(i).gameObject, layer);
    }

    static string GetPath(Component c)
    {
        if (c == null) return "(null)";
        Transform t = c.transform; if (t == null) return "(no transform)";
        string path = t.name;
        for (Transform p = t.parent; p != null; p = p.parent) path = p.name + "/" + path;
        return path;
    }
}
