using System;
using System.Collections;
using UnityEngine;

namespace BattleK.Scripts.AI.StaticScoreState.ActionStates
{
    public class StaticIdleState : IStaticActionState
    {
        private readonly StaticAICore _ai;
        public int Priority => 0;

        public StaticIdleState(StaticAICore ai)
        {
            _ai = ai;
        }
        public void Enter()
        {
            _ai.StopMovement();
            _ai.PlayAnimation(PlayerState.IDLE);
        }

        public IEnumerator Execute()
        {
            while (true)
            {
                if(!_ai.Target && _ai.Target.gameObject.activeInHierarchy) _ai.LookAt(_ai.Target.position);
                yield return null;
            }
        }

        public void Exit() { }
        public bool CanExecute()
        {
            return true;
        }
    }
}
