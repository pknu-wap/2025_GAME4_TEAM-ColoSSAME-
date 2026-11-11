using System.Collections;
using BattleK.Scripts.AI.StaticScoreState;
using UnityEngine;

namespace BattleK.Scripts.AI.AIState.StaticVerStates
{
    public class StaticDeathState : IStaticScoreState
    {
        private readonly StaticAICore _ai;
        private IStaticScoreState _staticScoreStateImplementation;

        public StaticDeathState(StaticAICore ai)
        {
            _ai = ai;
        }

        public void Enter()
        {
            _ai.PlayAnimation(PlayerState.DEATH);
        }

        public IEnumerator Execute()
        {
            const float waitTime = 1.5f;
            yield return new WaitForSeconds(waitTime);
            _ai.gameObject.SetActive(false);
        }

        public void Exit()
        {
        
        }
    }
}
