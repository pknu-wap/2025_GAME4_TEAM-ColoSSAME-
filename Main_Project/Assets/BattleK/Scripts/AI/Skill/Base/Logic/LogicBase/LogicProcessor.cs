using System.Collections.Generic;
using UnityEngine;

namespace BattleK.Scripts.AI.Skill.Base.Logic.LogicBase
{
    public abstract class LogicProcessor : MonoBehaviour 
    {
        protected StaticAICore _owner;
        private List<ISkillLogic> _logics;

        public void Initialize(StaticAICore owner, List<ISkillLogic> logics) 
        {
            _owner = owner;
            _logics = logics;
        }

        protected void ApplyLogicsToTarget(StaticAICore target) 
        {
            if (_logics == null || !target) return;
            foreach (var logic in _logics) 
            {
                logic.Execute(_owner, target); 
            }
        }
    }
}