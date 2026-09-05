using BattleK.Scripts.AI.Skill.Base.Logic.LogicBase;
using BattleK.Scripts.HP;
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

        [Header("Visual")]
        public StatusVisualType VisualType = StatusVisualType.Poison;

        public void Execute(StaticAICore owner, StaticAICore target)
        {
            if (!target) return;
            var rawDamage = UseMaxHpScaling
                ? target.runtimeStat.MaxHP * (BaseRatio + HpSkillPointRatio * owner.runtimeStat.SkillPoint)
                : BaseDamage + (SkillPointRatio * owner.runtimeStat.SkillPoint);

            var statusManager = target.GetComponent<StatusEffectManager>();
            if (statusManager)
            {
                statusManager.ApplyDotDamage(this, owner, rawDamage, IsPenetrating);
            }
        }
    }
}