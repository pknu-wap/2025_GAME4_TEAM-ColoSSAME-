using BattleK.Scripts.AI.Skill.Base.Logic.LogicBase;
using BattleK.Scripts.Data.Type.AIDataType.CC;
using BattleK.Scripts.HP;
using BattleK.Scripts.Manager.Battle;
using UnityEngine;

namespace BattleK.Scripts.AI.Skill.Base.Logic.ExecuteLogic
{
    public class ApplyCC : ISkillLogic
    {
        [Header("표시용 이름")]
        [Tooltip("스탯창 등 UI에 표시할 이름. 비어있으면 StatusType으로 대체 표시.")]
        [SerializeField] private string displayName;

        public string DisplayName => string.IsNullOrEmpty(displayName) ? StatusType.ToString() : displayName;

        [Header("CC 설정 (직접 입력)")]
        public StatusType StatusType;

        [Tooltip("SkillPoint와 무관하게 항상 적용되는 기본 증감분. 예: -0.5는 50% 감소")]
        [SerializeField] private float BasicStatDelta;
        [Tooltip("SkillPoint 1당 추가되는 증감분")]
        [SerializeField] private float SkillPointStatDelta;
        public float Duration = 2.0f;
        public bool IsHardCC = false;
        public PlayerState AnimState = PlayerState.DEBUFF;

        [Header("Visual")]
        public StatusVisualType VisualType = StatusVisualType.Normal;

        public void Execute(StaticAICore owner, StaticAICore target)
        {
            if (!target) return;
            var calculatedDelta = BasicStatDelta + SkillPointStatDelta * owner.runtimeStat.SkillPoint;
            var statusManager = target.GetComponent<StatusEffectManager>();
            if (statusManager)
            {
                statusManager.ApplyCustomCC(this, target, calculatedDelta);
            }
        }
    }
}