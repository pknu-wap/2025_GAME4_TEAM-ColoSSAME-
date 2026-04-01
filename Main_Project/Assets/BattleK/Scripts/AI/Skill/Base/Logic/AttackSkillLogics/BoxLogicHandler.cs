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
            Vector2 center = (Vector2)this.transform.position + (Vector2)(this.transform.rotation * (Vector3)this._offset);

            int count = Physics2D.OverlapBoxNonAlloc(
                center,
                this._areaSize,
                this.transform.eulerAngles.z,
                this._results,
                this._owner.TargetLayer
            );

            for (int i = 0; i < count; i++)
            {
                Collider2D col = this._results[i];

                StaticAICore target = null;

                if (!col.TryGetComponent(out target))
                {
                    target = col.GetComponentInParent<StaticAICore>();
                }

                if (target == null) continue;
                if (target == this._owner) continue;
                if (this._hitTargets.Contains(target)) continue;

                this._hitTargets.Add(target);
                this.ApplyLogicsToTarget(target);
            }
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