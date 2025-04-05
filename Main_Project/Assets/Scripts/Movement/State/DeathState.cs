using System.Collections;
using UnityEngine;

namespace Movement.State
{
    public class DeathState : IState
    {
        private BattleAI2 ai;
        private StateMachine stateMachine;

        public DeathState(BattleAI2 ai, StateMachine stateMachine)
        {
            this.stateMachine = stateMachine;
            this.ai = ai;
        }

        public void EnterState()
        {
            
        }

        public IEnumerator ExecuteState()
        {
            Debug.Log("사망처리 판정 시작");
            ai.GetCharAnimator().Death();
            yield return new WaitForSeconds(1f);
            ai.KillThis();
        }

        public void ExitState()
        {
            
        }
    }
}