using System.Collections.Generic;
using System.Linq;
using BattleK.Scripts.AI.Skill.Base;
using UnityEngine;
using UnityEngine.Serialization;

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

    public enum InjuryStatus
    {
        Healthy,
        Injury,
        FatalInjury
    }

    [System.Serializable]
    public class UnitStat
    {
        public string Name;
        public Sprite CharacterImage;

        [Header("부상")]
        public InjuryStatus InjuryLevel;
    
        [Header("클래스")]
        public UnitClass UnitClass;
        public bool IsRanged;

        [Header("시야 범위")]
        public float SightRange;
    
        [Header("스킬")]
        public List<SkillSO> EquippedSkills;
        public List<SkillSO> AllPossibleSkills;
        
        [Header("아이템")]
        public ItemData Item;
    
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
            if (unit.AllPossibleSkills == null || unit.AllPossibleSkills.Count == 0)
            {
                unit.EquippedSkills = new List<SkillSO>();
                return;
            }

            var ids = savedIds ?? new List<string>();

            unit.EquippedSkills = unit.AllPossibleSkills
                .Where(s => ids.Contains(s.SkillName))
                .ToList();
        }

        public void SaveTo(Unit unit)
        {
            unit.currentInjury = InjuryLevel;
            // unit.equippedItemId = Item != null ? Item.ItemId : null;
            unit.selectedSkills = EquippedSkills?.Select(s => s.SkillName).ToList() ?? new List<string>();
        }

        public void LoadFrom(Unit unit, ItemDatabase itemDb)
        {
            if (unit == null) return;
            InjuryLevel = unit.currentInjury;
            // Item = string.IsNullOrEmpty(unit.equippedItemId) ? null : itemDb.GetById(unit.equippedItemId);
            LoadEquipped(this, unit.selectedSkills);
        }
    }
}