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
            if (ai.StateMachine.currentState is RetreatState)
                return;

            Debug.Log($"{ai} : {ai.StateMachine.currentState}");
            ai.aiAnimator.Reset();
            ai.aiAnimator.Move();
        }
        
        public void UpdateState()
        {
            if (ai.IsInAttackRange() && ai.CanAttack()) 
            {
                ai.StateMachine.ChangeState(new AttackState(ai));
                return;
            }

            // 대상이 null이거나 삭제된 경우
            if (!ai.CurrentTarget || !ai.destinationSetter.target)
            {
                ai.StopMoving();
                ai.CurrentTarget = null;
                ai.destinationSetter.target = null;
                Transform newTarget = ai.Targeting.FindNearestEnemy();

                if (newTarget == null)
                {
                    ai.StateMachine.ChangeState(new IdleState(ai, false, ai.waitTime)); // 후퇴
                }
                return;
            }
            // 정상적으로 추적
            ai.destinationSetter.target = ai.CurrentTarget;
            ai.MoveTo(ai.CurrentTarget.position);
        }

        public void ExitState(){}
    }
}