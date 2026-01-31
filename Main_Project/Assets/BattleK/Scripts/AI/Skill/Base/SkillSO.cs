using UnityEngine;

namespace BattleK.Scripts.AI.Skill.Base
{
    public abstract class SkillSO : ScriptableObject
    {
        [Header("Basic Settings")]
        public string SkillName;
        public float Cooldown;
    
        [Header("Combat Config")]
        public int Damage;
        public float WindupTime;
        public float ActiveTime;
        public float RecoveryTime;
        public float Range;

        [Header("Animation Config")]
        public int AnimationIndex;

        public abstract void ExecuteSkill(StaticAICore owner, Transform target);
    }
}