using System.Collections.Generic;
using System.Linq;
using BattleK.Scripts.AI.Skill.Base;
using UnityEngine;

namespace BattleK.Scripts.Data.ClassInfo
{
    public enum UnitClass
    {
        Swordsman,
        Archer,
        Mage,
        Axeman,
        Spearman,
        Thief,
        Priest,
        Shieldman
    }

    public enum UnitAttackDelay
    {
        Swordsman = 25,
        Archer = 50,
        Mage = 25,
        Axeman = 43,
        Spearman = 30,
        Thief = 25
    }

    public enum TargetStrategy
    {
        NearestTarget,
        NearestTargetWithClass
    }

    [System.Serializable]
    public class UnitStat
    {
        public string Name;
        public Sprite CharacterImage;
    
        [Header("클래스")]
        public UnitClass UnitClass;
        public bool IsRanged;

        [Header("시야 범위")]
        public float SightRange;
    
        [Header("스킬")]
        public List<SkillSO> EquippedSkills;
        public List<SkillSO> AllPossibleSkills;
    
        [Header("능력치")]
        public int MaxHP;
        public int CurrentHP;
        public int AttackDamage;
        public int SkillPoint;
        public float AttackSpeed;
        public float AttackRange;
        public float AttackDelay;
        public int Defense;
        public float MoveSpeed;
        public float EvasionRate;
        
        public void LoadEquipped(UnitStat unit, List<string> savedIds)
        {
            unit.EquippedSkills = unit.AllPossibleSkills
                .Where(s => savedIds.Contains(s.SkillName))
                .ToList();
        }
    }
}