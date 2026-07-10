using System;
using BattleK.Scripts.AI;
using BattleK.Scripts.AI.Skill.Base.Logic.LogicBase;
using BattleK.Scripts.Data.Type.AIDataType.CC;
using BattleK.Scripts.Manager.Battle;
using UnityEngine;

namespace BattleK.Scripts.AI.Skill.Base.Logic.AttackSkillLogics
{
    [Serializable]
    public class SwordDamageLogic : ISkillLogic
    {
        [Header("Damage Settings")]
        [SerializeField] private float _damageMultiplier = 2.0f;
        [SerializeField] private int _flatBonusDamage = 50;

        [Header("CC Settings")]
        [SerializeField] private StatusData _hitCCData;

        public void Execute(StaticAICore owner, StaticAICore target)
        {
            if (!target || target == owner) return;

            if ((owner.TargetLayer.value & (1 << target.gameObject.layer)) == 0) return;

            int finalDamage = Mathf.RoundToInt(owner.Stat.AttackDamage * _damageMultiplier) + _flatBonusDamage;
            target.OnTakeDamage(finalDamage);

            if (_hitCCData != null && target.TryGetComponent(out StatusEffectManager statusEffectManager))
            {
                // statusEffectManager.ApplyCC(_hitCCData);
            }

            UnityEngine.Debug.Log($"[SwordSkill] {target.name}에게 {finalDamage} 데미지 적용!");
        }
    }
}