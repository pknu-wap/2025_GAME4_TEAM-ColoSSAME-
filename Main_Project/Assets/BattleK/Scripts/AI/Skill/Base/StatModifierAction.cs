using BattleK.Scripts.AI.Skill.Base.Logic.LogicBase;
using BattleK.Scripts.Data.Type.AIDataType.CC;
using UnityEngine;

namespace BattleK.Scripts.AI.Skill.Base
{
    [System.Serializable]
    public class StatModifierAction : ICCAction
    {
        public void OnStart(StaticAICore target, StatusData data)
        {
            target.EnterCCState(data.animName);

            if (!Mathf.Approximately(data.speedMultiplier, 1.0f))
            {
                target.SetMoveSpeedMultiplier(data.speedMultiplier);
            }
        }

        public void OnTick(StaticAICore target, StatusData data) { }

        public void OnEnd(StaticAICore target, StatusData data)
        {
            target.ExitCCState();
            target.ResetMoveSpeed();
        }
    }
}