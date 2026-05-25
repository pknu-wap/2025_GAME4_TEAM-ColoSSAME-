using BattleK.Scripts.AI.Skill.Base.Logic.LogicBase;
using UnityEngine;

namespace BattleK.Scripts.AI.Skill.Base.Logic.ExecuteLogic
{
    public class KnockbackProcessor : LogicProcessor
    {
        [SerializeField] private float KnockbackPower = 3f;

        public override void StartProcess()
        {
            // 타겟 없으면 종료
            if (!_targetTransform ||
                !_targetTransform.TryGetComponent<StaticAICore>(out var targetCore))
                return;

            // Rigidbody2D 가져오기
            if (!targetCore.TryGetComponent<Rigidbody2D>(out var rb))
                return;

            // 넉백 방향 계산
            Vector2 dir =
                (_targetTransform.position - _owner.transform.position).normalized;

            // 힘 적용
            rb.AddForce(dir * KnockbackPower, ForceMode2D.Impulse);

            // 데미지 로직 등 추가 적용
            ApplyLogicsToTarget(targetCore);
        }
    }
}