using UnityEngine;

namespace BattleK.Scripts.AI.StaticScoreState.Attack
{
    public class StaticRangedAttack : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private StaticProjectile _projectilePrefab;

        private StaticAICore _owner;

        public void Initialize(StaticAICore owner)
        {
            _owner = owner;
        }

        public void Fire(int damage)
        {
            if (!_owner.Target) return;
            
            var projectile = Instantiate(_projectilePrefab, transform.position, Quaternion.identity);
            var dir = (_owner.Target.position - transform.position).normalized;
            projectile.Initialize(_owner, damage, dir);
        }
    }
}
