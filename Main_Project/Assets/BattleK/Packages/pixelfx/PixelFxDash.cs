using System.Collections;
using System.Reflection;
using BattleK.Scripts.AI;
using BattleK.Scripts.AI.Skill.Base.Logic.LogicBase;
using BattleK.Scripts.Combat.Damage;
using BattleK.Scripts.Manager;
using UnityEngine;

// 돌진 스킬: 출발 이펙트, 달리는 동안 잔상, 실제로 맞힌 순간 베기와 타격 이펙트 (pixelfx dash-strike 구성)
public class PixelFxDash : MonoBehaviour
{
    public PixelFxStrike.Entry[] start;  // 출발할 때 (시전자 발밑 기준)
    public PixelFxStrike.Entry[] onHit;  // 맞힌 순간 (fromOwner 끄면 맞은 상대 기준)
    public PixelFxStrike.Hit hit = new() { time = 0.06f, flash = 0.06f, freeze = 0.17f, push = 0.0425f };
    public Material flashMaterial;

    public Color ghostColor = new(0.55f, 0.8f, 1f, 0.55f);
    public float ghostInterval = 0.035f;
    public float ghostLife = 0.22f;
    public float maxTime = 0.6f; // 이 시간 안에 못 맞히면 끝

    // 가까이서 쓰면 바로 맞아서 베기가 휘두르기 전에 나옴 -> 시전 후 이 시간보다 먼저는 안 띄움
    // (1_Skill_Normal 0.17초부터 휘두르고, 베기 그림은 나온 뒤 0.06초에 벰)
    public float earliest = 0.12f;
    public float shortDash = 0.4f; // 이보다 덜 달렸으면 뒤쪽에 잔상을 몰아서 남겨 돌진한 느낌을 냄
    public int burstCount = 3;
    public float burstGap = 0.14f;

    private static readonly FieldInfo OwnerField =
        typeof(LogicProcessor).GetField("_owner", BindingFlags.NonPublic | BindingFlags.Instance);
    private static readonly FieldInfo TargetField =
        typeof(LogicProcessor).GetField("_targetTransform", BindingFlags.NonPublic | BindingFlags.Instance);

    private LogicProcessor _logic;
    private StaticAICore _owner;
    private CombatEvents _events;
    private Material _ghostMat;
    private bool _started, _hit, _moved;
    private float _age, _lastGhost, _enabledAt, _travel;
    private Vector3 _lastPos;

    private void OnEnable()
    {
        _started = _hit = _moved = false;
        _owner = null; // 풀에서 다시 나오면 시전자가 바뀔 수 있음
        _age = _travel = 0f;
        _enabledAt = Time.time;
        Subscribe(); // 이미 붙어 있어서 생기자마자 맞히는 경우도 받게 미리 구독
    }

    private void OnDisable() => Unsubscribe();

    private void OnDestroy()
    {
        if (_ghostMat) Destroy(_ghostMat);
    }

    private StaticAICore Owner()
    {
        if (!_logic) _logic = GetComponent<LogicProcessor>();
        return _logic && OwnerField != null ? OwnerField.GetValue(_logic) as StaticAICore : null;
    }

    private Transform Target() =>
        _logic && TargetField != null ? TargetField.GetValue(_logic) as Transform : null;

    private void LateUpdate()
    {
        if (_hit) return;
        if (!_started)
        {
            _owner = Owner();
            if (!_owner) return; // 시전자가 들어올 때까지 기다림
            _started = true;
            Subscribe();

            var dir = Facing();
            if (start != null)
                foreach (var e in start)
                    if (e.effect) PixelFxStrike.Spawn(e, _owner.transform.position, dir, 0f);

            _lastPos = _owner.transform.position;
            _lastGhost = -ghostInterval;
            return;
        }

        if (!_owner || _age > maxTime)
        {
            Unsubscribe();
            return;
        }

        _age += Time.deltaTime;
        var pos = _owner.transform.position;
        var step = (pos - _lastPos).magnitude;
        var moving = step > 0.001f;
        _travel += step;
        _lastPos = pos;

        if (moving)
        {
            _moved = true;
            if (_age - _lastGhost >= ghostInterval)
            {
                Ghost(Vector3.zero, ghostLife);
                _lastGhost = _age;
            }
        }
        else if (_moved)
        {
            // 데미지 소식이 안 와도 돌진이 멈춘 순간(= 판정 순간) 이펙트는 냄
            var t = Target();
            if (t) Play(t, true);
        }
    }

    private void OnDamaged(DamageResult r)
    {
        if (_hit || !r.Target) return;
        var owner = _owner ? _owner : Owner();
        if (!owner || r.Attacker != owner) return;
        _owner = owner;
        Play(r.Target.transform, !r.Evaded);
    }

    private void Play(Transform victim, bool react)
    {
        _hit = true;
        Unsubscribe();
        var wait = Mathf.Max(0f, earliest - (Time.time - _enabledAt));
        StartCoroutine(PlayRoutine(victim, react, wait));
    }

    private IEnumerator PlayRoutine(Transform victim, bool react, float wait)
    {
        if (wait > 0f) yield return new WaitForSeconds(wait);
        if (!_owner || !victim) yield break;

        var self = _owner.transform.position;
        var to = (Vector2)(victim.position - self);
        var dir = to.x < 0f ? -1f : 1f;

        // 거의 안 달렸으면 뒤쪽으로 잔상을 늘어놓음 (먼 것일수록 빨리 사라짐)
        if (_travel < shortDash)
            for (var i = 1; i <= burstCount; i++)
                Ghost(new Vector3(-dir * burstGap * i, 0f, 0f), ghostLife * (1.3f - 0.25f * i));

        if (react && hit != null)
        {
            var push = to.sqrMagnitude > 0.0001f ? to.normalized : new Vector2(dir, 0f);
            PixelFxHitReact.Play(victim, push, hit.time, hit.flash, hit.freeze, hit.push, flashMaterial);
        }

        if (onHit == null) yield break;
        foreach (var e in onHit)
            if (e.effect) StartCoroutine(SpawnLater(e, e.fromOwner ? self : victim.position, dir));
    }

    private static IEnumerator SpawnLater(PixelFxStrike.Entry e, Vector3 origin, float dir)
    {
        if (e.delay > 0f) yield return new WaitForSeconds(e.delay);
        PixelFxStrike.Spawn(e, origin, dir, 0f);
    }

    private void Subscribe()
    {
        if (_events != null) return;
        _events = AI_Manager.Instance ? AI_Manager.Instance.Combat?.Events : null;
        if (_events != null) _events.Damaged += OnDamaged;
    }

    private void Unsubscribe()
    {
        if (_events != null) _events.Damaged -= OnDamaged;
        _events = null;
    }

    // 돌진하는 쪽 (상대 기준)
    private float Facing()
    {
        var target = Target();
        return target && target.position.x < _owner.transform.position.x ? -1f : 1f;
    }

    // 지금 자세를 그대로 복사해서 한 가지 색으로 남기고 흐려지게
    private void Ghost(Vector3 offset, float life)
    {
        var anim = _owner.GetComponentInChildren<Animator>();
        if (!anim || !flashMaterial) return;
        if (!_ghostMat)
        {
            _ghostMat = new Material(flashMaterial);
            _ghostMat.SetColor("_FlashColor", ghostColor); // 진하기는 a로
        }

        var src = anim.transform;
        var holder = new GameObject("DashGhost");
        holder.SetActive(false); // 복사본 스크립트의 Awake가 돌지 않게
        holder.transform.SetPositionAndRotation(src.position + offset, src.rotation);
        var copy = Instantiate(src.gameObject, holder.transform);
        copy.transform.localPosition = Vector3.zero;
        copy.transform.localRotation = Quaternion.identity;
        copy.transform.localScale = src.lossyScale;

        // 스크립트는 지우고 그림만 남김. 정렬 그룹은 유닛보다 한 칸 뒤로
        foreach (var m in copy.GetComponentsInChildren<MonoBehaviour>(true)) DestroyImmediate(m);
        foreach (var b in copy.GetComponentsInChildren<Behaviour>(true))
            if (b is UnityEngine.Rendering.SortingGroup g) g.sortingOrder -= 1;
            else b.enabled = false;
        foreach (var rd in copy.GetComponentsInChildren<Renderer>(true))
            if (rd is not SpriteRenderer) rd.enabled = false;

        foreach (var sr in copy.GetComponentsInChildren<SpriteRenderer>(true)) sr.sharedMaterial = _ghostMat;

        var fade = holder.AddComponent<PixelFxAutoDestroy>();
        fade.lifetime = life;
        fade.fade = life;
        holder.SetActive(true);
    }
}
