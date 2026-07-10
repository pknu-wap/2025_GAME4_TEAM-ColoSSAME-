using BattleK.Scripts.AI.Skill.Base.Logic.LogicBase;
using BattleK.Scripts.Manager.Battle;
using UnityEngine;

namespace BattleK.Scripts.AI.Skill.Base.Logic.AttackSkillLogics
{
    [System.Serializable]
    public class DotDamageLogic : ISkillLogic
    {
        [Header("DOT Settings")]
        public bool UseMaxHpScaling = true;

        [Header("Fixed Damage")]
        public float BaseDamage;
        public float SkillPointRatio;

        [Header("MaxHP Scaling")]
        public float BaseRatio = 0.02f;
        public float HpSkillPointRatio = 0.001f;

        [Header("Timing")]
        public float TickInterval = 1.0f;
        public float Duration = 3.0f;

        public bool IsPenetrating;

        public void Execute(StaticAICore owner, StaticAICore target)
        {
            if (!target) return;

            float rawDamage;

            if (UseMaxHpScaling)
            {
                rawDamage = target.Stat.MaxHP *
                    (BaseRatio + HpSkillPointRatio * owner.Stat.SkillPoint);
            }
            else
            {
                //기본데미지
                rawDamage = BaseDamage +
                    (SkillPointRatio * owner.Stat.SkillPoint);
            }

            var statusManager = target.GetComponent<StatusEffectManager>();
            if (statusManager)
            {
                statusManager.ApplyDotDamage(this, rawDamage, IsPenetrating);
            }
        }
    }
}