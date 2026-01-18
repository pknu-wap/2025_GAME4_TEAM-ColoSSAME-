using UnityEngine;

namespace BattleK.Scripts.AI.SO.Base
{
    [CreateAssetMenu(fileName = "New Skill", menuName = "BattleK/Skills")]
    public class SkillSO : ScriptableObject
    {
        public string SkillName;
        public float Damage;
    
        [Header("Crowd Control")]
        public bool HasCC; // CC기가 있는 스킬인가?
        public CCProfileSO CcProfile; // 여기에 "StunProfile" 에셋을 드래그앤드롭
    }
}