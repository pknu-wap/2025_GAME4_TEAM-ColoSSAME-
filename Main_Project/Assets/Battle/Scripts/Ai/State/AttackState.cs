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
            if(ai.CurrentTarget == null) ai.StateMachine.ChangeState(new IdleState(ai, true, 0f));
            //Weapon: Bow or Magic -> RangedAttack, X -> MeleeAttack
            if (ai.weaponType is WeaponType.Bow)
            {
                ArrowAttack();
            }
            else
            {
                if (ai.weaponType is WeaponType.Magic)
                {
                    MagicAttack();
                }
                else
                {
                    MeleeAttack();
                }
            }
        }

        void MeleeAttack()
        {
            ai.weaponTrigger.ActivateCollider();
            
            ai.RecordAttackTime();
            ai.StartCoroutine(AttackDelay());
        }

        void MagicAttack()
        {
            if(!ai.CurrentTarget || !ai.destinationSetter.target) ai.StateMachine.ChangeState(new IdleState(ai, true, ai.waitTime));
            ai.StartCoroutine(MagicAttackDelay());
        }
        
        void ArrowAttack()
        {
            if(!ai.CurrentTarget || !ai.destinationSetter.target) ai.StateMachine.ChangeState(new IdleState(ai, true, ai.waitTime));
            ai.StartCoroutine(ArrowAttackDelay());
        }

        private IEnumerator MagicAttackDelay()
        {
            ai.arrowWeaponTrigger.FireArrow();
            yield return new WaitForSeconds(ai.AttackDelay);
            ai.StateMachine.ChangeState(new IdleState(ai,false,ai.waitTime));
        }
        
        private IEnumerator ArrowAttackDelay()
        {
            yield return new WaitForSeconds(ai.AttackDelay / 2);
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
            
            ai.StateMachine.ChangeState(new IdleState(ai,false,ai.waitTime));
        }

        public void UpdateState()
        {
            
        }

        public void ExitState() { }
    }
}