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
        var head = point ? Vector3.Dot(point.position - transform.position, fwd) : 0f;
        var pos = transform.position + fwd * (head + depth) + Vector3.up * height;

        var fx = Instantiate(effect, pos, Quaternion.identity);
        fx.transform.localScale = new Vector3(scale * dir, scale, 1f);
        foreach (var sr in fx.GetComponentsInChildren<SpriteRenderer>()) sr.sortingOrder = sortingOrder;

        if (fx.TryGetComponent<PixelFxAutoDestroy>(out var auto))
        {
            if (lifetime > 0f) auto.lifetime = lifetime;
            auto.fade = fade;
        }
    }
}
