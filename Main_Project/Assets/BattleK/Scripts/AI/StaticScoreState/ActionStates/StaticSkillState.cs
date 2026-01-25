using System.Collections;

namespace BattleK.Scripts.AI.StaticScoreState.ActionStates
{
    public class StaticSkillState : IStaticActionState
    {
        private readonly StaticAICore _ai;
        private readonly float _windupTime;
        private readonly float _activeTime;
        private readonly float _recoveryTime;

        private bool _isUsingSkill;
        
        public int Priority => 105;

        public StaticSkillState(StaticAICore ai, float windupTime, float activeTime, float recoveryTime)
        {
            _ai = ai;
            _windupTime = windupTime;
            _activeTime = activeTime;
            _recoveryTime = recoveryTime;
        }
        
        public bool CanExecute()
        {
            if (_isUsingSkill) return true;
            if (!_ai.Target || !_ai.Target.gameObject.activeInHierarchy) return false;
            if (!_ai.IsAttackReady) return false;
            
            var distSq = (_ai.Target.position - _ai.transform.position).sqrMagnitude;
            var range = _ai.Stat.AttackRange + 0.1f;
            return distSq <= range * range;
        }
        
        public void Enter()
        {
            throw new System.NotImplementedException();
        }

        public IEnumerator Execute()
        {
            throw new System.NotImplementedException();
        }

        public void Exit()
        {
            throw new System.NotImplementedException();
        }
    }
}
