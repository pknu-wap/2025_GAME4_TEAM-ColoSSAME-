using UnityEngine;

// 한 번 재생하는 pixelfx 이펙트는 재생이 끝나면 삭제
public class PixelFxAutoDestroy : MonoBehaviour
{
    public float lifetime = 1f;

    private void Start() => Destroy(gameObject, lifetime);
}
