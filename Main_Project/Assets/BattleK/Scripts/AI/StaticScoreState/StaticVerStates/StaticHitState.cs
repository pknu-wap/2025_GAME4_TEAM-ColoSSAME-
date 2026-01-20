using System.Collections;
using UnityEngine;

namespace BattleK.Scripts.AI.StaticScoreState.StaticVerStates
{
    public class StaticHitState : IStaticScoreState
    {

        private readonly StaticAICore _ai;
        private readonly float _stunDuration;

        public StaticHitState(StaticAICore ai, float duration = 0.5f)
        {
            _ai = ai;
            _stunDuration = duration;
        }

        public void Enter()
        {
            _ai.PlayAnimation(PlayerState.DAMAGED);
        }
        
        public IEnumerator Execute()
        {
            yield return new WaitForSeconds(_stunDuration);
            _ai.OverrideMachine.StopAndClear();
        }

        public void Exit() { }
    }
}
