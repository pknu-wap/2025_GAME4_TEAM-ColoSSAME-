using BattleK.Scripts.AI.Skill.Base.Logic.LogicBase;

namespace BattleK.Scripts.AI.Skill.Base.Logic.ExecuteLogic
{
    public class InstantTargetProcessor : LogicProcessor
    {
        public override void StartProcess()
        {
            if (_targetTransform.TryGetComponent<StaticAICore>(out var targetCore))
            {
                ApplyLogicsToTarget(targetCore);
            }
        }
    }
}
