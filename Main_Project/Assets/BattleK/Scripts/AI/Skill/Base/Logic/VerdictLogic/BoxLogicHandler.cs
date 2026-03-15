using BattleK.Scripts.AI.Skill.Base.Logic.LogicBase;
using UnityEngine;

namespace BattleK.Scripts.AI.Skill.Base.Logic.VerdictLogic
{
    public class BoxLogicHandler : LogicProcessor 
    {
        [SerializeField] private Vector2 _areaSize;
        [SerializeField] private bool _isContinuous;

        public override void StartProcess()
        {
            var targets = Physics2D.OverlapBoxAll(
                transform.position, 
                _areaSize, 
                transform.eulerAngles.z,
                _owner.TargetLayer
            );

            foreach (var col in targets)
            {
                if (!col.TryGetComponent(out StaticAICore target)) continue;
                if (target == _owner) continue;
                ApplyLogicsToTarget(target);
            }
        }
        
        private void Update()
        {
            if (_isContinuous)
            {
                StartProcess();
            }
        }
    }
}