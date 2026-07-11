using System.Collections.Generic;
using BattleK.Scripts.AI.Skill.Base.Logic.LogicBase;
using UnityEngine;

namespace BattleK.Scripts.AI.Skill.Base.Logic.ExecuteLogic
{
    [RequireComponent(typeof(Collider2D))]
    public class ColliderLogicHandler : LogicProcessor
    {
        [SerializeField] private bool _isContinuous = true;
        [SerializeField] private bool _hitSameTargetOnlyOnce = true;

        private readonly Collider2D[] _results = new Collider2D[32];
        private readonly HashSet<StaticAICore> _hitTargets = new();
        private Collider2D _collider;
        private ContactFilter2D _filter;

        private void Awake()
        {
            _collider = GetComponent<Collider2D>();
            _collider.isTrigger = true;
        }

        public override void StartProcess()
        {
            _filter = new ContactFilter2D
            {
                useLayerMask = true,
                layerMask = _targetMask,
                useTriggers = true
            };

            DetectAndApply();
        }

        private void Update()
        {
            if (_isContinuous)
            {
                DetectAndApply();
            }
        }

        private void DetectAndApply()
        {
            if (!_collider || !_owner) return;

            var count = _collider.OverlapCollider(_filter, _results);
            for (var i = 0; i < count; i++)
            {
                var col = _results[i];
                if (!col) continue;

                if (!col.TryGetComponent(out StaticAICore target))
                {
                    target = col.GetComponentInParent<StaticAICore>();
                }

                if (!target || target == _owner) continue;
                if (_hitSameTargetOnlyOnce && _hitTargets.Contains(target)) continue;

                _hitTargets.Add(target);
                ApplyLogicsToTarget(target);
            }
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            var targetCollider = GetComponent<Collider2D>();
            if (!targetCollider) return;

            Gizmos.color = new Color(1f, 0.45f, 0f, 0.9f);
            Gizmos.matrix = transform.localToWorldMatrix;

            switch (targetCollider)
            {
                case BoxCollider2D box:
                    Gizmos.DrawWireCube(box.offset, box.size);
                    break;
                case CircleCollider2D circle:
                    Gizmos.DrawWireSphere(circle.offset, circle.radius);
                    break;
                case CapsuleCollider2D capsule:
                    Gizmos.DrawWireCube(capsule.offset, capsule.size);
                    break;
                default:
                    Gizmos.DrawWireCube(Vector3.zero, targetCollider.bounds.size);
                    break;
            }
        }
#endif
    }
}
