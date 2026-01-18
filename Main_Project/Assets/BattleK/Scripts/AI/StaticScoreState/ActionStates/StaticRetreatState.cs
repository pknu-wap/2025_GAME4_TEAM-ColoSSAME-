using System.Collections;
using UnityEngine;

namespace BattleK.Scripts.AI.StaticScoreState.ActionStates
{
    public class StaticRetreatState : IStaticActionState
    {
        private readonly StaticAICore _ai;

        private const float defaultRetreatTime = 3.0f;
        private const float CorneredTimePenalty = 1.5f;
        private const float WallCheckDistance = 2.0f;
        private const float retreatHpThreshold = 0.3f;
        private const float retreatDistance = 4.0f;
        private const float retreatSpeedMultiplier = 1.2f;

        private readonly LayerMask _obstacleMask;

        private float currentTimer;
        private bool isCornered;
        
        public StaticRetreatState(StaticAICore ai)
        {
            _ai = ai;
            _obstacleMask = LayerMask.GetMask("Wall");
        }

        public int Priority => 110;
        
        public bool CanExecute()
        {
            if (!_ai.Target) return false;
            if(Time.time < _ai.LastRetreatFinishTime + 5.0f) return false;
            var hpRatio = _ai.Stat.CurrentHP / _ai.Stat.MaxHP;
            return hpRatio < retreatHpThreshold;
        }
        
        public void Enter()
        {
            currentTimer = defaultRetreatTime;
            isCornered = false;
            
            _ai.PlayAnimation(PlayerState.MOVE);
            _ai.SetMoveSpeedMultiplier(retreatSpeedMultiplier);
        }

        public IEnumerator Execute()
        {
            while (currentTimer > 0)
            {
                currentTimer -= Time.deltaTime;
                
                if (_ai.Target)
                {
                    var myPos = _ai.transform.position;
                    var targetPos = _ai.Target.transform.position;
                    
                    var fleeDir = (myPos - targetPos).normalized;
                    var hit = Physics2D.Raycast(myPos, fleeDir, WallCheckDistance, _obstacleMask);
                    if (hit.collider)
                    {
                        if (!isCornered)
                        {
                            isCornered = true;
                            currentTimer -= CorneredTimePenalty;
                        }
                        var leftDir = new Vector2(-fleeDir.y, fleeDir.x);
                        var rightDir = new Vector2(fleeDir.y, -fleeDir.x);
                        
                        bool leftBlocked = Physics2D.Raycast(myPos, fleeDir, WallCheckDistance, _obstacleMask);

                        fleeDir = !leftBlocked ? leftDir : rightDir;
                    }
                    var dest = myPos + fleeDir * retreatDistance;
                    _ai.MoveTo(dest);
                    _ai.LookAt(dest);
                }

                yield return null;
            }
            
            _ai.MainMachine.ChangeState(new StaticIdleState(_ai));
        }

        public void Exit()
        {
            _ai.ResetMoveSpeed();
            _ai.LastRetreatFinishTime = Time.time;
        }
    }
}
