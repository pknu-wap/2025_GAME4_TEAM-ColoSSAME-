using BattleK.Scripts.AI.Skill.Base.Logic.LogicBase;
using BattleK.Scripts.Data.Type.AIDataType.CC;

namespace BattleK.Scripts.AI.Skill.Base
{
    [System.Serializable]
    public class HardCCAction : ICCAction
    {
        public void OnStart(StaticAICore target, CCData data)
        {
            target.EnterCCState(data.animName);
        }

        public void OnTick(StaticAICore target, CCData data) { }

        public void OnEnd(StaticAICore target, CCData data)
        {
            target.ExitCCState();
        }
    }
}