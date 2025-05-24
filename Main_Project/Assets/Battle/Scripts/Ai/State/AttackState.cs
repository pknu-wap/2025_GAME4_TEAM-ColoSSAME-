using System.Collections;
using Battle.Scripts.StateCore;
using UnityEngine;

namespace Battle.Scripts.Ai.State
{
    public class AttackState : IState
    {
        private BattleAI ai;
        public AttackState(BattleAI ai) { this.ai = ai; }

        public void EnterState()
        {
            Debug.Log($"{ai} : {ai.StateMachine.currentState}");
            
            ai.StopMoving();
            
            ai.Flip(ai.CurrentTarget.position);
            ai.aiAnimator.Reset();
            ai.aiAnimator.Attack();
            
            //Weapon: Bow or Magic -> RangedAttack, X -> MeleeAttack
            if (ai.weaponType is WeaponType.Bow or WeaponType.Magic)
            {
                RangedAttack();
            }
            else
            {
                MeleeAttack();
            }
        }

        void MeleeAttack()
        {
            ai.weaponTrigger.ActivateCollider();
            
            ai.RecordAttackTime();
            ai.StartCoroutine(AttackDelay());
        }

        void RangedAttack()
        {
            ai.StartCoroutine(RangedAttackDelay());
        }

        private IEnumerator RangedAttackDelay()
        {
            yield return new WaitForSeconds(ai.AttackDelay);
            ai.arrowWeaponTrigger.FireArrow();
            ai.StateMachine.ChangeState(new IdleState(ai,false,ai.waitTime));
        }

        private IEnumerator AttackDelay()
        {
            ai.weaponTrigger.ColliderMove();
            yield return new WaitForSeconds(ai.AttackDelay / 2);
            
            ai.aiAnimator.StopMove();
            ai.weaponTrigger.DeactivateCollider();
            yield return new WaitForSeconds(ai.AttackDelay / 2);
            
            if(ai.weaponType == WeaponType.ShortSword) ai.StateMachine.ChangeState(new IdleState(ai,false,ai.waitTime));
            else ai.StateMachine.ChangeState(new IdleState(ai,true,ai.waitTime));
        }

        public void UpdateState() { }

        public void ExitState() { }
    }
}