using BattleK.Scripts.AI;
using BattleK.Scripts.AI.Skill.Base.Logic.LogicBase;
using Shop.Item.Runner.Battle.Core;
using UnityEngine;

namespace BattleK.Scripts.AI.Skill.Base.Logic.AttackSkillLogics
{
    public class SPMultipleDamage : ISkillLogic
    {
        [SerializeField] private float SkillPointRatio = 0f;
        [SerializeField] private float basicBonusPoint = 0f;
        public void Execute(StaticAICore owner, StaticAICore target)
        {
            if (!target || target == owner) return;
            if (target.gameObject.layer == owner.TargetLayer) return;
            var finalDamage = Mathf.RoundToInt( basicBonusPoint + SkillPointRatio * owner.runtimeStat.SkillPoint) * owner.CurrentAttackDamage;
            target.OnTakeDamage(finalDamage, owner, true);
            UnityEngine.Debug.Log($"[SPMultipleDamage] {target.name}에게 {finalDamage} 데미지 적용!");
        }
    }
}
