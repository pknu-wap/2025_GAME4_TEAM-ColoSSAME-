using System.Collections;
using UnityEngine;

namespace Movement.State
{
    public class IdleState : IState
    {
        private BattleAI2 ai;
        private StateMachine stateMachine;
        private float idleTime = 1.0f;

        public IdleState(BattleAI2 ai, StateMachine stateMachine)
        {
            this.ai = ai;
            this.stateMachine = stateMachine;
        }

        public void EnterState()
        {
            ai.GetCharAnimator().Idle();
        }

        public IEnumerator ExecuteState()
        {
            yield return new WaitForSeconds(idleTime);

            Transform target = ai.GetTarget();
            if (target != null)
            {
                stateMachine.ChangeState(new ChaseState(ai, stateMachine, target));
            }
        }

        public void ExitState() { }
    }
}