using BattleK.Scripts.AI.Skill.Base.Logic.LogicBase;
using BattleK.Scripts.Data.Type.AIDataType.CC;
using BattleK.Scripts.Manager.Battle;
using UnityEngine;

namespace BattleK.Scripts.AI.Skill.Base.Logic.ExecuteLogic
{
    public class ApplyCC : ISkillLogic
    {
        [Header("CC 설정 (직접 입력)")]
        public StatusType StatusType;

        [SerializeField] private float BasicStatMultiplier;
        [SerializeField] private float SkillPointStatMultiplier;
        public float Duration = 2.0f;
        public bool IsHardCC = false;
        public PlayerState AnimState = PlayerState.DEBUFF;

        public void Execute(StaticAICore owner, StaticAICore target)
        {
            if (!target) return;
            var calculatedMultiplier = BasicStatMultiplier + SkillPointStatMultiplier * owner.Stat.SkillPoint;
            var statusManager = target.GetComponent<StatusEffectManager>();
            if (statusManager)
            {
                statusManager.ApplyCustomCC(this, target, calculatedMultiplier);
            }
        }
    }
}
