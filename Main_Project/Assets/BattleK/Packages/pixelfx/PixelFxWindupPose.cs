using BattleK.Scripts.AI;
using UnityEngine;

// 선딜 프리팹에 붙임: 시전자 공격 모션을 팔을 든 자세에서 멈췄다가, 선딜이 끝나는 순간 내뻗게 이어감
public class PixelFxWindupPose : MonoBehaviour
{
    public float hold = 1f;            // 선딜 길이 (SO WindupTime)
    public float pauseAt = 0.17f;      // 모션에서 팔을 든 시점 (0_Attack_Magic)
    public float resumeBefore = 0.08f; // 내뻗기까지 걸리는 시간만큼 먼저 이어감

    private float _enabledAt, _animSpeed;
    private Animator _anim;
    private bool _paused, _done;

    private void OnEnable()
    {
        _enabledAt = Time.time;
        _anim = null;
        _paused = _done = false;
    }

    private void OnDisable() => Resume();

    private void Update()
    {
        if (_done) return;
        var age = Time.time - _enabledAt;
        if (!_anim)
        {
            _anim = FindOwnerAnimator();
            if (!_anim) { _done = true; return; }
        }
        if (!_paused && age >= pauseAt && age < hold - resumeBefore)
        {
            _animSpeed = _anim.speed;
            _anim.speed = 0f;
            _paused = true;
        }
        else if (age >= hold - resumeBefore) Resume();
    }

    // 선딜 프리팹은 시전자 발밑에 생기므로 그 자리의 유닛
    private Animator FindOwnerAnimator()
    {
        StaticAICore best = null;
        var bestD = 0.1f;
        foreach (var ai in FindObjectsOfType<StaticAICore>())
        {
            var d = Vector2.Distance(ai.transform.position, transform.position);
            if (d < bestD) (best, bestD) = (ai, d);
        }
        return best ? best.GetComponentInChildren<Animator>() : null;
    }

    private void Resume()
    {
        if (_paused && _anim) _anim.speed = _animSpeed;
        _paused = false;
        _done = true;
    }
}
