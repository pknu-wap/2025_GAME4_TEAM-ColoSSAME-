using BattleK.Scripts.AI.Skill.Base.Logic.LogicBase;
using UnityEngine;

namespace BattleK.Scripts.AI.Skill.Base.Logic.AttackSkillLogics
{
    public class HealthPerHealLogic : ISkillLogic
    {
        [SerializeField] private float BaseHealRatio = 0.25f;
        [SerializeField] private float SkillPointBonusRatio = 0.01f;

        public void Execute(StaticAICore owner, StaticAICore target)
        {
            if (!target) return;
            if (target.gameObject.layer != owner.gameObject.layer) return;
            var healAmount = Mathf.RoundToInt(target.runtimeStat.MaxHP * (BaseHealRatio + SkillPointBonusRatio * owner.runtimeStat.SkillPoint));
            target.OnHeal(healAmount, owner);
        }
    }
}