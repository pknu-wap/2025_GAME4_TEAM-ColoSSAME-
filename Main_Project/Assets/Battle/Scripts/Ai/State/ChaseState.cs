using Battle.Scripts.StateCore;
using UnityEngine;
namespace Battle.Scripts.Ai.State
{
    public class ChaseState : IState
    {
        private BattleAI ai;

        public ChaseState(BattleAI ai) { this.ai = ai; }

        public void EnterState()
        {
            Debug.Log($"{ai} : {ai.StateMachine.currentState}");
            ai.aiAnimator.Reset();
            ai.aiAnimator.Move();
            ai.aiPath.canMove = true;
        }
        
        public void UpdateState()
        {
            if (ai.IsInAttackRange() && ai.CanAttack()) ai.StateMachine.ChangeState(new AttackState(ai));
            if (ai.CurrentTarget != null)
            {
                ai.destinationSetter.target = ai.CurrentTarget;
                ai.MoveTo(ai.CurrentTarget.position);
            }
            else
            {
                ai.Targeting.FindNearestEnemy();
                ai.StateMachine.ChangeState(new IdleState(ai, true,ai.waitTime));
            }
        }

        public void ExitState(){}
    }
}