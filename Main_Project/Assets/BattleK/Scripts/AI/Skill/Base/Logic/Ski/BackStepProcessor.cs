using System.Collections;
using BattleK.Scripts.AI.Skill.Base.Logic.LogicBase;
using UnityEngine;

namespace BattleK.Scripts.AI.Skill.Base.Logic.ExecuteLogic
{
    public class BackStepProcessor : LogicProcessor
    {
        [SerializeField] private float BackDistance = 10f;
        [SerializeField, Min(0.01f)] private float BackStepDuration = 0.2f;
        [SerializeField] private bool _applyLogicsToTarget = true;

        public override void StartProcess()
        {
            if (!_owner || !_targetTransform)
            {
                return;
            }

            Vector2 dir = (_owner.transform.position - _targetTransform.position).normalized;
            if (dir == Vector2.zero)
            {
                dir = _owner.transform.localScale.x < 0f ? Vector2.left : Vector2.right;
            }

            _owner.StartCoroutine(BackStepRoutine(dir));

            if (!_applyLogicsToTarget)
            {
                return;
            }

            if (_targetTransform.TryGetComponent(out StaticAICore targetCore))
            {
                ApplyLogicsToTarget(targetCore);
            }
        }

        private IEnumerator BackStepRoutine(Vector2 direction)
        {
            _owner.StopMovement();

            var startPosition = _owner.transform.position;
            var endPosition = startPosition + (Vector3)(direction * BackDistance);
            var elapsed = 0f;

            while (elapsed < BackStepDuration && _owner)
            {
                var t = Mathf.Clamp01(elapsed / BackStepDuration);
                var easedT = 1f - Mathf.Pow(1f - t, 2f);
                _owner.transform.position = Vector3.Lerp(startPosition, endPosition, easedT);

                if (_owner.Rigidbody)
                {
                    _owner.Rigidbody.velocity = Vector2.zero;
                }

                elapsed += Time.deltaTime;
                yield return null;
            }

            if (!_owner)
            {
                yield break;
            }

            _owner.transform.position = endPosition;
            if (_owner.Rigidbody)
            {
                _owner.Rigidbody.velocity = Vector2.zero;
            }
        }
    }
}
