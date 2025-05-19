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
            ai.aiAnimator.Move();
            
            if(!ai.IsRetreating) ai.Retreater.GetComponent<RetreatTarget>().SetRetreatTarget();
            ai.IsRetreating = true;
        }

        public void UpdateState()
        {
            ai.MoveTo(ai.Retreater.position);
            if (ai.IsInRetreatDistance())
            {
                ai.IsRetreating = false;
                ai.StateMachine.ChangeState(new IdleState(ai, true, ai.waitTime));
            }
        }
        public void ExitState()
        {
        }
    }
}