using BattleK.Scripts.AI.Skill.Base.Logic.LogicBase;
using UnityEngine;

namespace BattleK.Scripts.AI.Skill.Base.Logic.HealSkillLogics
{
    public class HealthPerHealLogic : ISkillLogic
    {
        [SerializeField] private float BaseHealRatio = 0.25f;
        [SerializeField] private float SkillPointBonusRatio = 0.01f;

        public void Execute(StaticAICore owner, StaticAICore target)
        {
            if (!target) return;

            if (target.gameObject.layer != owner.gameObject.layer)
                return;

            int healAmount = Mathf.RoundToInt(
                target.Stat.MaxHP * (BaseHealRatio + SkillPointBonusRatio * owner.Stat.SkillPoint)
            );

            target.OnHeal(healAmount);
            UnityEngine.Debug.Log($"[Heal] {target.name}에게 {healAmount} 회복!");
        }
    }
}