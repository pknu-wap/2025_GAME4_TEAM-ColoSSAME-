using System.Collections;
using UnityEngine;

namespace BattleK.Scripts.AI.StaticScoreState.StaticVerStates
{
    public class StaticCCState : IStaticScoreState
    {
        private readonly StaticAICore _ai;
        private readonly float _duration;
        private readonly PlayerState _animationState;
        
        public StaticCCState(StaticAICore ai, PlayerState animationName = PlayerState.DEBUFF)
        {
            _ai = ai;
            _animationState = animationName;
        }
        
        public void Enter()
        {
            _ai.StopMovement();
            _ai.PlayAnimation(_animationState);
        }

        public IEnumerator Execute()
        {
            while (true)
            {
                yield return null; 
            }
        }

        public void Exit()
        {
            _ai.ResumeMovement();
        }
    }
}
