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
            ai.aiAnimator.Move();
        }

        public void UpdateState()
        {
            Debug.Log(ai.IsInAttackRange());
            if (ai.IsInAttackRange() && ai.CanAttack()) ai.StateMachine.ChangeState(new AttackState(ai));
            if (ai.CurrentTarget != null)
            {
                ai.MoveTo(ai.CurrentTarget.position);
            }
            else
            {
                ai.Targeting.FindNearestEnemy();
            }
        }

        public void ExitState() => ai.StopMoving();
    }
}