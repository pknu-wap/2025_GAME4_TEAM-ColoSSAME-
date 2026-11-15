using System;
using BattleK.Scripts.AI.Skill.Base.Logic.LogicBase;
using BattleK.Scripts.Combat.Damage;
using BattleK.Scripts.Combat.Status;
using BattleK.Scripts.HP;
using BattleK.Scripts.Manager.Battle;
using UnityEngine;

namespace BattleK.Scripts.AI.Skill.Base.Logic.AttackSkillLogics
{
    [Serializable]
    public class DealDotDamageLogic : ISkillLogic
    {
        [SerializeField] private DamageSpec _spec = new();

        [Header("Timing")]
        [Min(0.05f)] [SerializeField] private float _tickInterval = 1f;
        [Min(0f)] [SerializeField] private float _duration = 3f;

        [Header("Effect")]
        [SerializeField] private StatusVisualType _visualType = StatusVisualType.Poison;
        [SerializeField] private StatusStackPolicy _stackPolicy = StatusStackPolicy.Refresh;

        public void Execute(StaticAICore owner, StaticAICore target)
        {
            if (!target) return;
            if (!target.TryGetComponent(out StatusEffectManager host)) return;

            var amount = _spec.Evaluate(owner, target);
            if (amount <= 0) return;

            host.ApplyDot(
                key: (this, owner),
                stackPolicy: _stackPolicy,
                attacker: owner,
                amount: amount,
                type: _spec.Type,
                flags: _spec.Flags | DamageFlags.NoHitReaction,
                profile: _spec.GetProfileOrDefault(DotEffect.DefaultProfile),
                origin: this,
                interval: _tickInterval,
                duration: _duration,
                visual: _visualType);
        }
    }
}