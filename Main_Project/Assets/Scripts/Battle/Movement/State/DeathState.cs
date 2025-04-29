using System.Collections;
using UnityEditor.Timeline.Actions;
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
            Debug.Log("사망처리 판정 시작");
            ai.GetCharAnimator().Death();
            ai.KillThis();
        }

        public IEnumerator ExecuteState()
        {
            yield return null;
        }

        public void ExitState()
        {
            
        }
    }
}