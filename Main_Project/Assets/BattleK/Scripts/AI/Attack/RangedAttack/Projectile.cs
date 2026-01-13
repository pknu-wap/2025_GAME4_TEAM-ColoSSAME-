using System;
using BattleK.Scripts.AI;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
[DisallowMultipleComponent]
public class Projectile : MonoBehaviour
{
    [Serializable]
    public struct Params
    {
        public float speed;
        public int damage;
        public float maxTravelDistance;
        public LayerMask hitLayers;
        public GameObject attacker;
    }

    /// <summary>풀로 복귀할 때 호출됨. (RangedAttack에서 IObjectPool.Release)</summary>
    public Action<Projectile> OnDespawnRequested;

    [Header("Behavior")]
    [Tooltip("피격 시 대상의 evasionRate(%)로 회피 판정을 수행할지")]
    public bool useEvasionCheck = true;

    private Rigidbody2D _rb;
    private Collider2D _col;

    private Vector2 _dir;
    private Vector2 _spawnPos;

    private float _speed;
    private int _damage;
    private float _maxDist;
    private LayerMask _hitLayers;
    private GameObject _attacker;

    // 아군 마스크(아군 충돌 무시)
    private int _allyLayerMask;

    // 무시할 콜라이더(발사자)
    private Collider2D _ignored;

    private bool _launched;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _col = GetComponent<Collider2D>();

        _rb.gravityScale   = 0f;
        _rb.isKinematic    = false;
        _rb.interpolation  = RigidbodyInterpolation2D.Interpolate;

        _col.isTrigger = true;
    }

    /// <summary>아군 마스크 설정(레이어 비트마스크)</summary>
    public void SetAllyMask(LayerMask allyMask) => _allyLayerMask = allyMask.value;

    /// <summary>발사자와의 충돌 무시</summary>
    public void IgnoreCollider(Collider2D owner)
    {
        _ignored = owner;
        if (owner) Physics2D.IgnoreCollision(_col, owner, true);
    }

    /// <summary>투사체 발사 시작. RangedAttack에서 rotation을 통해 방향을 미리 설정함.</summary>
    public void Launch(Params p)
    {
        _spawnPos  = transform.position;
        _speed     = Mathf.Max(0f, p.speed);
        _damage    = p.damage;
        _maxDist   = Mathf.Max(0.01f, p.maxTravelDistance);
        _hitLayers = p.hitLayers;
        _attacker  = p.attacker;

        _dir = transform.right.normalized; // RangedAttack: Quaternion.FromToRotation(Vector3.right, dir)

        _launched = true;
        enabled   = true;
    }

    private void FixedUpdate()
    {
        if (!_launched) { _rb.velocity = Vector2.zero; return; }

        _rb.velocity = _dir * _speed;

        float traveled = Vector2.Distance(_spawnPos, _rb.position);
        if (traveled >= _maxDist)
        {
            Despawn();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!_launched) return;

        // 자기/아군 무시
        if (other == _ignored) return;
        if (((1 << other.gameObject.layer) & _allyLayerMask) != 0) return;

        // 타격 가능한 레이어만
        if (((1 << other.gameObject.layer) & _hitLayers.value) == 0) return;

        // 대상 유효성
        var ai = other.GetComponent<AICore>();
        if (ai != null)
        {
            // 이미 죽었거나 비활성? 무시
            if (ai.IsDead || ai.State == State.Death || !ai.gameObject.activeInHierarchy)
            {
                Despawn();
                return;
            }

            // ✅ 회피 판정 (원거리도 근접과 동일하게 evasionRate 사용)
            if (useEvasionCheck && Roll(ai.evasionRate))
            {
                ai.StateMachine.ChangeState(new EvadeState(ai)); // 연출 등은 EvadeState에서
                Despawn();
                return;
            }
        }

        // 데미지 적용: IDamageable → AICore가 DamageState로 진입
        var dmg = other.GetComponent<IDamageable>();
        if (dmg != null)
        {
            dmg.ApplyDamage(_damage, _attacker);
        }

        Despawn();
    }

    private void OnDisable()
    {
        // 풀 복귀/파괴 시 속도 클린업
        if (_rb) _rb.velocity = Vector2.zero;
        _launched = false;
    }

    /// <summary>풀로 반환(혹은 파괴). RangedAttack가 풀 Release를 호출함.</summary>
    public void Despawn()
    {
        _launched = false;
        enabled = false;
        OnDespawnRequested?.Invoke(this);
    }

    // ───── 유틸 ─────
    private static bool Roll(float percent)
    {
        if (percent <= 0f) return false;
        if (percent >= 100f) return true;
        return UnityEngine.Random.Range(0f, 100f) < percent;
    }
}
