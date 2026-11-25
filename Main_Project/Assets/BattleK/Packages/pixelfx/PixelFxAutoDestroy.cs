using UnityEngine;

// 한 번 재생하는 pixelfx 이펙트는 재생이 끝나면 삭제
public class PixelFxAutoDestroy : MonoBehaviour
{
    public float lifetime = 1f;
    public float fade; // 끝나기 전 흐려지는 시간 (0이면 안 흐려짐)

    private float elapsed;
    private SpriteRenderer[] renderers;

    private void Start()
    {
        renderers = GetComponentsInChildren<SpriteRenderer>();
        Destroy(gameObject, lifetime);
    }

    private void Update()
    {
        if (fade <= 0f) return;
        elapsed += Time.deltaTime;
        var a = Mathf.Clamp01((lifetime - elapsed) / fade);
        foreach (var sr in renderers)
        {
            var c = sr.color;
            c.a = a;
            sr.color = c;
        }
    }
}
