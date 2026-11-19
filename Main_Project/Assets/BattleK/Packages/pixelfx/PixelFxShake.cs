using System.Collections.Generic;
using UnityEngine;

// 화면 흔들림: 스킬 프리팹에 붙이면 나온 순간부터 time초 뒤에 카메라를 흔듦
public class PixelFxShake : MonoBehaviour
{
    [System.Serializable]
    public class Shake
    {
        public float time;
        public float strength = 0.15f; // 월드 거리
        public float duration = 0.3f;
    }

    public Shake[] shakes;

    private void OnEnable() // 풀에서 다시 나올 때마다
    {
        if (shakes == null) return;
        foreach (var s in shakes) PixelFxCameraShake.Play(s.strength, s.duration, s.time);
    }
}

// 카메라 위치에 흔들림만큼 더했다가 다음 프레임에 빼서 원래 카메라 위치는 건드리지 않음
[DefaultExecutionOrder(-1000)]
public class PixelFxCameraShake : MonoBehaviour
{
    private class Item { public float start, strength, duration, seed; }

    private static PixelFxCameraShake _inst;
    private readonly List<Item> _items = new();
    private Transform _cam;
    private Vector3 _applied;

    public static void Play(float strength, float duration, float delay = 0f)
    {
        if (strength <= 0f || duration <= 0f) return;
        if (!_inst)
        {
            var go = new GameObject("PixelFxCameraShake");
            DontDestroyOnLoad(go);
            _inst = go.AddComponent<PixelFxCameraShake>();
        }
        _inst._items.Add(new Item { start = Time.time + delay, strength = strength, duration = duration, seed = Random.value * 100f });
    }

    private void Update()
    {
        if (_cam) _cam.position -= _applied;
        _applied = Vector3.zero;
    }

    private void LateUpdate()
    {
        var offset = Vector2.zero;
        for (var i = _items.Count - 1; i >= 0; i--)
        {
            var it = _items[i];
            var age = Time.time - it.start;
            if (age < 0f) continue;
            if (age >= it.duration) { _items.RemoveAt(i); continue; }
            var k = 1f - age / it.duration;
            var n = age * 35f;
            offset += new Vector2(Mathf.PerlinNoise(it.seed, n) - 0.5f, Mathf.PerlinNoise(n, it.seed) - 0.5f) * (2f * it.strength * k * k);
        }

        var cam = Camera.main;
        _cam = cam ? cam.transform : null;
        if (!_cam || offset == Vector2.zero) return;
        _applied = offset;
        _cam.position += _applied;
    }

    private void OnDisable()
    {
        if (_cam) _cam.position -= _applied;
        _applied = Vector3.zero;
    }
}
