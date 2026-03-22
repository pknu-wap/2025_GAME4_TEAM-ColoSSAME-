using BattleK.Scripts.AI.Skill.Base.Logic.LogicBase;
using BattleK.Scripts.Manager.Battle;
using UnityEngine;

namespace BattleK.Scripts.AI.Skill.Base
{
    [System.Serializable]
    public class DotDamageLogic : ISkillLogic
    {
        [Header("DOT Settings")]
        public float BaseDamage;
        public float SkillPointRatio;
        public float TickInterval = 0.5f;
        public float Duration = 3.0f;
        public bool bIsPenetrating;
        
        public void Execute(StaticAICore owner, StaticAICore target)
        {
            if (!target) return;
            var calculatedDamagePerTick = BaseDamage + (SkillPointRatio * owner.Stat.SkillPoint);

            var statusManager = target.GetComponent<StatusEffectManager>();
            if (statusManager)
            {
                statusManager.ApplyDotDamage(this, calculatedDamagePerTick, bIsPenetrating);
            }
        }
    }
}