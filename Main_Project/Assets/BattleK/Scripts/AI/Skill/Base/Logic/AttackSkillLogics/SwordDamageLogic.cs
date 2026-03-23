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
        public float DamageMultiplier = 2.0f;
        public int FlatBonusDamage = 50;

        [Header("CC Settings")]
        public StatusData HitCCData;

        public void Execute(StaticAICore owner, StaticAICore target)
        {
            if (!target || target == owner) return;
            // LayerMask에 포함된 레이어인지 비트마스크로 검사 (포함되지 않으면 리턴)
            if ((owner.TargetLayer.value & (1 << target.gameObject.layer)) == 0) return;
            
            var finalDamage = Mathf.RoundToInt(owner.Stat.AttackDamage * DamageMultiplier) + FlatBonusDamage;
            target.OnTakeDamage(finalDamage);

            if (HitCCData != null && target.TryGetComponent(out StatusEffectManager statManager))
            {
                statManager.ApplyCC(HitCCData);
            }

            UnityEngine.Debug.Log($"[SwordSkill] {target.name}에게 {finalDamage} 데미지 적용!");
        }
    }
}