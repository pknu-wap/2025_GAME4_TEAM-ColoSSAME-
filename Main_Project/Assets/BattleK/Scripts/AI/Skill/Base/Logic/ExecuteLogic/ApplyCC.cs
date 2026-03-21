using BattleK.Scripts.AI.Skill.Base.Logic.LogicBase;
using BattleK.Scripts.Data.Type.AIDataType.CC;
using BattleK.Scripts.Manager.Battle;
using UnityEngine;

namespace BattleK.Scripts.AI.Skill.Base.Logic.ExecuteLogic
{
    public class ApplyCC : ISkillLogic
    {
        [Header("CC 설정 (직접 입력)")]
        public StatusType StatusType;      // 수정할 스탯
        public float Multiplier = 0.5f;    // 배율
        public float Duration = 2.0f;      // 지속 시간
        public bool IsHardCC = false;      // 기절 등 애니메이션 동반 여부
        public PlayerState AnimState = PlayerState.DEBUFF;

        public void Execute(StaticAICore owner, StaticAICore target)
        {
            if (!target) return;
            var statusManager = target.GetComponent<StatusEffectManager>();
            if (statusManager)
            {
                statusManager.ApplyCustomCC(this, target);
            }
        }
    }
}
