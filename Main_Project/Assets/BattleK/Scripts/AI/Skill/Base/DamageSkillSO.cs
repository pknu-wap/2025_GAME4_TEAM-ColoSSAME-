using System;
using BattleK.Scripts.Data.Type.AIDataType.CC;
using BattleK.Scripts.Manager.Battle;
using UnityEngine;

namespace BattleK.Scripts.AI.Skill.Base
{
    [CreateAssetMenu(fileName = "RectDamageSkillSO", menuName = "BattleK/RectDamageSkillSO")]
    public class RectDamageSkillSO : SkillSO
    {
        public override bool IsInArea(Transform owner, Transform target)
        {
            var rel = owner.InverseTransformPoint(target.position);
            return rel.z >= 0 && rel.z <= SkillArea.y && Mathf.Abs(rel.x) <= SkillArea.x * 0.5f;
        }

        public override void ExecuteSkill(StaticAICore owner, Transform target)
        {
            var instance = SpawnAt switch
            {
                SpawnPosition.Owner => Instantiate(SkillPrefab, owner.transform),
                SpawnPosition.Target => Instantiate(SkillPrefab, target.transform),
                _ => throw new ArgumentOutOfRangeException()
            };
            
            var center = new Vector3(0, 0, SkillArea.y * 0.5f);
            var halfExtents = new Vector3(SkillArea.x, 1f, SkillArea.y);
            var hitTargets = GetTargetsInArea(owner, center, halfExtents);
            
            foreach (var col in hitTargets)
            {
                if (!col.TryGetComponent(out StaticAICore targetAI)) continue;
                targetAI.OnTakeDamage(Damage);
                    
                if (CCProfile && targetAI.TryGetComponent(out CCManager ccManager))
                {
                    ccManager.ApplyCC(CCProfile.Data);
                }
            }
            if (instance.TryGetComponent(out AutoDestroy ad))
            {
                ad.Duration = SkillAnimDuration;
            }
            else
            {
                Destroy(instance, SkillAnimDuration);
            }
        }
    }
}
