using BattleK.Scripts.AI.Skill.Base.Logic.LogicBase;
using UnityEngine;

namespace BattleK.Scripts.AI.Skill.Base.Logic.ExecuteLogic
{
    public class BackStepProcessor : LogicProcessor
    {
        [SerializeField] private float BackDistance = 10f;

        public override void StartProcess()
        {
            if (!_targetTransform)
                return;

            // 적 반대 방향 계산
            Vector2 dir =
                (_owner.transform.position -
                 _targetTransform.position).normalized;

            // 뒤로 이동
            _owner.transform.position +=
                (Vector3)(dir * BackDistance);

            // 공격 로직 실행
            if (_targetTransform.TryGetComponent(
                    out StaticAICore targetCore))
            {
                ApplyLogicsToTarget(targetCore);
            }
        }
    }
}