using Battle.Scripts.StateCore;
using System.Collections;
using UnityEngine;
namespace Battle.Scripts.Ai.State
{
    public class IdleState : IState
    {
        private BattleAI ai;
        private bool isChase;
        private float waitTime;
        private bool canMove;

        public IdleState(BattleAI ai, bool isChase, float waitTime)
        {
            this.ai = ai;
            this.isChase = isChase;
            this.waitTime = waitTime;
        }

        public void EnterState()
        {
            if (ai.StateMachine.currentState is RetreatState)
                return;
            Debug.Log($"{ai} : {ai.StateMachine.currentState}");
            ai.StopMoving();
            ai.aiAnimator.Reset();
            ai.aiAnimator.StopMove();
            canMove = false;
            ai.StartCoroutine(Wait(waitTime));
        }

        
        private IEnumerator Wait (float time)
        {
            yield return new WaitForSeconds(time);
            canMove = true;
        }

        public void UpdateState()
        {
            if (!canMove) return;
            
            if (isChase)
            {
                if (!ai.HasEnemyInSight()) return;
                if (ai.IsInAttackRange())
                {
                    if (ai.CanAttack()) ai.StateMachine.ChangeState(new AttackState(ai));
                } else
                {
                    ai.StateMachine.ChangeState(new ChaseState(ai));
                }
            } else
            {
                ai.StateMachine.ChangeState(new RetreatState(ai));
            }
        }

        public void ExitState ()
        {
        }
    }
}