using BattleK.Scripts.AI.Skill.Base.Logic.LogicBase;
using UnityEngine;

namespace BattleK.Scripts.AI.Skill.Base.Logic.ExecuteLogic
{
    public class TeleportToTargetProcessor : LogicProcessor
    {
        public override void StartProcess()
        {
            if (!_targetTransform || !_targetTransform.TryGetComponent<StaticAICore>(out var targetCore)) return;
            _owner.transform.position = _targetTransform.position;
            transform.position = _targetTransform.position;
            ApplyLogicsToTarget(targetCore);
        }
    }
}
