using BattleK.Scripts.Manager;
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

            var dir = (_owner.Target.position - transform.position).normalized;
            var rotation = Quaternion.AngleAxis(Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg, Vector3.forward);

            var projectile = PrefabPoolManager.Instance.Spawn(_projectilePrefab, transform.position, rotation);
            if (!projectile) return;

            projectile.Initialize(_owner, damage, dir);
        }
    }
}