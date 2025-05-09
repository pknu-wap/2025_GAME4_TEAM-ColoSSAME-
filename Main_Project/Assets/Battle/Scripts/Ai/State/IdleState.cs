using Battle.Ai;
using Battle.Ai.State;
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
        private bool starting;

        public IdleState(BattleAI ai, bool isChase, float waitTime)
        {
            this.ai = ai;
            this.isChase = isChase;
            this.waitTime = waitTime;
        }

        public void EnterState()
        {
            ai.aiAnimator.Reset();
            ai.aiAnimator.StopMove();
            ai.StopMoving();
            ai.aiPath.canMove = false;
            starting = false;
            ai.rb.velocity = Vector2.zero;
            ai.StartCoroutine(Wait(waitTime));
        }

        private IEnumerator Wait (float time)
        {
            yield return new WaitForSeconds(time);
            starting = true;
        }

        public void UpdateState()
        {
            if (starting)
            {
                if (isChase)
                {
                    if (ai.HasEnemyInSight())
                    {
                        ai.destinationSetter.target = ai.CurrentTarget;
                        if (ai.IsInAttackRange())
                        {
                            if (ai.CanAttack()) ai.StateMachine.ChangeState(new AttackState(ai));
                        } else
                        {
                            ai.StateMachine.ChangeState(new ChaseState(ai));
                        }
                    }
                } else
                {
                    ai.StateMachine.ChangeState(new RetreatState(ai));
                }
            }
        }

        public void ExitState ()
        {
            ai.aiPath.canMove = true;
        }
    }
}