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
            var projectile = Instantiate(_projectilePrefab, transform.position, Quaternion.identity);
            
            var dir = _owner.transform.localScale.x > 0 ? Vector3.left : Vector3.right;
            
            projectile.Initialize(_owner, damage, dir);
        }
    }
}
