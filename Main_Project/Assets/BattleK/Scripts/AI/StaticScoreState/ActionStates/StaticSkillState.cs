using System.Collections;
using BattleK.Scripts.AI.Skill.Base;
using UnityEngine;

namespace BattleK.Scripts.AI.StaticScoreState.ActionStates
{
    public class StaticSkillState : IStaticActionState
    {
        private readonly StaticAICore _ai;
        private readonly RuntimeSkill _skillRunner;
        private bool _isExecuting;
        
        public int Priority => 110;

        public StaticSkillState(StaticAICore ai, SkillSO skillData)
        {
            _ai = ai;
            _skillRunner = new RuntimeSkill(skillData);
        }

        public bool CanExecute()
        {
            if (_isExecuting) return true;
            if (!_ai.Target) return false;
            if (!_skillRunner.IsReady) return false;
            
            var distSq = (_ai.Target.position - _ai.transform.position).sqrMagnitude;
            var range = _skillRunner.Data.Range;
        
            return distSq <= range * range;
        }

        public void Enter() => _isExecuting = true;

        public IEnumerator Execute()
        {
            _ai.StopMovement();
            _ai.LookAt(_ai.Target.position);

            _ai.PlayAnimation(PlayerState.ATTACK, _skillRunner.Data.AnimationIndex);
            yield return new WaitForSeconds(_skillRunner.Data.WindupTime);
            yield return new WaitForSeconds(_skillRunner.Data.ActiveTime);
            yield return new WaitForSeconds(_skillRunner.Data.RecoveryTime);

            _isExecuting = false;
            _ai.MainMachine.ChangeState(new StaticIdleState(_ai));
        }

        public void Exit() => _isExecuting = false;
    }
}
