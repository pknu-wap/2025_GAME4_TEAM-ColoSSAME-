using Battle.Scripts.StateCore;
using UnityEngine;

namespace Battle.Scripts.Ai.State
{
    public class IdleState : IState
    {
        private BattleAI ai;

        public IdleState(BattleAI ai)
        {
            this.ai = ai;
        }

        public void EnterState()
        {
            ai.StopMoving();
            ai.aiAnimator.Reset();
        }

        public void UpdateState()
        {
            if (ai.HasEnemyInSight())
            {
                if (ai.IsInAttackRange())
                {
                    ai.rb.velocity = Vector2.zero;
                    if(ai.CanAttack()) ai.StateMachine.ChangeState(new AttackState(ai));
                }
                else
                {
                    ai.StateMachine.ChangeState(new ChaseState(ai));
                    ai.aiAnimator.Move();
                }
            }
        }

        public void ExitState() { }
    }
}