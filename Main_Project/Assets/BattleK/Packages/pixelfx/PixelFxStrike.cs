using System.Collections;
using System.Reflection;
using BattleK.Scripts.AI.Skill.Base.Logic.LogicBase;
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
    public Hit[] hits;
    public Material flashMaterial;
    public float maxTilt = 45f; // 대각선 상대 쪽으로 기울이는 최대 각도

    private static readonly FieldInfo OwnerField =
        typeof(LogicProcessor).GetField("_owner", BindingFlags.NonPublic | BindingFlags.Instance);
    private static readonly FieldInfo TargetField =
        typeof(LogicProcessor).GetField("_targetTransform", BindingFlags.NonPublic | BindingFlags.Instance);

    private LogicProcessor _logic;
    private bool _played;

    private void OnEnable() => _played = false; // 풀에서 다시 나올 때

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

        // 상대 쪽으로 기울이되 옆으로 휘두르는 모션과 너무 어긋나지 않게 maxTilt까지만
        var to = (Vector2)(target - self);
        var dir = to.x < 0f ? -1f : 1f;
        var aim = to.sqrMagnitude > 0.0001f ? Mathf.Atan2(to.y, Mathf.Abs(to.x)) * Mathf.Rad2Deg : 0f;

        foreach (var e in effects)
        {
            if (!e.effect) continue;
            var origin = e.fromOwner ? self : target;
            var limit = e.maxTilt >= 0f ? e.maxTilt : maxTilt;
            var tilt = Mathf.Clamp(aim, -limit, limit);
            if (e.delay > 0f) StartCoroutine(SpawnLater(e, origin, dir, tilt));
            else Spawn(e, origin, dir, tilt);
        }

        if (hits == null || hits.Length == 0) return;
        var victim = _logic && TargetField != null ? TargetField.GetValue(_logic) as Transform : null;
        var push = to.sqrMagnitude > 0.0001f ? to.normalized : new Vector2(dir, 0f);
        foreach (var h in hits)
            PixelFxHitReact.Play(victim, push, h.time, h.flash, h.freeze, h.push, flashMaterial);
    }

    private static IEnumerator SpawnLater(Entry e, Vector3 origin, float dir, float tilt)
    {
        yield return new WaitForSeconds(e.delay);
        Spawn(e, origin, dir, tilt);
    }

    private static void Spawn(Entry e, Vector3 origin, float dir, float tilt)
    {
        // 좌우 반전된 그림은 반대로 돌려야 같은 쪽으로 기울어짐
        var angle = tilt * dir;
        var pos = origin + Quaternion.Euler(0f, 0f, angle) * new Vector3(e.offset.x * dir, e.offset.y, 0f);
        PixelFxImpact.Spawn(e.effect, pos, e.scale, dir, e.sortingOrder, e.lifetime, e.fade, e.speed, angle);
    }
}
