using BattleK.Scripts.AI.CCState;
using BattleK.Scripts.AI.Skill.Base.Logic.LogicBase;
using BattleK.Scripts.Data.Type.AIDataType.CC;
using UnityEngine;

namespace BattleK.Scripts.AI.Skill.Base
{
    [System.Serializable]
    public class StatModifierAction : ICCAction
    {
        [Header("Status Settings")]
        public StatusType TargetStat;
        [Tooltip("기존 스탯 대비 증감 비율. 예: -0.5는 50% 감소, 0.3은 30% 증가")]
        public float MultiplierDelta = 0f;

        [Header("표시용 정보")]
        public StatSourceCategory Category = StatSourceCategory.Debuff;
        [Tooltip("스탯창에 표시될 이름. 예: '화염 저항 감소'")]
        public string Label = "효과";

        [Header("Visual Settings")]
        public bool IsHardCC;
        public PlayerState AnimName = PlayerState.DEBUFF;

        public void OnStart(StaticAICore target, StatusData data)
        {
            if (IsHardCC) target.EnterCCState(data.animName);
            target.SetStatMultiplier(TargetStat, this, Category, Label, MultiplierDelta);
        }

        public void OnTick(StaticAICore target, StatusData data) { }

        public void OnEnd(StaticAICore target, StatusData data)
        {
            if (IsHardCC) target.ExitCCState();
            target.RemoveStatMultiplier(TargetStat, this);
        }
    }
}