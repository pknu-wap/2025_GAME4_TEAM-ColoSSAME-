using System.Collections;
using UnityEngine;

namespace Battle.Scripts.Class.Close.Warrior.State
{
    public class WarriorDeadState : IState
    {

        private WarriorAI ai;
        public WarriorDeadState(WarriorAI ai)
        {
            this.ai = ai;
        }
        public void EnterState()
        {
            ai.warriorAnimator.Dead();
            ai.StartCoroutine(Dead());
        }

        private IEnumerator Dead()
        {
            yield return new WaitForSeconds(1f);
            ai.gameObject.SetActive(false);
        }
        public void UpdateState()
        {
            
        }

        public void ExitState()
        {
            throw new System.NotImplementedException();
        }
    }
}
