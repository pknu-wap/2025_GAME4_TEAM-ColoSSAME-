using BattleK.Scripts.AI.Skill.Base.Logic.LogicBase;
using UnityEngine;

namespace BattleK.Scripts.AI.Skill.Base.Logic.ExecuteLogic
{
    [RequireComponent(typeof(Collider2D))]
    public sealed class NormalAttackProjectileProcessor : LogicProcessor
    {
        private readonly Collider2D[] _overlaps = new Collider2D[8];
        private Collider2D _collider;
        private ContactFilter2D _filter;
        private bool _hasHit;

        public override void StartProcess()
        {
            _hasHit = false;
            _collider = GetComponent<Collider2D>();
            _collider.isTrigger = true;
            _filter = new ContactFilter2D { useLayerMask = true, layerMask = _targetMask, useTriggers = true };
            CheckOverlaps();
        }

        private void FixedUpdate() => CheckOverlaps();

        private void CheckOverlaps()
        {
            if (_hasHit || !_owner || !_collider) return;
            var count = _collider.OverlapCollider(_filter, _overlaps);
            for (var i = 0; i < count && !_hasHit; i++) TryHit(_overlaps[i]);
        }

        private void OnTriggerEnter2D(Collider2D other) => TryHit(other);

        private void TryHit(Collider2D other)
        {
            if (_hasHit || !_owner || !other) return;
            var target = other.GetComponentInParent<StaticAICore>();
            if (!target || target == _owner || !target.gameObject.activeInHierarchy || target.IsDead) return;
            if ((_targetMask.value & (1 << target.gameObject.layer)) == 0) return;

            _hasHit = true;
            ApplyLogicsToTarget(target);
            ReturnToPool();
        }
    }
}
