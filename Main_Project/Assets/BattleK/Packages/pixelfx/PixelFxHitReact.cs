using System.Collections;
using UnityEngine;

// 근접 평타에 맞는 반응: 하얗게 번쩍, 맞은 쪽으로 살짝 밀렸다 돌아옴, 피격 모션 잠깐 멈춤
// 그림(Animator 오브젝트)만 움직이고 유닛 위치와 판정은 그대로. 맞는 유닛에 처음 맞을 때 붙음
public class PixelFxHitReact : MonoBehaviour
{
    private const float PushTime = 0.2f; // 밀렸다 돌아오는 시간
    private const float PushPeak = 0.2f; // 그중 밀리는 구간 비율

    private Animator _anim;
    private Transform _visual;
    private SpriteRenderer[] _renderers;
    private Material[] _saved;

    private bool _flashing, _frozen, _pushing;
    private float _flashEnd, _freezeEnd, _savedSpeed, _pushAge;
    private Vector3 _base, _push;

    public static void Play(Transform target, Vector2 dir, float delay, float flash, float freeze, float push, Material flashMaterial)
    {
        if (!target || !target.gameObject.activeInHierarchy) return;
        if (!target.TryGetComponent<PixelFxHitReact>(out var react)) react = target.gameObject.AddComponent<PixelFxHitReact>();
        react.StartCoroutine(react.Run(dir, delay, flash, freeze, push, flashMaterial));
    }

    private IEnumerator Run(Vector2 dir, float delay, float flash, float freeze, float push, Material flashMaterial)
    {
        if (delay > 0f) yield return new WaitForSeconds(delay);
        if (!Find()) yield break;

        if (flash > 0f && flashMaterial)
        {
            if (!_flashing)
            {
                _renderers = _visual.GetComponentsInChildren<SpriteRenderer>(true);
                _saved = new Material[_renderers.Length];
                for (var i = 0; i < _renderers.Length; i++)
                {
                    _saved[i] = _renderers[i].sharedMaterial;
                    _renderers[i].sharedMaterial = flashMaterial;
                }
                _flashing = true;
            }
            _flashEnd = Mathf.Max(_flashEnd, Time.time + flash);
        }

        if (freeze > 0f && _anim)
        {
            if (!_frozen)
            {
                _savedSpeed = _anim.speed;
                _anim.speed = 0f;
                _frozen = true;
            }
            _freezeEnd = Mathf.Max(_freezeEnd, Time.time + freeze);
        }

        if (push > 0f && dir.sqrMagnitude > 0.0001f)
        {
            if (!_pushing) _base = _visual.localPosition;
            var world = (Vector3)(dir.normalized * push);
            _push = _visual.parent ? _visual.parent.InverseTransformVector(world) : world;
            _pushAge = 0f;
            _pushing = true;
        }
    }

    private bool Find()
    {
        if (!_visual)
        {
            _anim = GetComponentInChildren<Animator>();
            _visual = _anim ? _anim.transform : null; // 체력바 같은 건 빼고 캐릭터 그림만
        }
        return _visual;
    }

    private void LateUpdate()
    {
        if (_flashing && Time.time >= _flashEnd) EndFlash();
        if (_frozen && Time.time >= _freezeEnd) EndFreeze();
        if (!_pushing) return;

        _pushAge += Time.deltaTime;
        var k = _pushAge / PushTime;
        if (k >= 1f)
        {
            EndPush();
            return;
        }
        var f = k < PushPeak ? k / PushPeak : Mathf.Pow(1f - (k - PushPeak) / (1f - PushPeak), 2f);
        _visual.localPosition = _base + _push * f;
    }

    // 죽거나 풀로 돌아갈 때 원래대로
    private void OnDisable()
    {
        StopAllCoroutines();
        EndFlash();
        EndFreeze();
        EndPush();
    }

    private void EndFlash()
    {
        if (!_flashing) return;
        for (var i = 0; i < _renderers.Length; i++)
            if (_renderers[i]) _renderers[i].sharedMaterial = _saved[i];
        _flashing = false;
    }

    private void EndFreeze()
    {
        if (!_frozen) return;
        if (_anim) _anim.speed = _savedSpeed;
        _frozen = false;
    }

    private void EndPush()
    {
        if (!_pushing) return;
        if (_visual) _visual.localPosition = _base;
        _pushing = false;
    }
}
