using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using BattleK.Scripts.AI;
using BattleK.Scripts.AI.Skill.Base.Logic.LogicBase;
using BattleK.Scripts.Combat.Damage;
using BattleK.Scripts.Manager;
using UnityEngine;

// 근접 평타: 상대 위치에 나온 순간 베기, 타격 이펙트를 띄움 (시전자에서 상대 쪽을 보게)
public class PixelFxStrike : MonoBehaviour
{
    [System.Serializable]
    public class Entry
    {
        public GameObject effect;
        public bool fromOwner;      // 켜면 시전자 발밑 기준 (무기 쪽), 끄면 상대 발밑 기준
        public Vector2 offset;      // 기준점에서 (x는 상대 쪽 방향, 월드)
        public float scale = 0.25f;
        public float delay;
        public float lifetime;      // 0이면 이펙트 길이대로
        public float fade;
        public float speed = 1f;    // 재생 속도
        public int sortingOrder = 60;
        public float maxTilt = -1f; // 0 이상이면 이 이펙트만 이 각도까지 기울임 (-1이면 컴포넌트 값)
        public float skip;          // 이펙트 앞부분을 이만큼 건너뛰고 시작 (초)
        public float fly;           // 0보다 크면 시전자 쪽에서 상대 발밑 쪽으로 이만큼 걸려 날아감 (초)
        public bool family;         // 켜면 시전자 가문 색으로 칠함 (PixelFxFamilyColors)
    }

    // 맞는 반응 (PixelFxHitReact). time은 이펙트 시작부터, push는 월드 거리
    [System.Serializable]
    public class Hit
    {
        public float time;
        public float flash = 0.06f;
        public float freeze = 0.12f;
        public float push = 0.035f;
    }

    public Entry[] effects;
    public Entry[] onHit; // reactAll일 때 맞은 상대마다 맞는 순간에 띄움
    public Hit[] hits;
    public Material flashMaterial;
    public float maxTilt = 45f; // 대각선 상대 쪽으로 기울이는 최대 각도

    private static readonly FieldInfo OwnerField =
        typeof(LogicProcessor).GetField("_owner", BindingFlags.NonPublic | BindingFlags.Instance);
    private static readonly FieldInfo TargetField =
        typeof(LogicProcessor).GetField("_targetTransform", BindingFlags.NonPublic | BindingFlags.Instance);

    public bool reactAll; // 켜면 실제로 맞은 상대 전원에게 반응 (범위 스킬). 끄면 대상 한 명만

    private LogicProcessor _logic;
    private bool _played;
    private float _enabledAt;
    private CombatEvents _events;
    private readonly HashSet<StaticAICore> _reacted = new();

    private void OnEnable() // 풀에서 다시 나올 때
    {
        _played = false;
        _enabledAt = Time.time;
        _reacted.Clear();
        // 생기자마자(StartProcess) 맞히므로 미리 구독
        if (reactAll && _events == null && AI_Manager.Instance && AI_Manager.Instance.Combat != null)
        {
            _events = AI_Manager.Instance.Combat.Events;
            _events.Damaged += OnDamaged;
        }
    }

    private void OnDisable()
    {
        if (_events != null) _events.Damaged -= OnDamaged;
        _events = null;
    }

    private void OnDamaged(DamageResult r)
    {
        if (hits == null || !r.Target || r.Evaded) return;
        if (!_logic) _logic = GetComponent<LogicProcessor>();
        var owner = _logic && OwnerField != null ? OwnerField.GetValue(_logic) as StaticAICore : null;
        if (!owner || r.Attacker != owner || !_reacted.Add(r.Target)) return;

        // 데미지는 먼저 들어가도 반응은 그림에서 베는 순간에
        var to = (Vector2)(r.Target.transform.position - owner.transform.position);
        var push = to.sqrMagnitude > 0.0001f ? to.normalized : Vector2.right;
        var age = Time.time - _enabledAt;
        foreach (var h in hits)
            PixelFxHitReact.Play(r.Target.transform, push, Mathf.Max(0f, h.time - age), h.flash, h.freeze, h.push, flashMaterial);

        if (onHit == null || onHit.Length == 0) return;
        var wait = hits.Length > 0 ? Mathf.Max(0f, hits[0].time - age) : 0f;
        var dir = push.x < 0f ? -1f : 1f;
        foreach (var e in onHit)
        {
            if (!e.effect) continue;
            var at = e.fromOwner ? owner.transform : r.Target.transform;
            if (wait + e.delay <= 0f) Spawn(e, at.position, dir, 0f, null, owner);
            else StartCoroutine(SpawnAt(e, at, wait + e.delay, dir, owner));
        }
    }

    // 맞는 반응과 같이 나오게, 그때의 위치에서
    private static IEnumerator SpawnAt(Entry e, Transform at, float wait, float dir, Component owner)
    {
        if (wait > 0f) yield return new WaitForSeconds(wait);
        if (at) Spawn(e, at.position, dir, 0f, null, owner);
    }

    // 시전자는 생성 직후 Initialize에서 들어오므로 첫 LateUpdate에서 재생
    private void LateUpdate()
    {
        if (_played) return;
        _played = true;
        if (effects == null) return;

        if (!_logic) _logic = GetComponent<LogicProcessor>();
        var owner = _logic && OwnerField != null ? OwnerField.GetValue(_logic) as Component : null;
        var target = transform.position;
        var self = owner ? owner.transform.position : target;
        var victim = _logic && TargetField != null ? TargetField.GetValue(_logic) as Transform : null;

        // 상대 쪽으로 기울이되 옆으로 휘두르는 모션과 너무 어긋나지 않게 maxTilt까지만
        // 스킬은 시전자 자리에서 생기므로 방향은 상대 기준
        var to = (Vector2)((victim ? victim.position : target) - self);
        var dir = to.x < 0f ? -1f : 1f;
        var aim = to.sqrMagnitude > 0.0001f ? Mathf.Atan2(to.y, Mathf.Abs(to.x)) * Mathf.Rad2Deg : 0f;

        foreach (var e in effects)
        {
            if (!e.effect) continue;
            var origin = e.fromOwner ? self : target;
            var limit = e.maxTilt >= 0f ? e.maxTilt : maxTilt;
            var tilt = Mathf.Clamp(aim, -limit, limit);
            var dest = victim ? victim.position : target;
            if (e.delay > 0f) StartCoroutine(SpawnLater(e, origin, dir, tilt, dest, owner));
            else Spawn(e, origin, dir, tilt, dest, owner);
        }

        if (reactAll || hits == null || hits.Length == 0) return;
        var push = to.sqrMagnitude > 0.0001f ? to.normalized : new Vector2(dir, 0f);
        foreach (var h in hits)
            PixelFxHitReact.Play(victim, push, h.time, h.flash, h.freeze, h.push, flashMaterial);
    }

    private static IEnumerator SpawnLater(Entry e, Vector3 origin, float dir, float tilt, Vector3 dest, Component owner)
    {
        yield return new WaitForSeconds(e.delay);
        Spawn(e, origin, dir, tilt, dest, owner);
    }

    public static void Spawn(Entry e, Vector3 origin, float dir, float tilt, Vector3? dest = null, Component owner = null)
    {
        // 좌우 반전된 그림은 반대로 돌려야 같은 쪽으로 기울어짐
        var angle = tilt * dir;
        var pos = origin + Quaternion.Euler(0f, 0f, angle) * new Vector3(e.offset.x * dir, e.offset.y, 0f);
        var fx = PixelFxImpact.Spawn(e.effect, pos, e.scale, dir, e.sortingOrder, e.lifetime, e.fade, e.speed, angle);
        if (e.skip > 0f && fx.TryGetComponent<Animator>(out var anim)) anim.Update(e.skip / Mathf.Max(0.01f, e.speed));
        if (e.family) PixelFxFamilyColors.Apply(fx, owner);
        if (e.fly > 0f && dest.HasValue)
        {
            var fly = fx.AddComponent<Fly>();
            fly.from = pos;
            fly.to = dest.Value + new Vector3(0f, e.offset.y, 0f);
            fly.time = e.fly;
        }
    }

    // 시전자 앞에서 상대 쪽으로 날아감 (도착하면 그 자리에 머묾)
    public class Fly : MonoBehaviour
    {
        public Vector3 from, to;
        public float time;
        private float _t;

        private void Update()
        {
            _t += Time.deltaTime;
            transform.position = Vector3.Lerp(from, to, Mathf.Clamp01(_t / time));
        }
    }
}
