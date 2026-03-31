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
        public float Multiplier = 1.0f;

        [Header("Visual Settings")]
        public bool IsHardCC;
        public PlayerState AnimName = PlayerState.DEBUFF;

        public void OnStart(StaticAICore target, StatusData data) 
        {
            if (IsHardCC) target.EnterCCState(data.animName);
            target.SetStatMultiplier(TargetStat, this, Multiplier);
        }

        public void OnTick(StaticAICore target, StatusData data) { }

        public void OnEnd(StaticAICore target, StatusData data)
        {
            if (IsHardCC) target.ExitCCState();
            target.RemoveStatMultiplier(TargetStat, this);
        }
    }
}