using System;
using UnityEngine;

namespace BattleK.Scripts.AI.StaticScoreState.Attack
{
    public class StaticProjectile : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private float _speed = 10f;
        [SerializeField] private float _lifetime = 3f;

        private int _damage;
        private StaticAICore _owner;
        private Vector3 _direction;

        public void Initialize(StaticAICore owner, int damage, Vector3 direction)
        {
            _owner = owner;
            _damage = damage;
            _direction = direction;
            
            var angle = Mathf.Atan2(_direction.y, _direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
            
            Destroy(gameObject, _lifetime);
        }

        private void Update()
        {
            transform.Translate(Vector3.right * (_speed * Time.deltaTime));
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (_owner && other.gameObject == _owner.gameObject) return;
            if (other.GetComponent<Projectile>()) return;
            
            var target = other.GetComponent<StaticAICore>();
            if (!target || target.IsDead) return;
            target.OnTakeDamage(_damage);
            Destroy(gameObject);
        }
    }
}
