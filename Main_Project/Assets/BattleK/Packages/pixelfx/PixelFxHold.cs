using System.Reflection;
using BattleK.Scripts.AI;
using BattleK.Scripts.AI.Attack;
using BattleK.Scripts.AI.Skill.Base.Logic.ColliderLogics;
using BattleK.Scripts.AI.Skill.Base.Logic.LogicBase;
using UnityEngine;

// 기 모으기: hold초 동안 판정을 제자리에 묶고 아무도 안 맞게 했다가 내려찍는 순간 풀어줌
// 시전자 공격 모션도 치켜든 자세에서 멈췄다가 내려찍기에 맞춰 이어서 휘두름
public class PixelFxHold : MonoBehaviour, IProjectileVelocityModifier
{
    public float hold = 0.5f;
    public float pauseAt = 0.13f;      // 모션에서 칼을 치켜든 시점
    public float resumeBefore = 0.04f; // 내려찍기 이만큼 전에 모션을 이어감

    private static readonly FieldInfo OwnerField =
        typeof(LogicProcessor).GetField("_owner", BindingFlags.NonPublic | BindingFlags.Instance);
    private static readonly FieldInfo OffsetField =
        typeof(BoxLogicHandler).GetField("_offset", BindingFlags.NonPublic | BindingFlags.Instance);
    private static readonly Vector2 Away = new(0f, 10000f);

    private BoxLogicHandler _box;
    private LogicProcessor _logic;
    private Vector2 _offset;
    private float _enabledAt, _speed;
    private bool _held;
    private Animator _anim;
    private float _animSpeed;
    private bool _paused, _animDone;

    public int Priority => -1000;

    private void Awake()
    {
        _box = GetComponent<BoxLogicHandler>();
        _logic = GetComponent<LogicProcessor>();
        if (_box && OffsetField != null) _offset = (Vector2)OffsetField.GetValue(_box);
    }

    // StartProcess(바로 판정)보다 먼저 불림
    private void OnEnable()
    {
        _enabledAt = Time.time;
        _speed = 0f;
        _held = true;
        _anim = null;
        _paused = _animDone = false;
        if (_box && OffsetField != null) OffsetField.SetValue(_box, _offset + Away);
    }

    private void OnDisable()
    {
        Release();
        Resume();
    }

    private void Update()
    {
        var age = Time.time - _enabledAt;
        if (_held && age >= hold) Release();

        if (_animDone) return;
        if (!_anim)
        {
            var owner = _logic && OwnerField != null ? OwnerField.GetValue(_logic) as StaticAICore : null;
            if (owner) _anim = owner.GetComponentInChildren<Animator>();
            if (!_anim) return;
        }
        if (!_paused && age >= pauseAt && age < hold - resumeBefore)
        {
            _animSpeed = _anim.speed;
            _anim.speed = 0f;
            _paused = true;
        }
        else if (_paused && age >= hold - resumeBefore) Resume();
    }

    private void Release()
    {
        if (!_held) return;
        _held = false;
        if (_box && OffsetField != null) OffsetField.SetValue(_box, _offset);
    }

    private void Resume()
    {
        if (_paused && _anim) _anim.speed = _animSpeed;
        _paused = false;
        _animDone = true;
    }

    public void ModifyVelocity(ref Vector2 direction, ref float speed, float deltaTime, Transform self)
    {
        if (speed > 0f) _speed = speed;
        speed = _held ? 0f : _speed;
    }
}
