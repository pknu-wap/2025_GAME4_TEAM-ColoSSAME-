using System.Collections;
using Battle.Scripts.StateCore;
using UnityEngine;

namespace Battle.Scripts.Ai.State
{
    public class DeadState : IState
    {

        private BattleAI ai;
        public DeadState(BattleAI ai) { this.ai = ai; }
        public void EnterState()
        {
            Debug.Log($"{ai} : {ai.StateMachine.currentState}");
            
            ai.aiAnimator.Dead();
            
            ai.StartCoroutine(Dead());
        }

        private IEnumerator Dead()
        {
            yield return new WaitForSeconds(0.3f);
            ai.Kill();
            
        }
        public void UpdateState()
        {
            
        }

        public void ExitState()
        {
        }
    }
}
