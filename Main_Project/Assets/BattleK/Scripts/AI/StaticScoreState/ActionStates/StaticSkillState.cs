using System.Collections;
using System.Collections.Generic;
using BattleK.Scripts.AI.Skill.Base;
using UnityEngine;

namespace BattleK.Scripts.AI.StaticScoreState.ActionStates
{
    public class StaticSkillState : IStaticActionState
    {
        private readonly StaticAICore _ai;
        private readonly List<RuntimeSkill> _runtimeSkills = new();
        private RuntimeSkill _selectedSkill;
        private bool _isExecuting;
        
        public int Priority => 110;

        public StaticSkillState(StaticAICore ai, List<SkillSO> skillDataList)
        {
            _ai = ai;
            foreach (var data in skillDataList)
            {
                _runtimeSkills.Add(new RuntimeSkill(data));
            }
        }

        public bool CanExecute()
        {
            if (_isExecuting) return true;
            if (!_ai.Target) return false;
            _selectedSkill = null;
            
            _selectedSkill = null;
            var highestPriority = int.MinValue;
            
            foreach (var skill in _runtimeSkills)
            {
                if (!skill.IsReady) continue;

                if (!skill.Data.IsInArea(_ai.transform, _ai.Target)) continue;
                if (skill.Data.InternalPriority <= highestPriority) continue;
                highestPriority = skill.Data.InternalPriority;
                _selectedSkill = skill;
            }
            return _selectedSkill != null;
        }

        public void Enter() { }

        public IEnumerator Execute()
        {
            if (_selectedSkill == null) yield break;
            
            _isExecuting = true;
            _ai.StopMovement();
            
            var data = _selectedSkill.Data;

            _ai.PlayAnimation(PlayerState.ATTACK, data.AnimationIndex);
            _selectedSkill.ResetCooldown();
            
            yield return data.ExecuteSkillRoutine(_ai, _ai.Target);
            
            _isExecuting = false;
            _ai.MainMachine.ChangeState(new StaticIdleState(_ai));
        }

        public void Exit() => _isExecuting = false;
    }
}
