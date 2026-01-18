using System.Collections;
using UnityEngine;

namespace BattleK.Scripts.AI.StaticScoreState.ActionStates
{
    public class StaticChaseState : IStaticActionState
    {

        private readonly StaticAICore _ai;
        private const float PathUpdateInterval = 0.1f;
        
        public StaticChaseState(StaticAICore ai)
        {
            _ai = ai;
        }
        
        public int Priority => 40;
        public bool CanExecute()
        {
            if (!_ai.Target) return false;
            var distance = Vector3.Distance(_ai.transform.position, _ai.Target.position);
            return distance > _ai.Stat.AttackRange;
        }
        
        public void Enter()
        {
            _ai.PlayAnimation(PlayerState.MOVE);
            _ai.ResumeMovement();
        }

        public IEnumerator Execute()
        {
            var timer = 0f;
            while (true)
            {
                if (!_ai.Target) yield break;
                if (timer <= 0f)
                {
                    _ai.MoveTo(_ai.Target.position);
                    timer = PathUpdateInterval;
                }
                else timer -= Time.deltaTime;
                
                _ai.LookAt(_ai.Target.position);
                
                yield return null;
            }
        }

        public void Exit()
        {
            _ai.StopMovement();
        }
    }
}
