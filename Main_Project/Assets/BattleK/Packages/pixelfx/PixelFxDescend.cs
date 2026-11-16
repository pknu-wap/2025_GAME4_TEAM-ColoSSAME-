using System.Reflection;
using BattleK.Scripts.AI.Skill.Base.Logic.LogicBase;
using UnityEngine;

// 투사체 그림을 시작 높이(또는 쏜 유닛의 손)에서 대상 발밑 쪽 높이까지 일직선으로 날림
// 높이는 화면 위쪽 기준이라 대각선, 위아래로 쏴도 손에서 나와 상대 몸에 맞음 (판정은 발밑 선 그대로)
public class PixelFxDescend : MonoBehaviour
{
    public Transform visual;
    public float startHeight = 0.45f; // 월드
    public float endHeight = 0.05f;   // 월드
    public float arriveEarly;         // 계산된 맞는 지점보다 이만큼 먼저 도착 (보통 0)
    public bool fromHand;             // 켜면 시전자 손에서 출발
    public float growTime = 0.2f;     // 쏜 직후 작게 시작해서 커짐 (긴 꼬리가 시전자 뒤로 삐져나오지 않게)

    [Header("시전 이펙트 (출발 자리에 남음)")]
    public GameObject castEffect;
    public float castScale = 0.1f;
    public float castLifetime = 0.25f;
    public float castFade = 0.1f;
    public int castSortingOrder = 55;

    private const string HandName = "P_Weapon"; // SPUM 손 (무기 붙는 자리)

    // 시전자와 대상은 LogicProcessor가 Initialize에서 받지만 공개돼 있지 않아서 읽기만 함
    private static readonly FieldInfo OwnerField =
        typeof(LogicProcessor).GetField("_owner", BindingFlags.NonPublic | BindingFlags.Instance);
    private static readonly FieldInfo TargetField =
        typeof(LogicProcessor).GetField("_targetTransform", BindingFlags.NonPublic | BindingFlags.Instance);

    // 타격 이펙트가 쓰는 값: 발밑 선 위의 그림 머리 위치와 날아가는 방향 (월드)
    public Vector3 Ground { get; private set; }
    public Vector2 Direction { get; private set; }
    public bool Tracking { get; private set; }

    private LogicProcessor _logic;
    private Vector3 _start, _ground0, _ground1;
    private float _height0, _height1;
    private float _forward = float.NaN; // 프리팹에 정해진 앞쪽 거리 (월드)
    private Vector3 _baseScale;
    private float _length = -1f, _age;

    private void OnEnable() // 풀에서 다시 나올 때 초기화
    {
        _length = -1f;
        Tracking = false;
    }

    private void LateUpdate()
    {
        if (!visual) return;
        if (_length < 0f) Measure();

        var t = _length > 0f ? Mathf.Clamp01(Vector2.Distance(_start, transform.position) / _length) : 0f;
        Ground = _length > 0f
            ? Vector3.Lerp(_ground0, _ground1, t)
            : transform.position + (Vector3)(Direction * _forward); // 대상이 없으면 루트를 따라감
        var p = Ground + Vector3.up * Mathf.Lerp(_height0, _height1, t);
        visual.position = new Vector3(p.x, p.y, visual.position.z);

        // 그림 머리를 출발점에서 도착점까지의 선 방향으로 (루트의 회전, 좌우 반전 안에서)
        var path = _length > 0f
            ? (_ground1 + Vector3.up * _height1) - (_ground0 + Vector3.up * _height0)
            : (Vector3)Direction;
        var local = transform.InverseTransformVector(path);
        visual.localRotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(local.y, local.x) * Mathf.Rad2Deg);

        _age += Time.deltaTime;
        var grow = growTime > 0f ? Mathf.Clamp01(_age / growTime) : 1f;
        visual.localScale = _baseScale * Mathf.Lerp(0.2f, 1f, grow); // 머리(피벗) 기준으로 커짐
    }

    private void Measure()
    {
        if (!_logic) _logic = GetComponent<LogicProcessor>();
        if (float.IsNaN(_forward))
        {
            _forward = visual.localPosition.x * Mathf.Abs(transform.lossyScale.x);
            _baseScale = visual.localScale;
        }
        _age = 0f;
        _start = transform.position;

        // 날아가는 방향: 루트 +x (왼쪽이면 x 스케일 반전)
        var flip = transform.lossyScale.x < 0f ? -1f : 1f;
        Direction = ((Vector2)transform.right * flip).normalized;

        // 실제 판정 크기로 맞는 순간까지 날아갈 거리
        var target = Read<Transform>(TargetField);
        _length = target
            ? Mathf.Max(0.01f, Vector2.Distance(_start, target.position)
                - (Front(GetComponent<Collider2D>()) + Radius(target.GetComponent<Collider2D>()) + arriveEarly))
            : 0f;

        _ground0 = _start + (Vector3)(Direction * _forward);
        _ground1 = _start + (Vector3)(Direction * (_length + _forward));
        _height0 = startHeight;
        _height1 = endHeight;

        var hand = fromHand ? FindHand(Read<Component>(OwnerField)) : null;
        if (hand)
        {
            _height0 = hand.position.y - _start.y;
            _ground0 = new Vector3(hand.position.x, _start.y, _start.z);
        }

        Ground = _ground0;
        Tracking = true;

        if (castEffect)
        {
            var dir = Direction.x < 0f ? -1f : 1f;
            PixelFxImpact.Spawn(castEffect, _ground0 + Vector3.up * _height0, castScale, dir, castSortingOrder, castLifetime, castFade);
        }
    }

    private T Read<T>(FieldInfo field) where T : class =>
        _logic && field != null ? field.GetValue(_logic) as T : null;

    // 무기를 든 손 우선, 없으면 첫 번째 손
    private static Transform FindHand(Component owner)
    {
        if (!owner) return null;
        Transform first = null;
        foreach (var t in owner.GetComponentsInChildren<Transform>())
        {
            if (t.name != HandName) continue;
            if (!first) first = t;
            var sr = t.GetComponentInChildren<SpriteRenderer>();
            if (sr && sr.sprite) return t;
        }
        return first;
    }

    // 루트에서 판정 앞쪽 끝까지 (월드, 진행 방향 기준)
    private float Front(Collider2D col)
    {
        var sx = Mathf.Abs(transform.lossyScale.x);
        return col switch
        {
            BoxCollider2D box => (box.offset.x + box.size.x * 0.5f) * sx,
            CircleCollider2D circle => (circle.offset.x + circle.radius) * sx,
            _ => 0f,
        };
    }

    private static float Radius(Collider2D col) => col ? col.bounds.extents.x : 0f;
}
