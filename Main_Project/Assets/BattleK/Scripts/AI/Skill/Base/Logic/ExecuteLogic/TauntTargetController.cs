using UnityEngine;

namespace BattleK.Scripts.AI.Skill.Base.Logic.ExecuteLogic
{
    public sealed class TauntTargetController : MonoBehaviour
    {
        private StaticAICore _target;
        private StaticAICore _owner;
        private Transform _previousTarget;
        private float _releaseTime;
        private bool _restorePreviousTarget;
        private bool _isReleasing;

        private void Awake()
        {
            _target = GetComponent<StaticAICore>();
        }

        public void Apply(StaticAICore owner, float duration, bool restorePreviousTarget, bool refreshDuration)
        {
            if (!_target || !owner) return;

            var isNewTaunt = _owner == null || _target.Target != _owner.transform;
            if (isNewTaunt)
            {
                _previousTarget = _target.Target;
            }
            else if (!refreshDuration)
            {
                return;
            }

            _owner = owner;
            _restorePreviousTarget = restorePreviousTarget;
            _releaseTime = Time.time + Mathf.Max(0f, duration);
            _target.Target = _owner.transform;
        }

        private void Update()
        {
            if (!_target)
            {
                Destroy(this);
                return;
            }

            if (_owner == null || _owner.IsDead || _target.IsDead || Time.time >= _releaseTime)
            {
                Release(true);
            }
        }

        private void OnDisable()
        {
            Release(false);
        }

        private void Release(bool destroyAfterRelease)
        {
            if (_isReleasing) return;
            _isReleasing = true;

            if (_target && _owner && _target.Target == _owner.transform)
            {
                _target.Target = _restorePreviousTarget && IsValidPreviousTarget()
                    ? _previousTarget
                    : null;
            }

            _owner = null;
            _previousTarget = null;
            _isReleasing = false;

            if (destroyAfterRelease)
            {
                Destroy(this);
            }
        }

        private bool IsValidPreviousTarget()
        {
            if (!_previousTarget) return false;
            if (!_previousTarget.TryGetComponent(out StaticAICore previousCore)) return false;
            if (previousCore.IsDead) return false;
            return _target && previousCore.gameObject.layer != _target.gameObject.layer;
        }
    }
}
