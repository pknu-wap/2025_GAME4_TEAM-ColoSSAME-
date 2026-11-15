using System;
using BattleK.Scripts.AI;
using BattleK.Scripts.Combat.Damage.Formulas;

namespace BattleK.Scripts.Combat.Status
{
    public readonly struct StatusRequest
    {
        public readonly StaticAICore Source;
        public readonly StaticAICore Target;
        public readonly StatusSpec Spec;

        public StatusRequest(StaticAICore source, StaticAICore target, StatusSpec spec)
        {
            Source = source;
            Target = target;
            Spec = spec;
        }
    }

    public interface ITaggedStatusEffect
    {
        StatusTags Tags { get; }
    }

    public sealed class StatusService
    {
        public event Action<StatusRequest> Applied;

        public bool Apply(in StatusRequest request)
        {
            var target = request.Target;
            var spec = request.Spec;
            if (!target || spec == null || target.IsDead) return false;

            var manager = target.StatusEffects;
            if (!manager) return false;

            // 무적 대상은 Debuff 안 받음
            if (spec.Tags.HasFlag(StatusTags.Debuff) && target.IsInvincible) return false;

            var ctx = new DamageFormulaContext(request.Source, target);
            var values = new float[spec.Modifiers.Count];
            for (var i = 0; i < values.Length; i++)
                values[i] = spec.Modifiers[i].Delta?.Evaluate(ctx) ?? 0f;

            manager.ApplyEffect(new StatusEffect(spec, request.Source, values));
            Applied?.Invoke(request);
            return true;
        }
    }
}