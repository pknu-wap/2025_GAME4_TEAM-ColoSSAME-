using System.Collections;
using UnityEngine;

namespace Battle.Movement.State
{
    public class DeathState2 : IState
    {
        private BattleAI2 ai;

        public DeathState2(BattleAI2 ai, StateMachine stateMachine)
        {
            this.ai = ai;
        }
        public void EnterState()
        {
            ai.GetCharAnimator().Death();
            Debug.unityLogger.Log("Entered DeathState2");
        }

        public IEnumerator ExecuteState()  //문제가 되는 부분
        {
            Debug.unityLogger.Log("Executed DeathState2");
            yield return new WaitForSeconds(1f);
            ai.KillThis();
        }

        public void ExitState()
        {
        
        }
    }
}