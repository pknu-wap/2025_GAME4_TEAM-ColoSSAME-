using Battle.Scripts.Ai.State;
using Battle.Scripts.StateCore;
using System.Collections;
using UnityEngine;
namespace Battle.Ai.State
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
            ai.TakeDamage(damage);
            ai.FlashRedTransparent(0.8f, 0.1f);
            ai.StartCoroutine(EndDamageRoutine());
        }
        
        private IEnumerator EndDamageRoutine()
        {
            if (ai.IsDead()) ai.StateMachine.ChangeState(new DeadState(ai));
            else
            {
                Debug.Log($"{ai.gameObject.name}의 DamageState에서 IdleState");
                ai.StateMachine.ChangeState(ai.CurrentTarget == ai.destinationSetter.target ? new IdleState(ai, true, stun) : new IdleState(ai, false, stun));
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