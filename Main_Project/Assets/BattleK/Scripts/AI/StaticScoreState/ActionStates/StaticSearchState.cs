using System.Collections;
using UnityEngine;

namespace BattleK.Scripts.AI.StaticScoreState.ActionStates
{
    public class StaticSearchState : IStaticActionState
    {
        private readonly StaticAICore _ai;
    
        private const float ScanInterval = 0.5f;

        public StaticSearchState(StaticAICore ai)
        {
            _ai = ai;
        }

        public int Priority => 20;
        public bool CanExecute()
        {
            return !_ai.Target;
        }
        public void Enter()
        {
            _ai.PlayAnimation(PlayerState.IDLE);
            _ai.StopMovement();
        }

        public IEnumerator Execute()
        {
            while (true)
            {
                if (_ai.Target) yield break;

                if (_ai.Targeting != null)
                {
                    var foundTarget = _ai.Targeting.FindTarget(_ai);
                
                    if (foundTarget)
                    {
                        _ai.Target = foundTarget;
                    }
                }
            
                yield return new WaitForSeconds(ScanInterval);
            }
        }

        public void Exit()
        {
            
        }
    }
}
