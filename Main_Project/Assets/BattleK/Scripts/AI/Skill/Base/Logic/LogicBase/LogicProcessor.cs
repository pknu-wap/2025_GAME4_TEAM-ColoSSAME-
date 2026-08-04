using System.Collections.Generic;
using UnityEngine;

namespace BattleK.Scripts.AI.Skill.Base.Logic.LogicBase
{
    public abstract class LogicProcessor : MonoBehaviour 
    {
        protected StaticAICore _owner;
        private List<ISkillLogic> _logics;
        protected LayerMask _targetMask;
        protected Transform _targetTransform;
        protected Vector2 _targetPosition;
        protected int _maxHitTargets;

        public void Initialize(StaticAICore owner, List<ISkillLogic> logics, float lifeTime, LayerMask mask, Transform target, Vector2 targetPos, int maxHitTargets = 0)
        {
            _owner = owner;
            _logics = logics;
            _targetMask = mask;
            _targetTransform = target;
            _targetPosition = targetPos;
            _maxHitTargets = Mathf.Max(0, maxHitTargets);
            Destroy(gameObject, lifeTime);
        }
        
        public abstract void StartProcess();

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
