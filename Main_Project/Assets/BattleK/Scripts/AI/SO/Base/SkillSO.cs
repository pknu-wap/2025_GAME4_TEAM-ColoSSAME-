using UnityEngine;

namespace BattleK.Scripts.AI.SO.Base
{
    [CreateAssetMenu(fileName = "New Skill", menuName = "BattleK/Skills")]
    public class SkillSO : ScriptableObject
    {
        [Header("이름")]
        public string SkillName;
        
        [Header("Combat Config")]
        public int Damage;
        public float WindupTime;
        public float ActiveTime;
        public float RecoveryTime;
        public float Range;

        [Header("Animation Config")]
        public int AnimationIndex;
        public int SkillPrefabAnimationIndex;
        
        [Header("Crowd Control")]
        public bool HasCC;
        public CCProfileSO CcProfile;
    }
}