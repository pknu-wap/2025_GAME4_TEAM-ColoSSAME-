using System.Collections.Generic;
using System.Linq;
using BattleK.Scripts.AI.Skill.Base;
using BattleK.Scripts.Data.Stat;
using UnityEngine;
using UnityEngine.Serialization;

namespace BattleK.Scripts.Data.ClassInfo
{
    [System.Serializable]
    public class UnitRuntimeStat
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
        [FormerlySerializedAs("Skills")]
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
        
        public void LoadEquipped(UnitRuntimeStat unitRuntime, List<UnitSkill> savedIds)
        {
            if (unitRuntime.AllPossibleSkills == null || unitRuntime.AllPossibleSkills.Count == 0)
            {
                unitRuntime.EquippedSkills = new List<SkillSO>();
                return;
            }

            var ids = savedIds ?? new List<UnitSkill>();
            var equippedNames = new HashSet<string>(ids.Select(u => u.skillName));

            unitRuntime.EquippedSkills = unitRuntime.AllPossibleSkills
                .Where(s => equippedNames.Contains(s.SkillName))
                .ToList();
        }

        public void SaveTo(Unit unit)
        {
            unit.currentInjury = InjuryLevel;
            unit.equippedItemId = Item != null ? Item.id : -1;
            unit.EquippedSkills = EquippedSkills?.Select(s => new UnitSkill(s.SkillName, s.SkillLevel)).ToList() ?? new List<UnitSkill>();
        }

        public void LoadFrom(Unit unit, ItemDatabase itemDb)
        {
            if (unit == null) return;
            InjuryLevel = unit.currentInjury;
            Item = itemDb?.GetById(unit.equippedItemId);
            LoadEquipped(this, unit.EquippedSkills);
        }
    }
}
