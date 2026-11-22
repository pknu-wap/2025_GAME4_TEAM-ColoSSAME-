using System.Collections.Generic;
using System.Reflection;
using BattleK.Scripts.AI;
using BattleK.Scripts.AI.Skill.Base.Logic.LogicBase;
using UnityEngine;

// 지속 피해 동안 맞은 상대 몸에 붙어 도는 이펙트 (불붙음 등). 같은 상대에 다시 걸리면 시간만 새로 잼
public class PixelFxStatus : MonoBehaviour
{
    public GameObject effect; // 반복 재생하는 이펙트
    public float duration = 3f;
    public float scale = 0.7f;
    public Vector2 offset;
    public float fade = 0.3f;
    public int sortingOrder = 61;

    private static readonly FieldInfo TargetField =
        typeof(LogicProcessor).GetField("_targetTransform", BindingFlags.NonPublic | BindingFlags.Instance);
    private static readonly Dictionary<Transform, Follow> Active = new();

    private bool _played;

    private void OnEnable() => _played = false; // 풀에서 다시 나올 때

    // 대상은 생성 직후 Initialize에서 들어오므로 첫 LateUpdate에서
    private void LateUpdate()
    {
        if (_played) return;
        _played = true;
        if (!effect || TargetField == null || !TryGetComponent<LogicProcessor>(out var logic)) return;
        var victim = TargetField.GetValue(logic) as Transform;
        if (!victim) return;

        if (Active.TryGetValue(victim, out var on) && on)
        {
            on.until = Time.time + duration;
            return;
        }

        var fx = PixelFxImpact.Spawn(effect, victim.position + (Vector3)offset, scale, 1f, sortingOrder, 0f, 0f);
        var follow = fx.AddComponent<Follow>();
        follow.target = victim;
        follow.core = victim.GetComponent<StaticAICore>();
        follow.offset = offset;
        follow.fade = fade;
        follow.until = Time.time + duration;
        Active[victim] = follow;
    }

    public class Follow : MonoBehaviour
    {
        public Transform target;
        public StaticAICore core;
        public Vector2 offset;
        public float fade, until;

        private SpriteRenderer[] _renderers;
        private float _endAt = -1f;

        private void Awake() => _renderers = GetComponentsInChildren<SpriteRenderer>();

        private void LateUpdate()
        {
            // 상대가 죽거나 사라지면 바로 꺼지기 시작
            var gone = !target || !target.gameObject.activeInHierarchy || (core && core.IsDead);
            if (!gone) transform.position = target.position + (Vector3)offset;
            if (_endAt < 0f && (gone || Time.time >= until)) _endAt = Time.time + fade;
            if (_endAt < 0f) return;

            var a = fade > 0f ? Mathf.Clamp01((_endAt - Time.time) / fade) : 0f;
            foreach (var sr in _renderers)
            {
                var c = sr.color;
                c.a = a;
                sr.color = c;
            }
            if (a <= 0f) Destroy(gameObject);
        }

        private void OnDestroy()
        {
            if (target && Active.TryGetValue(target, out var on) && on == this) Active.Remove(target);
        }
    }
}
