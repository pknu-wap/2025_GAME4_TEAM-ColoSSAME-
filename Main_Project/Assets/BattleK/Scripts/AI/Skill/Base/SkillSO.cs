using UnityEngine;

namespace BattleK.Scripts.AI.Skill.Base
{
    [CreateAssetMenu(fileName = "SkillProfileSO", menuName = "BattleK/SkillProfileSO")]
    
    public abstract class SkillSO : ScriptableObject
    {
        [Header("Basic Settings")]
        public string SkillName;
        public int InternalPriority;
    
        [Header("Skill Perfab Settings")]
        public GameObject SkillPrefab;
        public enum SpawnPosition { Owner, Target }
        public SpawnPosition SpawnAt;
        
        [Header("Combat Config")]
        public int Damage;
        public float WindupTime;
        public float ActiveTime;
        public float RecoveryTime;
        public float Cooldown;
        public float Range;

        [Header("Animation Config")]
        public int AnimationIndex;

        public abstract void ExecuteSkill(StaticAICore owner, Transform target);
    }
}