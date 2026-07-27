using System;
using System.Collections.Generic;
using UnityEngine;
using BattleK.Scripts.AI.Skill.Base;
using BattleK.Scripts.Data.ClassInfo;

namespace BattleK.Scripts.Data
{
    [CreateAssetMenu(fileName = "ClassSkillDatabase", menuName = "BattleK/ClassSkillDatabase")]
    public class ClassSkillDatabase : ScriptableObject
    {
        [Serializable]
        public class ClassSkillEntry
        {
            public UnitClass UnitClass;
            public List<SkillSO> AvailableSkills = new();
        }

        public List<ClassSkillEntry> Entries = new();

        public List<SkillSO> GetSkillsForClass(UnitClass unitClass)
        {
            var entry = Entries.Find(e => e.UnitClass == unitClass);
            return entry != null ? entry.AvailableSkills : new List<SkillSO>();
        }
    }
}