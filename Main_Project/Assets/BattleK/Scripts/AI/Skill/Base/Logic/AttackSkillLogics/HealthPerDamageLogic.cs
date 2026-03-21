using BattleK.Scripts.AI.Skill.Base.Logic.LogicBase;
using UnityEngine;

namespace BattleK.Scripts.AI.Skill.Base.Logic.AttackSkillLogics
{
    public class HealthPerDamageLogic : ISkillLogic
    {
        [SerializeField] private float BaseDamageRatio = 0f;
        [SerializeField] private float SkillPointBonusRatio = 0f;
        public void Execute(StaticAICore owner, StaticAICore target)
        {
            if (!target || target == owner) return;
            if (target.gameObject.layer == owner.TargetLayer) return;
            var finalDamage = Mathf.RoundToInt(target.Stat.MaxHP * BaseDamageRatio + SkillPointBonusRatio * owner.Stat.SkillPoint);
            target.OnTakeDamage(finalDamage, true);
            UnityEngine.Debug.Log($"[HealthPerDamage] {target.name}에게 {finalDamage} 데미지 적용!");
        }
    }
}
