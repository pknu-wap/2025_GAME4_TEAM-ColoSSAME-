using System.Linq;
using BattleK.Scripts.AI;
using BattleK.Scripts.AI.CCState;
using BattleK.Scripts.Data.Type.AIDataType.CC;
using BattleK.Scripts.HP;
using BattleK.Scripts.Manager.Battle;

namespace BattleK.Scripts.Combat.Status
{
    public sealed class StatusEffect : IStatusEffect, ITaggedStatusEffect
    {
        private readonly StatusSpec _spec;
        private readonly StaticAICore _caster;
        private readonly float[] _values;
        private float _endTime;
        public StatusTags Tags => _spec.Tags;

        public StatusEffect(StatusSpec spec, StaticAICore caster, float[] values)
        {
            _spec = spec;
            _caster = caster;
            _values = values;
        }

        public object Key => _spec;
        public StatusStackPolicy StackPolicy => _spec.StackPolicy;

        public void OnApply(StatusEffectManager m, float now)
        {
            _endTime = now + _spec.Duration;

            var category = _spec.Tags.HasFlag(StatusTags.Buff) ? StatSourceCategory.Buff : StatSourceCategory.Debuff;
            for (var i = 0; i < _spec.Modifiers.Count; i++)
            {
                var mod = _spec.Modifiers[i];
                if (mod.Stat == StatusType.None) continue;
                m._aiCore.SetStatMultiplier(mod.Stat, this, category, _spec.DisplayName, _values[i]);
            }

            if (_spec.Tags.HasFlag(StatusTags.HardCC)) m.EnterHardCc(_spec.HardCcAnim);
            if (_spec.Visual != StatusVisualType.Normal) m.AddVisual(_spec.Visual);
            m.AddFlags(_spec.Flags);
        }

        public bool Update(StatusEffectManager m, float now) => now >= _endTime;

        public void Refresh(float now) => _endTime = now + _spec.Duration;

        public void OnRemove(StatusEffectManager m)
        {
            foreach (var mod in _spec.Modifiers.Where(mod => mod.Stat != StatusType.None))
            {
                m._aiCore.RemoveStatMultiplier(mod.Stat, this);
            }

            if (_spec.Tags.HasFlag(StatusTags.HardCC)) m.ExitHardCc();
            if (_spec.Visual != StatusVisualType.Normal) m.RemoveVisual(_spec.Visual);
            m.RemoveFlags(_spec.Flags);
        }
    }
}