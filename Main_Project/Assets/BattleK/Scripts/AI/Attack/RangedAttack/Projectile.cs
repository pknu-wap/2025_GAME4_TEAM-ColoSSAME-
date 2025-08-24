using System;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
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

    public Action<Projectile> OnDespawnRequested;

    Rigidbody2D _rb;
    Collider2D _col;
    Vector2 _dir;
    Vector2 _spawnPos;
    float _speed;
    int _damage;
    float _maxDist;
    LayerMask _hitLayers;
    GameObject _attacker;
    int _allyLayerMask;

    void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _col = GetComponent<Collider2D>();
        _rb.gravityScale = 0f;
        _rb.isKinematic = false;
        _rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        _col.isTrigger = true;
    }

    public void SetAllyMask(LayerMask allyMask) => _allyLayerMask = allyMask.value;

    public void IgnoreCollider(Collider2D owner)
    {
        if (owner) Physics2D.IgnoreCollision(_col, owner, true);
    }

    public void Launch(Params p)
    {
        _spawnPos = transform.position;
        _speed    = Mathf.Max(0f, p.speed);
        _damage   = p.damage;
        _maxDist  = Mathf.Max(0.01f, p.maxTravelDistance);
        _hitLayers = p.hitLayers;
        _attacker = p.attacker;

        _dir = transform.right.normalized; // RangedAttack에서 rotation으로 방향 세팅
    }

    void FixedUpdate()
    {
        _rb.velocity = _dir * _speed;

        float traveled = Vector2.Distance(_spawnPos, _rb.position);
        if (traveled >= _maxDist)
            Despawn();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // 아군 레이어면 무시
        if (((1 << other.gameObject.layer) & _allyLayerMask) != 0)
            return;

        // 타격 가능한 레이어만
        if (((1 << other.gameObject.layer) & _hitLayers.value) == 0)
            return;

        // 데미지 적용: IDamageable에 위임 → AICore는 DamageState로 진입
        var dmg = other.GetComponent<IDamageable>();
        if (dmg != null)
        {
            dmg.ApplyDamage(_damage, _attacker);
        }

        Despawn();
    }

    void OnDisable()
    {
        if (_rb) _rb.velocity = Vector2.zero;
    }

    void Despawn()
    {
        OnDespawnRequested?.Invoke(this);
    }
}
