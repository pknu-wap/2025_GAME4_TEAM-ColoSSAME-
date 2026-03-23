using System.Collections.Generic;
using UnityEngine;

namespace BattleK.Scripts.AI.Skill.General
{
    /// <summary>
    /// Attach this to any trigger-based 2D collider (Box/Circle/Capsule/Polygon) to force enemies
    /// inside the zone to target the owner for a limited duration.
    /// Existing runtime code is not modified; this works by directly assigning StaticAICore.Target.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class ForcedTargetZone2D : MonoBehaviour
    {
        [SerializeField] private StaticAICore _owner;
        [SerializeField] private float _forceTargetDuration = 2f;
        [SerializeField] private bool _refreshDurationWhileInside = true;
        [SerializeField] private bool _autoFindOwnerInParent = true;
        [SerializeField] private GameObject _zoneVfxPrefab;
        [SerializeField] private Vector3 _zoneVfxOffset = Vector3.zero;
        [SerializeField] private GameObject _hitVfxPrefab;
        [SerializeField] private Vector3 _hitVfxOffset = Vector3.zero;

        private readonly Dictionary<StaticAICore, ForcedTargetRecord> _forcedTargets = new();
        private readonly List<StaticAICore> _releaseBuffer = new();
        private Collider2D _zoneCollider;

        private struct ForcedTargetRecord
        {
            public Transform PreviousTarget;
            public Transform ForcedTarget;
            public float ReleaseTime;
        }

        private void Awake()
        {
            _zoneCollider = GetComponent<Collider2D>();
            _zoneCollider.isTrigger = true;

            if (_owner == null && _autoFindOwnerInParent)
            {
                _owner = GetComponentInParent<StaticAICore>();
            }

            if (_zoneVfxPrefab != null)
            {
                Instantiate(_zoneVfxPrefab, transform.position + _zoneVfxOffset, Quaternion.identity, transform);
            }
        }

        /// <summary>
        /// Skill prefab side initialization helper.
        /// </summary>
        public void Initialize(StaticAICore owner, float forceTargetDuration)
        {
            _owner = owner;
            _forceTargetDuration = forceTargetDuration;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            TryForceTarget(other);
        }

        private void OnTriggerStay2D(Collider2D other)
        {
            if (!_refreshDurationWhileInside)
            {
                return;
            }

            TryForceTarget(other);
        }

        private void Update()
        {
            if (_forcedTargets.Count == 0)
            {
                return;
            }

            _releaseBuffer.Clear();

            foreach (KeyValuePair<StaticAICore, ForcedTargetRecord> pair in _forcedTargets)
            {
                StaticAICore enemy = pair.Key;
                ForcedTargetRecord record = pair.Value;

                if (enemy == null || enemy.IsDead || _owner == null || _owner.IsDead)
                {
                    ReleaseForcedTarget(enemy, record);
                    _releaseBuffer.Add(enemy);
                    continue;
                }

                if (Time.time < record.ReleaseTime)
                {
                    continue;
                }

                ReleaseForcedTarget(enemy, record);
                _releaseBuffer.Add(enemy);
            }

            for (int i = 0; i < _releaseBuffer.Count; i++)
            {
                _forcedTargets.Remove(_releaseBuffer[i]);
            }
        }

        private void OnDisable()
        {
            ClearAllForcedTargets();
        }

        private void OnDestroy()
        {
            ClearAllForcedTargets();
        }

        private void TryForceTarget(Collider2D other)
        {
            if (_owner == null || _owner.IsDead)
            {
                return;
            }

            if (!other.TryGetComponent(out StaticAICore enemy))
            {
                return;
            }

            if (enemy == null || enemy == _owner || enemy.IsDead)
            {
                return;
            }

            if (enemy.gameObject.layer == _owner.gameObject.layer)
            {
                return;
            }

            if (_forcedTargets.TryGetValue(enemy, out ForcedTargetRecord existingRecord))
            {
                existingRecord.ReleaseTime = Time.time + _forceTargetDuration;
                _forcedTargets[enemy] = existingRecord;
                enemy.Target = _owner.transform;
                return;
            }

            ForcedTargetRecord newRecord = new ForcedTargetRecord
            {
                PreviousTarget = enemy.Target,
                ForcedTarget = _owner.transform,
                ReleaseTime = Time.time + _forceTargetDuration
            };

            _forcedTargets.Add(enemy, newRecord);
            enemy.Target = _owner.transform;

            if (_hitVfxPrefab != null)
            {
                Instantiate(_hitVfxPrefab, enemy.transform.position + _hitVfxOffset, Quaternion.identity);
            }

            Debug.Log($"[ForcedTargetZone2D] {enemy.name} is now targeting {_owner.name} for {_forceTargetDuration:F2}s");
        }

        private void ReleaseForcedTarget(StaticAICore enemy, ForcedTargetRecord record)
        {
            if (enemy == null)
            {
                return;
            }

            if (record.ForcedTarget != null && enemy.Target != record.ForcedTarget)
            {
                return;
            }

            if (IsValidTarget(record.PreviousTarget))
            {
                enemy.Target = record.PreviousTarget;
            }
            else
            {
                enemy.Target = null;
            }

            Debug.Log($"[ForcedTargetZone2D] Released forced target on {enemy.name}");
        }

        private void ClearAllForcedTargets()
        {
            foreach (KeyValuePair<StaticAICore, ForcedTargetRecord> pair in _forcedTargets)
            {
                ReleaseForcedTarget(pair.Key, pair.Value);
            }

            _forcedTargets.Clear();
            _releaseBuffer.Clear();
        }

        private bool IsValidTarget(Transform target)
        {
            if (target == null)
            {
                return false;
            }

            if (!target.TryGetComponent(out StaticAICore targetAi))
            {
                return false;
            }

            if (targetAi.IsDead)
            {
                return false;
            }

            if (_owner != null && targetAi.gameObject.layer == _owner.gameObject.layer)
            {
                return false;
            }

            return true;
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            Collider2D zoneCollider = GetComponent<Collider2D>();
            if (zoneCollider == null)
            {
                return;
            }

            Gizmos.color = new Color(1f, 0.4f, 0.1f, 0.9f);
            Gizmos.matrix = transform.localToWorldMatrix;

            switch (zoneCollider)
            {
                case BoxCollider2D boxCollider:
                    Gizmos.DrawWireCube(boxCollider.offset, boxCollider.size);
                    break;
                case CircleCollider2D circleCollider:
                    Gizmos.DrawWireSphere(circleCollider.offset, circleCollider.radius);
                    break;
                case CapsuleCollider2D capsuleCollider:
                    Gizmos.DrawWireCube(capsuleCollider.offset, capsuleCollider.size);
                    break;
            }
        }
#endif
    }
}
