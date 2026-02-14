using BattleK.Scripts.Data.Type.AIDataType.CC;
using BattleK.Scripts.Manager.Battle;
using UnityEngine;

namespace BattleK.Scripts.AI.Skill.Base
{
    public class SkillEffect : MonoBehaviour
    {
        private BoxCollider2D _col;
        private float _damage;
        private StaticAICore _owner;
        private bool _isContinuous;
        private bool _isInitialized = false;
        private CCData _ccData;
        private static readonly Collider2D[] _results = new Collider2D[4];

        public void Setup(StaticAICore owner, Vector2 area, float damage, bool isContinuous, CCData ccData)
        {
            _owner = owner;
            
            _col = GetComponent<BoxCollider2D>();
            _col.isTrigger = true;
            _col.size = area;
            _col.offset = Vector2.zero; 
        
            _damage = damage;
            _isInitialized = true;
            _isContinuous = isContinuous;
            _ccData = ccData;
            
            if (!_isContinuous)
            {
                ExecuteInstantDamage(area);
            }
        }

        private void ExecuteInstantDamage(Vector2 area)
        {
            var hitCount = Physics2D.OverlapBoxNonAlloc(
                (Vector2)transform.position + (Vector2)(transform.up * _col.offset.y), 
                area, 
                transform.eulerAngles.z, 
                _results
            );
            for (var i = 0; i < hitCount; i++)
            {
                ApplyDamage(_results[i]);
                _results[i] = null; 
            }

            _col.enabled = false;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!_isInitialized || !_isContinuous) return;
            ApplyDamage(other);
        }
        
        private void ApplyDamage(Collider2D other)
        {
            if (!other.TryGetComponent(out StaticAICore targetAI)) return;
            if (targetAI == _owner) return;
            if (targetAI.gameObject.layer == _owner.TargetLayer) return;
            
            targetAI.OnTakeDamage((int)_damage);
            
            if (_ccData != null && targetAI.TryGetComponent(out CCManager ccManager))
            {
                ccManager.ApplyCC(_ccData);
            }
        }
    }
}