using BattleK.Scripts.AI;
using BattleK.Scripts.HP;
using BattleK.Scripts.Manager.Battle;

namespace BattleK.Scripts.Combat.Status
{
    public class TimedInvincibleEffect : IStatusEffect, ITaggedStatusEffect
    {
        private const string EffectKey = "Revive.Invincible";

        private readonly float _duration;
        private readonly StatusVisualType _visual;
        private float _endTime;

        public object Key => EffectKey;
        public StatusStackPolicy StackPolicy => StatusStackPolicy.Refresh;
        public StatusTags Tags => StatusTags.Buff;

        public TimedInvincibleEffect(StaticAICore target, float duration, StatusVisualType visual)
        {
            _duration = duration;
            _visual = visual;
        }

        public void OnApply(StatusEffectManager host, float now)
        {
            _endTime = now + _duration;
            host.AddFlags(UnitStatusFlags.Invincible);
            if (_visual != StatusVisualType.Normal) host.AddVisual(_visual);
        }

        public void Refresh(float now)
        {
            _endTime = now + _duration;
        }

        public bool Update(StatusEffectManager host, float now) => now >= _endTime;

        public void OnRemove(StatusEffectManager host)
        {
            host.RemoveFlags(UnitStatusFlags.Invincible);
            if (_visual != StatusVisualType.Normal) host.RemoveVisual(_visual);
        }
    }
}