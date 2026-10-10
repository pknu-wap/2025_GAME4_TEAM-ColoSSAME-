using System;
using BattleK.Scripts.AI.Skill.Base.Logic.LogicBase;
using Shop.Item.Runner.Battle.Core;
using UnityEngine;

namespace BattleK.Scripts.AI.Skill.Base.Logic.AttackSkillLogics
{
    [Serializable]
    public sealed class NormalAttackDamageLogic : ISkillLogic
    {
        [Min(0f)] public float DamageMultiplier = 1f;

        public void Execute(StaticAICore owner, StaticAICore target)
        {
            if (!owner || !target || owner == target || target.IsDead) return;
            if ((owner.TargetLayer.value & (1 << target.gameObject.layer)) == 0) return;

            var damage = Mathf.Max(1, Mathf.RoundToInt(owner.CurrentAttackDamage * Mathf.Max(0f, DamageMultiplier)));
            target.OnTakeDamage(damage, owner, false);
        }
    }
}
