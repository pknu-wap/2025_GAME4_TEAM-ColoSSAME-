using BattleK.Scripts.AI.Skill.Base.Logic.LogicBase;
using BattleK.Scripts.Data.Type.AIDataType.CC;
using BattleK.Scripts.Manager.Battle;
using UnityEngine;

namespace BattleK.Scripts.AI.Skill.Base
{
    [System.Serializable]
    public class DotDamageLogic : ISkillLogic
    {
        [Header("DOT Settings")]
        public float DamagePerTick = 10f;
        public float TickInterval = 0.5f;
        public float Duration = 3.0f;
        
        public void Execute(StaticAICore owner, StaticAICore target)
        {
            if (!target) return;

            // 타겟의 매니저에게 "이 도트 데미지 로직을 돌려줘"라고 요청
            var statusManager = target.GetComponent<StatusEffectManager>();
            if (statusManager)
            {
                statusManager.ApplyDotDamage(this);
            }
        }
    }
}