using System.Collections;
using Battle.Scripts.StateCore;
using UnityEngine;
namespace Battle.Scripts.Ai.State
{
    public class RetreatState : IState
    {
        private BattleAI ai;

        public RetreatState(BattleAI Ai) { this.ai = Ai; }

        public void EnterState()
        {
            Debug.Log($"{ai} : {ai.StateMachine.currentState}");
            
            ai.StopMoving();
            
            ai.aiAnimator.Reset();
            ai.aiAnimator.StopMove();
            ai.Retreater.GetComponent<RetreatTarget>().SetRetreatTarget();
        }

        public void UpdateState()
        {
            ai.aiAnimator.Move();
            ai.MoveTo(ai.Retreater.position);
            if (ai.IsInRetreatDistance())
            {
                ai.StateMachine.ChangeState(new IdleState(ai, true, ai.waitTime));
            }
        }
        public void ExitState()
        {
        }
    }
}