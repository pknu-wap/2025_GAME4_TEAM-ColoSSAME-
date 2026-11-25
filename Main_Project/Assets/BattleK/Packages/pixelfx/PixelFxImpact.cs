using System.Reflection;
using BattleK.Scripts.AI.Skill.Base.Logic.LogicBase;
using UnityEngine;

// 투사체가 맞고 삭제될 때 타격 이펙트 생성
public class PixelFxImpact : MonoBehaviour
{
    public GameObject effect;
    public Transform point;
    public float scale = 0.5f;
    public float lifetime; // 0이면 이펙트 길이대로
    public float fade;     // 끝나기 전 흐려지는 시간
    public int sortingOrder = 60;
    public float depth = 0.4f;  // 닿은 지점에서 몸 안쪽으로 (월드)
    public float height = 0.4f; // 발밑에서 몸통 높이 (월드)

    [Header("맞는 반응 (PixelFxHitReact)")]
    public Material flashMaterial;
    public float hitFlash = 0.06f;
    public float hitFreeze = 0.08f;
    public float hitPush = 0.025f; // 날아온 방향으로 밀리는 거리 (월드)

    private const float VictimRange = 0.8f; // 사라진 자리에서 이 거리 안의 대상만 (다른 유닛에 막힌 경우 제외)

    private static readonly FieldInfo TargetField =
        typeof(LogicProcessor).GetField("_targetTransform", BindingFlags.NonPublic | BindingFlags.Instance);

    private static bool quitting;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void Hook()
    {
        quitting = false;
        Application.quitting -= OnQuit;
        Application.quitting += OnQuit;
    }

    private static void OnQuit() => quitting = true;

    private int disabledFrame = -1;

    private void OnDisable() => disabledFrame = Time.frameCount;

    // 맞으면 Destroy(비활성화 후 같은 프레임에 OnDestroy), 수명이 끝나면 풀로 비활성화만 됨
    private void OnDestroy()
    {
        if (quitting || !effect || disabledFrame != Time.frameCount || !gameObject.scene.isLoaded) return;

        // 날아가는 방향 (루트가 방향대로 돌고, 왼쪽이면 x 스케일 반전)
        var dir = transform.lossyScale.x < 0f ? -1f : 1f;
        var fwd = transform.right * dir;
        Vector3 pos;
        if (TryGetComponent<PixelFxDescend>(out var path) && path.Tracking)
        {
            // 그림 높이를 뺀 발밑 선 위치 기준 (대각선으로 날아가도 화면 위쪽으로 height)
            fwd = path.Direction;
            dir = fwd.x < 0f ? -1f : 1f;
            pos = path.Ground + fwd * depth + Vector3.up * height;
        }
        else
        {
            var head = point ? Vector3.Dot(point.position - transform.position, fwd) : 0f;
            pos = transform.position + fwd * (head + depth) + Vector3.up * height;
        }

        Spawn(effect, pos, scale, dir, sortingOrder, lifetime, fade);
        React(fwd);
    }

    private void React(Vector2 fwd)
    {
        if (!flashMaterial || !TryGetComponent<LogicProcessor>(out var logic) || TargetField == null) return;
        var victim = TargetField.GetValue(logic) as Transform;
        if (!victim || Vector2.Distance(victim.position, transform.position) > VictimRange) return;
        PixelFxHitReact.Play(victim, fwd, 0f, hitFlash, hitFreeze, hitPush, flashMaterial);
    }

    // 한 번 재생하는 이펙트 생성 (dir -1이면 좌우 반전, angle은 기울기, lifetime 0이면 이펙트 길이대로)
    public static GameObject Spawn(GameObject effect, Vector3 pos, float scale, float dir, int sortingOrder, float lifetime, float fade, float speed = 1f, float angle = 0f)
    {
        var fx = Instantiate(effect, pos, Quaternion.Euler(0f, 0f, angle));
        if (!Mathf.Approximately(speed, 1f) && fx.TryGetComponent<Animator>(out var anim)) anim.speed = speed;
        fx.transform.localScale = new Vector3(scale * dir, scale, 1f);
        foreach (var sr in fx.GetComponentsInChildren<SpriteRenderer>()) sr.sortingOrder = sortingOrder;

        // 반복 이펙트는 삭제 컴포넌트가 없으므로 수명을 주면 붙임
        if (!fx.TryGetComponent<PixelFxAutoDestroy>(out var auto) && lifetime > 0f) auto = fx.AddComponent<PixelFxAutoDestroy>();
        if (auto)
        {
            if (lifetime > 0f) auto.lifetime = lifetime;
            auto.fade = fade;
        }
        return fx;
    }
}
