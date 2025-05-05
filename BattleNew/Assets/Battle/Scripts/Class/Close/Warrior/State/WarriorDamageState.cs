using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Battle.Scripts.Class.Close.Warrior.State
{
    public class WarriorDamageState : IState
    {
        private WarriorAI ai;
        private float damage;

        public WarriorDamageState(WarriorAI ai, float damage)
        {
            this.ai = ai;
            this.damage = damage;
        }

        public void EnterState()
        {
            ai.StopMoving();
            ai.warriorAnimator.Damaged();
            ai.TakeDamage(damage);
            ai.StartCoroutine(EndDamageRoutine());
        }
        
        private IEnumerator EndDamageRoutine()
        {
            yield return new WaitForSeconds(0.2f); // 피격 애니메이션 길이
            if (ai.IsDead()) ai.StateMachine.ChangeState(new WarriorDeadState(ai));
            else ai.StateMachine.ChangeState(new WarriorIdleState(ai));
        }

        public void UpdateState()
        {
            
        }

        public void ExitState()
        {
            
        }
    }
}