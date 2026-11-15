using System.Reflection;
using BattleK.Scripts.AI.Skill.Base.Logic.LogicBase;
using UnityEngine;

// 투사체 그림을 시작 높이(또는 쏜 유닛의 손)에서 대상 발밑까지 일직선으로 날림 (판정은 발밑 선 그대로)
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

    private LogicProcessor _logic;
    private Vector3 _start, _from, _to; // _from, _to는 그림의 로컬 위치
    private float _endX = float.NaN;
    private Vector3 _baseScale;
    private float _age;
    private float _length = -1f, _angle;

    private void OnEnable() => _length = -1f; // 풀에서 다시 나올 때 초기화

    private void LateUpdate()
    {
        if (!visual) return;
        if (_length < 0f) Measure();

        var t = _length > 0f ? Mathf.Clamp01(Vector2.Distance(_start, transform.position) / _length) : 0f;
        visual.localPosition = Vector3.Lerp(_from, _to, t);
        visual.localRotation = Quaternion.Euler(0f, 0f, _angle); // 좌우 반전은 루트 스케일이 처리

        _age += Time.deltaTime;
        var grow = growTime > 0f ? Mathf.Clamp01(_age / growTime) : 1f;
        visual.localScale = _baseScale * Mathf.Lerp(0.2f, 1f, grow); // 머리(피벗) 기준으로 커짐
    }

    private void Measure()
    {
        if (!_logic) _logic = GetComponent<LogicProcessor>();
        if (float.IsNaN(_endX))
        {
            _endX = visual.localPosition.x; // 프리팹에 정해진 앞쪽 거리
            _baseScale = visual.localScale;
        }
        _age = 0f;

        var sx = Mathf.Max(0.0001f, Mathf.Abs(transform.lossyScale.x));
        var sy = Mathf.Max(0.0001f, Mathf.Abs(transform.lossyScale.y));
        var z = visual.localPosition.z;
        _start = transform.position;
        _to = new Vector3(_endX, endHeight / sy, z);

        var hand = fromHand ? FindHand(Read<Component>(OwnerField)) : null;
        _from = hand ? transform.InverseTransformPoint(hand.position) : new Vector3(_endX, startHeight / sy, z);
        _from.z = z;

        if (castEffect)
        {
            var dir = transform.lossyScale.x < 0f ? -1f : 1f;
            PixelFxImpact.Spawn(castEffect, transform.TransformPoint(_from), castScale, dir, castSortingOrder, castLifetime, castFade);
        }

        // 실제 판정 크기로 맞는 순간까지 날아갈 거리
        var target = Read<Transform>(TargetField);
        _length = target
            ? Mathf.Max(0.01f, Vector2.Distance(_start, target.position)
                - (Front(GetComponent<Collider2D>()) + Radius(target.GetComponent<Collider2D>()) + arriveEarly))
            : 0f;

        var dx = _length / sx + _to.x - _from.x;
        _angle = _length > 0f ? Mathf.Atan2(_to.y - _from.y, dx) * Mathf.Rad2Deg : 0f;
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
