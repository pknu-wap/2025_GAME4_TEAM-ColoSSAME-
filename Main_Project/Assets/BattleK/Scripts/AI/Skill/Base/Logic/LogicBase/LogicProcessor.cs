using System.Collections.Generic;
using BattleK.Scripts.AI.Skill.Base.Projectile;
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
        
        protected Vector2 _direction;
        protected Vector2 _spawnPosition;

        public void Initialize(StaticAICore owner, List<ISkillLogic> logics, float lifeTime, LayerMask mask, Transform target, Vector2 targetPos)
        {
            _owner = owner;
            _logics = logics;
            _targetMask = mask;
            _targetTransform = target;
            _targetPosition = targetPos;
            _spawnPosition = targetPos;
            
            _direction = Vector2.zero;

            if (_targetTransform != null)
            {
                _direction = ((Vector2)(_targetTransform.position) - _spawnPosition).normalized;
            }

            ProjectileMovement movement = GetComponent<ProjectileMovement>();
            if (movement != null)
            {
                movement.Init(_direction);
            }
            
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