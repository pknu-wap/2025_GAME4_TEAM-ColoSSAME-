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
            ai.TakeDamage(damage);
            
            ai.FlashRedTransparent(0.8f, 0.1f);
            ai.StartCoroutine(EndDamageRoutine());
        }
        
        private IEnumerator EndDamageRoutine()
        {
            //Ai's CurrentHp <= 0 -> DeadState
            if (ai.IsDead()) ai.StateMachine.ChangeState(new DeadState(ai));
            else
            {
                if (ai.StateMachine.previousState is AttackState)
                {
                    stun += ai.AttackDelay;
                }
                //Weapon: Bow or Magic -> Idle(RetreatState), X -> Idle(ChaseState)
                if (ai.weaponType is WeaponType.Bow or WeaponType.Magic)
                {
                    ai.StateMachine.ChangeState(new IdleState(ai, false, stun));
                    yield return null;
                }
                else
                {
                    ai.StateMachine.ChangeState(new IdleState(ai, true, stun));
                }
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