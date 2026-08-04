using BattleK.Scripts.AI.Skill.Base.Logic.LogicBase;
using UnityEngine;
using System.Collections.Generic;

namespace BattleK.Scripts.AI.Skill.Base.Logic.ColliderLogics
{
    public class BoxLogicHandler : LogicProcessor
    {
        [Header("Hit Box Settings")]
        [SerializeField] private Vector2 _areaSize = new Vector2(2f, 2f);
        [SerializeField] private Vector2 _offset = Vector2.zero;

        [Header("Hit Option")]
        [SerializeField] private bool _isContinuous = true;

        private readonly Collider2D[] _results = new Collider2D[20];
        private readonly HashSet<StaticAICore> _hitTargets = new();
        private readonly List<StaticAICore> _detectedTargets = new();

        public override void StartProcess()
        {
            this.DetectAndApply();
        }

        private void Update()
        {
            if (this._isContinuous)
            {
                this.DetectAndApply();
            }
        }

        private void DetectAndApply()
        {
            if (this._maxHitTargets > 0 && this._hitTargets.Count >= this._maxHitTargets)
            {
                return;
            }

            Vector2 center = (Vector2)this.transform.position + (Vector2)(this.transform.rotation * (Vector3)this._offset);

            int count = Physics2D.OverlapBoxNonAlloc(
                center,
                this._areaSize,
                this.transform.eulerAngles.z,
                this._results,
                this._owner.TargetLayer
            );

            this.CollectTargets(count);

            if (this._maxHitTargets > 0)
            {
                this.SortTargetsByDistance(center);
            }

            this.ApplyTargets();
        }

        private void CollectTargets(int count)
        {
            this._detectedTargets.Clear();

            for (int i = 0; i < count; i++)
            {
                StaticAICore target = this.GetTargetFromCollider(this._results[i]);

                if (target == null) continue;
                if (target == this._owner) continue;
                if (this._hitTargets.Contains(target)) continue;
                if (this._detectedTargets.Contains(target)) continue;

                this._detectedTargets.Add(target);
            }
        }

        private void SortTargetsByDistance(Vector2 center)
        {
            this._detectedTargets.Sort((a, b) =>
            {
                float aDistance = ((Vector2)a.transform.position - center).sqrMagnitude;
                float bDistance = ((Vector2)b.transform.position - center).sqrMagnitude;
                return aDistance.CompareTo(bDistance);
            });
        }

        private void ApplyTargets()
        {
            foreach (StaticAICore target in this._detectedTargets)
            {
                if (this._maxHitTargets > 0 && this._hitTargets.Count >= this._maxHitTargets)
                {
                    break;
                }

                this._hitTargets.Add(target);
                this.ApplyLogicsToTarget(target);
            }
        }

        private StaticAICore GetTargetFromCollider(Collider2D col)
        {
            if (col == null) return null;

            if (col.TryGetComponent(out StaticAICore target))
            {
                return target;
            }

            return col.GetComponentInParent<StaticAICore>();
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;

            Vector2 center = (Vector2)this.transform.position + (Vector2)(this.transform.rotation * (Vector3)this._offset);

            Gizmos.matrix = Matrix4x4.TRS(
                center,
                this.transform.rotation,
                Vector3.one
            );

            Gizmos.DrawWireCube(Vector3.zero, this._areaSize);
        }
#endif
    }
}
