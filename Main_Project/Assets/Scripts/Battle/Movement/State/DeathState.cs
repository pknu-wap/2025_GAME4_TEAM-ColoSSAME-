using System.Collections;
using Battle.Value;
using UnityEngine;

namespace Battle.Movement.State
{
    public class DeathState : IState
    {
        private BattleAI2 ai;

        public DeathState(BattleAI2 ai)
        {
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