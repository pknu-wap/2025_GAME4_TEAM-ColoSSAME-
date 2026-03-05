using BattleK.Scripts.Data.Type.AIDataType.CC;

namespace BattleK.Scripts.AI.Skill.Base.Logic.LogicBase
{
    public interface ISkillLogic
    {
        void Execute(StaticAICore owner, StaticAICore target);
    }
    
    public interface ICCAction
    {
        void OnStart(StaticAICore target, StatusData data);
        void OnTick(StaticAICore target, StatusData data);
        void OnEnd(StaticAICore target, StatusData data);
    }
}
