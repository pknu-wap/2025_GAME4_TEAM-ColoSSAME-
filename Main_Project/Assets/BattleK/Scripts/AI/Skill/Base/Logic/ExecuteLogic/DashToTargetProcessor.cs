using System.Collections;
using BattleK.Scripts.AI.Skill.Base.Logic.LogicBase;
using UnityEngine;

namespace BattleK.Scripts.AI.Skill.Base.Logic.ExecuteLogic
{
    public class DashToTargetProcessor : LogicProcessor
    {
        [SerializeField, Min(0.01f)] private float _dashSpeed = 12f;
        [SerializeField, Min(0f)] private float _stopDistance = 0.75f;
        [SerializeField, Min(0.01f)] private float _maxDashTime = 0.35f;
        [SerializeField] private bool _applyOnTimeout = true;

        private bool _hasApplied;

        public override void StartProcess()
        {
            if (!_owner || !_targetTransform) return;

            StartCoroutine(DashRoutine());
        }

        private IEnumerator DashRoutine()
        {
            StaticAICore targetCore = GetTargetCore();
            if (!targetCore) yield break;

            if (_owner.AiPath)
            {
                _owner.AiPath.isStopped = true;
                _owner.AiPath.destination = _owner.transform.position;
            }

            StopOwnerVelocity();

            var elapsed = 0f;
            while (elapsed < _maxDashTime && _owner && targetCore && !targetCore.IsDead)
            {
                Vector3 ownerPosition = _owner.transform.position;
                Vector3 targetPosition = _targetTransform.position;
                Vector3 toTarget = targetPosition - ownerPosition;
                toTarget.z = 0f;

                if (toTarget.magnitude <= _stopDistance)
                {
                    break;
                }

                _owner.LookAt(targetPosition);

                Vector3 nextPosition = Vector3.MoveTowards(
                    ownerPosition,
                    targetPosition,
                    _dashSpeed * Time.deltaTime
                );
                nextPosition.z = ownerPosition.z;

                _owner.transform.position = nextPosition;
                transform.position = nextPosition;
                StopOwnerVelocity();

                elapsed += Time.deltaTime;
                yield return null;
            }

            if (!_owner || !targetCore || targetCore.IsDead) yield break;

            float finalDistance = Vector2.Distance(_owner.transform.position, targetCore.transform.position);
            if (finalDistance <= _stopDistance || _applyOnTimeout)
            {
                ApplyOnce(targetCore);
            }
        }

        private StaticAICore GetTargetCore()
        {
            if (!_targetTransform) return null;

            if (_targetTransform.TryGetComponent(out StaticAICore targetCore))
            {
                return targetCore;
            }

            return _targetTransform.GetComponentInParent<StaticAICore>();
        }

        private void StopOwnerVelocity()
        {
            if (_owner && _owner.Rigidbody)
            {
                _owner.Rigidbody.velocity = Vector2.zero;
            }
        }

        private void ApplyOnce(StaticAICore target)
        {
            if (_hasApplied) return;

            _hasApplied = true;
            ApplyLogicsToTarget(target);
        }
    }
}
