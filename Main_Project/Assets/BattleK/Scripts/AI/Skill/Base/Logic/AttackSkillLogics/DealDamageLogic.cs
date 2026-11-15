using System;
using BattleK.Scripts.AI.Skill.Base.Logic.LogicBase;
using BattleK.Scripts.Combat.Damage;
using UnityEngine;

namespace BattleK.Scripts.AI.Skill.Base.Logic.AttackSkillLogics
{
    [Serializable]
    public class DealDamageLogic : ISkillLogic
    {
        [SerializeField] private DamageSpec _spec = new();

        [Tooltip("기존 로직과 같은 안전장치. 대상 필터링이 확실히 보장되는 스킬에서만 끈다.")]
        [SerializeField] private bool _checkTargetLayer = true;

        public void Execute(StaticAICore owner, StaticAICore target)
        {
            if (!target || target == owner) return;
            if (_checkTargetLayer && (owner.TargetLayer.value & (1 << target.gameObject.layer)) == 0) return;

            target.ReceiveDamage(_spec.CreateRequest(owner, target, origin: this));
        }
    }
}