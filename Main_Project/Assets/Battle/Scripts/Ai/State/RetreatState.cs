using Battle.Ai;
using Battle.Ai.State;
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
            ai.aiAnimator.Reset();
            ai.aiAnimator.Move();
            ai.Retreater.GetComponent<RetreatTarget>().SetRetreatTarget();
        }

        public void UpdateState()
        {
            ai.MoveTo(ai.Retreater.position);
            if (ai.IsInRetreatDistance())
            {
                if (ai.CurrentTarget != null)
                {
                    ai.destinationSetter.target = ai.CurrentTarget;
                    Debug.Log($"{ai.gameObject.name}의 RetreatState(currenttarget != null)에서 IdleState");
                    ai.StateMachine.ChangeState(new IdleState(ai, true, ai.waitTime));
                } else
                {
                    Debug.Log($"{ai.gameObject.name}의 RetreatState(currenttarget == null)에서 IdleState");
                    ai.StateMachine.ChangeState(new IdleState(ai, true,ai.waitTime));
                }
            }
        }
        public void ExitState()
        {
        }
    }
}