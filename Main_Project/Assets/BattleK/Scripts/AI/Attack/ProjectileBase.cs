using System.Linq;
using UnityEngine;
using BattleK.Scripts.Manager;

namespace BattleK.Scripts.AI.Attack
{
    public class ProjectileBase : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField] private float _speed = 10f;
        [SerializeField] private float _lifetime = 3f;
        
        [Header("Direction Settings")]
        [SerializeField] private bool _useFlip = true;
        [SerializeField] private bool _useRotation = true;
        
        [Header("Art Offset")]
        [SerializeField] private float _baseAngleOffset;

        private Vector2 _direction;
        private float _currentSpeed;

        private IProjectileVelocityModifier[] _modifiers;

        protected Vector2 Direction => _direction;
        protected float CurrentSpeed => _currentSpeed;

        public virtual void Init(Vector2 direction)
        {
            _direction = direction.normalized;
            _currentSpeed = _speed;

            CacheModifiers();

            if (_direction != Vector2.zero)
            {
                var isLeft = _direction.x < 0f;
                if (_useFlip) ApplyFlip(isLeft);
                if (_useRotation) ApplyRotation(_direction, isLeft);
            }
            
            CancelInvoke(nameof(ReturnToPool));
            Invoke(nameof(ReturnToPool), _lifetime);
        }

        private void CacheModifiers()
        {
            _modifiers = GetComponents<IProjectileVelocityModifier>().OrderBy(m => m.Priority).ToArray();
        }

        private void Update()
        {
            ApplyModifiers(Time.deltaTime);
            Move(Time.deltaTime);
        }

        private void ApplyModifiers(float deltaTime)
        {
            if (_modifiers == null || _modifiers.Length == 0) return;
            foreach (var modifier in _modifiers)
            {
                modifier.ModifyVelocity(ref _direction, ref _currentSpeed, deltaTime, transform);
            }
        }

        protected virtual void Move(float deltaTime)
        {
            transform.position += (Vector3)(_direction * (_currentSpeed * deltaTime));
            if (!_useRotation || _direction == Vector2.zero) return;
            var isLeft = _direction.x < 0f;
            ApplyRotation(_direction, isLeft);
        }

        private void ApplyFlip(bool isLeft)
        {
            var scale = transform.localScale;
            scale.x = Mathf.Abs(scale.x) * (isLeft ? -1f : 1f);
            transform.localScale = scale;
        }

        private void ApplyRotation(Vector2 direction, bool isLeft)
        {
            var angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            if (isLeft) angle += 180f;
            transform.rotation = Quaternion.Euler(0f, 0f, angle + _baseAngleOffset);
        }

        private void ReturnToPool()
        {
            CancelInvoke(nameof(ReturnToPool));
            if(PrefabPoolManager.Instance) PrefabPoolManager.Instance.Release(gameObject);
            else Destroy(gameObject);
        }

        private void OnDisable()
        {
            CancelInvoke(nameof(ReturnToPool));
            _direction = Vector2.zero;
            _currentSpeed = 0f;
        }
    }
}