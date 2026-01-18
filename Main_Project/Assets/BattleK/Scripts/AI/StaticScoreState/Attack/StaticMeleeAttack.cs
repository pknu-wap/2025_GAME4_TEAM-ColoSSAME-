using System.Collections.Generic;
using UnityEngine;

namespace BattleK.Scripts.AI.StaticScoreState.Attack
{
    public class StaticMeleeAttack : MonoBehaviour
    {
        [Header("Settings")] [SerializeField] private Collider2D _hitBox;

        private int _damage;
        private StaticAICore _owner;

        private readonly HashSet<GameObject> _hitTargets = new();

        private void Awake()
        {
            if (_hitBox) _hitBox.enabled = false;
        }

        public void Initialize(StaticAICore owner)
        {
            _owner = owner;
        }

        public void EnableHitBox(int damage)
        {
            _damage = damage;
            _hitTargets.Clear();
            _hitBox.enabled = true;
        }

        public void DisableHitBox()
        {
            _hitBox.enabled = false;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (_hitTargets.Contains(other.gameObject)) return;
            if (other.gameObject == _owner.gameObject) return;

            var target = other.GetComponent<StaticAICore>();
            if (!target || target.IsDead) return;

            target.OnTakeDamage(_damage);
            _hitTargets.Add(other.gameObject);
        }
    }
}
