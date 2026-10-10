using BattleK.Scripts.AI.Skill.Base.Logic.LogicBase;

namespace BattleK.Scripts.AI.Skill.Base.Logic.ExecuteLogic
{
    public sealed class NormalAttackTargetProcessor : LogicProcessor
    {
        public override void StartProcess()
        {
            if (!_owner || !_targetTransform || !_targetTransform.gameObject.activeInHierarchy) return;
            var target = _targetTransform.GetComponent<StaticAICore>();
            if (!target || target == _owner || target.IsDead) return;
            if ((_targetMask.value & (1 << target.gameObject.layer)) == 0) return;

            var range = _owner.runtimeStat.AttackRange + 0.1f;
            if ((target.transform.position - _owner.transform.position).sqrMagnitude > range * range) return;
            ApplyLogicsToTarget(target);
        }
    }
}
