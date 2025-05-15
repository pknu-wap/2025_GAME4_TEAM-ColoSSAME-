using System.Collections;
using Battle.Scripts.StateCore;
using UnityEngine;

namespace Battle.Scripts.Ai.State
{
    public class DamageState : IState
    {
        private BattleAI ai;
        private float damage;
        private float stun;

        public DamageState(BattleAI ai, float damage, float stun)
        {
            this.ai = ai;
            this.damage = damage;
            this.stun = stun;
        }

        public void EnterState()
        {
            Debug.Log($"{ai} : {ai.StateMachine.currentState}");
            ai.TakeDamage(damage);
            ai.FlashRedTransparent(0.8f, 0.1f);
            ai.StartCoroutine(EndDamageRoutine());
        }
        
        private IEnumerator EndDamageRoutine()
        {
            if (ai.IsDead()) ai.StateMachine.ChangeState(new DeadState(ai));
            else
            {
                if (ai.StateMachine.previousState is AttackState)
                {
                    stun += ai.AttackDelay;
                }
                ai.StateMachine.ChangeState(new IdleState(ai, true, stun));
            }
            yield return null;
        }

        public void UpdateState()
        {
            
        }

        public void ExitState()
        {
            
        }
    }
}