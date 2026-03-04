using System;
using BattleK.Scripts.AI.Skill.Base.Logic;
using UnityEngine;

namespace BattleK.Scripts.AI.Skill.Base
{
    [CreateAssetMenu(fileName = "RectDamageSkillSO", menuName = "BattleK/RectDamageSkillSO")]
    public class RectDamageSkillSO : SkillSO
    {
        [Header("Skill Type")]
        public bool IsContinuous;

        public override void ExecuteSkill(StaticAICore owner, Transform target)
        {
            var instance = CreateInstance(owner, target);

            if (!instance.TryGetComponent(out BoxLogicHandler handler)) return;
            handler.SetAreaSize(SkillArea);
            handler.Initialize(owner, SkillLogics);
        
            if (!IsContinuous) 
            {
                handler.StartProcess();
            }
        }

        private GameObject CreateInstance(StaticAICore owner, Transform target)
        {
            switch (SpawnAt)
            {
                case SpawnPosition.Owner:
                    return Instantiate(SkillPrefab, owner.transform.position, owner.transform.rotation, owner.transform);
                case SpawnPosition.Target:
                    var dir = (target.position - owner.transform.position).normalized;
                    var angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
                    return Instantiate(SkillPrefab, target.position, Quaternion.Euler(0, 0, angle - 90f));
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public override bool IsInArea(Transform owner, Transform target)
        {
            var rel = owner.InverseTransformPoint(target.position);
            return rel.y >= 0 && rel.y <= SkillArea.y && Mathf.Abs(rel.x) <= SkillArea.x * 0.5f;
        }
        
        public override void DrawGizmos(Transform owner)
        {
            if (!owner) return;
            Gizmos.matrix = owner.localToWorldMatrix;

            var center = new Vector3(0, SkillArea.y * 0.5f, 0);
            var size = new Vector3(SkillArea.x, SkillArea.y, 0.1f);

            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(center, size);
            Gizmos.color = new Color(0, 1, 1, 0.3f);
            Gizmos.DrawCube(center, size);
        }
    }
}