using UnityEngine;

namespace BattleK.Scripts.AI.Skill.Base
{
    public abstract class SkillSO : ScriptableObject
    {
        [Header("Basic Settings")]
        public string SkillName;
        public int InternalPriority;
    
        [Header("Skill Perfab Settings")]
        public GameObject SkillPrefab;
        public float SkillAnimDuration;
        
        [Header("Combat Config")]
        public int Damage;
        public float WindupTime;
        public float ActiveTime;
        public float RecoveryTime;
        public float Cooldown;
        public Vector2 SkillArea;
        
        [Header("CC Config")]
        public CCProfileSO CCProfile;
        
        public enum SpawnPosition { Owner, Target }
        public SpawnPosition SpawnAt;

        [Header("Animation Config")]
        public int AnimationIndex;

        public abstract void ExecuteSkill(StaticAICore owner, Transform target);
        public abstract bool IsInArea(Transform owner, Transform target);
        protected Collider2D[] GetTargetsInArea(StaticAICore owner, Vector2 centerOffset, Vector2 size)
        {
            var worldCenter = owner.transform.TransformPoint(centerOffset);
            return Physics2D.OverlapBoxAll(worldCenter, size * 0.5f, owner.transform.eulerAngles.z, owner.TargetLayer);
        }
    }
}