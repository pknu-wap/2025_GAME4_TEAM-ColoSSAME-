using System;
using BattleK.Scripts.AI.Skill.Base.Logic.LogicBase;
using BattleK.Scripts.Data.Type.AIDataType.CC;
using BattleK.Scripts.Manager.Battle;
using UnityEngine;

namespace BattleK.Scripts.AI.Skill.Base.Logic.AttackSkillLogics
{
    [Serializable]
    public class AxeDamageLogic : ISkillLogic
    {
        [Header("Damage Settings")]
        public float DamageMultiplier = 2.0f;
        public int FlatBonusDamage = 50;

        [Header("CC Settings")]
        public StatusData HitCCData;

        public void Execute(StaticAICore owner, StaticAICore target)
        {
            if (!target || target == owner) return;
            if (target.gameObject.layer == owner.TargetLayer) return;
            
            var finalDamage = Mathf.RoundToInt(owner.Stat.AttackDamage * DamageMultiplier) + FlatBonusDamage;
            target.OnTakeDamage(finalDamage);

            if (HitCCData != null && target.TryGetComponent(out StatusEffectManager statManager))
            {
                statManager.ApplyCC(HitCCData);
            }

            UnityEngine.Debug.Log($"[AxeSkill] {target.name}에게 {finalDamage} 데미지 적용!");
        }
    }
}