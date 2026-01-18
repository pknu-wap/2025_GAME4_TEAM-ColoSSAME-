using System.Collections;
using UnityEngine;

namespace BattleK.Scripts.AI.StaticScoreState.ActionStates
{
    public class StaticAttackState : IStaticActionState
    {
        private readonly StaticAICore _ai;
        private readonly float _windupTime;
        private readonly float _activeTime;
        private readonly float _recoveryTime;

        private bool _isAttacking;
        
        public int Priority => 100;
        
        public StaticAttackState(StaticAICore ai, float windupTime, float activeTime, float recoveryTime)
        {
            _ai = ai;
            _windupTime = windupTime;
            _activeTime = activeTime;
            _recoveryTime = recoveryTime;
        }

        public bool CanExecute()
        {
            if (_isAttacking) return true;
            
            if (!_ai.Target || !_ai.Target.gameObject.activeInHierarchy) return false;
            if (!_ai.IsAttackReady) return false;
            
            var distSq = (_ai.Target.position - _ai.transform.position).sqrMagnitude;
            var range = _ai.Stat.AttackRange + 0.1f;
            return distSq <= range * range;
        }
        
        public void Enter()
        {
            _isAttacking = true;
            _ai.StopMovement();
            _ai.LookAt(_ai.Target.position);
            _ai.PlayAnimation(PlayerState.ATTACK);
        }

        public IEnumerator Execute()
        {
            yield return new WaitForSeconds(_windupTime);
            _ai.EnableWeapon();
            yield return new WaitForSeconds(_activeTime);
            _ai.DisableWeapon();
            yield return new WaitForSeconds(_recoveryTime);
            
            _ai.SetAttackCooldown();
            _isAttacking = false;
            _ai.MainMachine.ChangeState(new StaticIdleState(_ai));
        }

        public void Exit()
        {
            _ai.DisableWeapon();
            _isAttacking = false;
        }
    }
}
