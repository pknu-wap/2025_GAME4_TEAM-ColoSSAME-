using System;
using BattleK.Scripts.Data.Type.AIDataType.CC;
using BattleK.Scripts.Manager.Battle;
using UnityEngine;

namespace BattleK.Scripts.AI.Skill.Base
{
    [CreateAssetMenu(fileName = "RectDamageSkillSO", menuName = "BattleK/RectDamageSkillSO")]
    public class RectDamageSkillSO : SkillSO
    {
        [Header("Skill Type")]
        public bool IsContinuous;
        
        public override bool IsInArea(Transform owner, Transform target)
        {
            var rel = owner.InverseTransformPoint(target.position);
            return rel.y >= 0 && rel.y <= SkillArea.y && Mathf.Abs(rel.x) <= SkillArea.x * 0.5f;
        }
        
        public void DrawSkillAreaGizmos(Transform owner)
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

        public override void ExecuteSkill(StaticAICore owner, Transform target)
        {
            GameObject instance;

            switch (SpawnAt)
            {
                case SpawnPosition.Owner:
                    instance = Instantiate(SkillPrefab, owner.transform);
                    instance.transform.localPosition = Vector3.zero;
                    instance.transform.localRotation = Quaternion.identity;
                    break;

                case SpawnPosition.Target:
                    var dir = (target.position - owner.transform.position).normalized;
                    var angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
                    instance = Instantiate(SkillPrefab, target.position, Quaternion.Euler(0, 0, angle - 90f));
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
            if (instance.TryGetComponent(out SkillEffect effect))
            {
                var ccData = CCProfile ? CCProfile.Data : null;
                effect.Setup(owner, SkillArea, Damage, IsContinuous, ccData);
            }
            
            if (instance.TryGetComponent(out AutoDestroy ad)) ad.Duration = SkillAnimDuration;
            else Destroy(instance, SkillAnimDuration);
        }
    }
}
