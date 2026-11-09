using System.Collections;
using BattleK.Scripts.AI.Skill.Base;
using UnityEngine;

namespace BattleK.Scripts.AI.StaticScoreState.ActionStates
{
    public class StaticAttackState : IStaticActionState
    {
        private readonly StaticAICore _ai;
        private readonly SkillSO _attackData;

        private bool _isAttacking;
        private Coroutine _runningRoutine;

        public int Priority => 100;

        public StaticAttackState(StaticAICore ai, SkillSO attackData)
        {
            _ai = ai;
            _attackData = attackData;
        }

        public bool CanExecute()
        {
            if (_isAttacking) return true;

            if (!_ai.Target || !_ai.Target.gameObject.activeInHierarchy) return false;
            if (!_ai.IsAttackReady) return false;

            var distSq = (_ai.Target.position - _ai.transform.position).sqrMagnitude;
            var range = _ai.runtimeStat.AttackRange + 0.1f;
            return distSq <= range * range;
        }

        public void Enter()
        {
            _isAttacking = true;
            _ai.StopMovement();
            _ai.LookAt(_ai.Target.position);
            _ai.PlayAnimation(PlayerState.ATTACK, _ai.AttackIndex);
        }

        public IEnumerator Execute()
        {
            yield return _attackData.ExecuteSkillRoutine(_ai, _ai.Target);

            _ai.SetAttackCooldown();
            _isAttacking = false;
            _ai.MainMachine.ChangeState(new StaticIdleState(_ai));
        }

        public void Exit()
        {
            _isAttacking = false;
        }
    }
}