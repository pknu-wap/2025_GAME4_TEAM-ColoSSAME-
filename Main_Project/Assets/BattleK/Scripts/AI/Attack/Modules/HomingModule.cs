using UnityEngine;

namespace BattleK.Scripts.AI.Attack.Modules
{
    public class HomingModule : MonoBehaviour, IProjectileVelocityModifier
    {
        [Header("Homing")]
        [SerializeField] private int _priority;
        [SerializeField] private float _turnSpeedDegPerSec = 180f;
        [SerializeField] private float _detectRadius = 8f;
        [SerializeField] private LayerMask _targetLayer;
        [SerializeField] private int _maxDetectCount = 10;

        private Transform _target;
        private Collider2D[] results;
        public int Priority => _priority;

        private void OnEnable()
        {
            _target = null;
        }

        public void ModifyVelocity(ref Vector2 direction, ref float speed, float deltaTime, Transform self)
        {
            if (!_target)
            {
                _target = FindNearestTarget(self.position);
                if(!_target) return;
            }

            var toTarget = ((Vector2)_target.position - (Vector2)self.position).normalized;
            direction = Vector3.RotateTowards(direction,
                    toTarget,
                    _turnSpeedDegPerSec * Mathf.Deg2Rad * deltaTime,
                    0f)
                .normalized;
        }

        private Transform FindNearestTarget(Vector2 origin)
        {
            var count = Physics2D.OverlapCircleNonAlloc(origin, _detectRadius, results, _targetLayer);
            if (count == 0) return null;
            Transform nearest = null;
            var best = float.MaxValue;
            foreach (var hit in results)
            {
                var dist = ((Vector2)hit.transform.position - origin).sqrMagnitude;
                if (!(dist < best)) continue;
                best = dist;
                nearest = hit.transform;
            }

            return nearest;
        }
    }
}