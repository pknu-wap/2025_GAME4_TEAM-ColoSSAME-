using BattleK.Scripts.AI;
using BattleK.Scripts.Combat.Damage;
using BattleK.Scripts.HP;
using BattleK.Scripts.Manager.Battle;
using UnityEngine;

namespace BattleK.Scripts.Combat.Status
{
    public class DotEffect : IStatusEffect
    {
        public static readonly IDamageSourceProfile DefaultProfile = new SimpleDamageSourceProfile("Skill.Dot", "Skill", DamageDelivery.Periodic, DamageFlags.NoHitReaction);
        private const float MinInterval = 0.05f;
        private const int MaxTicksPerFrame = 20;

        private readonly StaticAICore _attacker;
        private readonly StaticAICore _target;
        private readonly int _amount;
        private readonly DamageType _type;
        private readonly DamageFlags _flags;
        private readonly IDamageSourceProfile _profile;
        private readonly object _origin;
        private readonly float _interval;
        private readonly float _duration;
        private readonly StatusVisualType _visual;

        private float _endTime;
        private float _nextTickTime;

        public object Key { get; }
        public StatusStackPolicy StackPolicy { get; }

        public DotEffect(
            object key, StatusStackPolicy stackPolicy,
            StaticAICore attacker, StaticAICore target, int amount,
            DamageType type, DamageFlags flags,
            IDamageSourceProfile profile, object origin,
            float interval, float duration, StatusVisualType visual)
        {
            Key = key;
            StackPolicy = stackPolicy;
            _attacker = attacker;
            _target = target;
            _amount = amount;
            _type = type;
            _flags = flags;
            _profile = profile;
            _origin = origin;
            _interval = Mathf.Max(MinInterval, interval);
            _duration = Mathf.Max(0f, duration);
            _visual = visual;
        }

        public void OnApply(StatusEffectManager host, float now)
        {
            _endTime = now + _duration;
            _nextTickTime = now + _interval;
            if (_visual != StatusVisualType.Normal) host.AddVisual(_visual);
        }

        public void Refresh(float now)
        {
            _endTime = now + _duration;
        }

        public bool Update(StatusEffectManager host, float now)
        {
            var ticks = 0;
            while (now >= _nextTickTime && _nextTickTime <= _endTime + 0.0001f && ticks < MaxTicksPerFrame)
            {
                _target.ReceiveDamage(_profile.CreateRequest(_attacker, _target, _amount, _type, _flags, _origin));
                _nextTickTime += _interval;
                ticks++;
            }

            return now >= _endTime;
        }

        public void OnRemove(StatusEffectManager host)
        {
            if (_visual != StatusVisualType.Normal) host.RemoveVisual(_visual);
        }
    }
}