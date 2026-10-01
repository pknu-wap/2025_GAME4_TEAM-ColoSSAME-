using System.Collections.Generic;
using BattleK.Scripts.AI.Skill.Base;
using BattleK.Scripts.Data.Stat;

namespace TeamManage
{
    public enum SkillTier
    {
        Tier3 = 0,
        Tier2 = 1,
        Ultimate = 2,
        Unknown = 3
    }
    public readonly struct SkillEntryData
    {
        public readonly string SkillName;
        public readonly SkillSO Skill;
        public readonly SkillTier Tier;
        public readonly int Level;
        public readonly bool Equipped;

        public string DisplayName =>
            Skill != null && !string.IsNullOrEmpty(Skill.SkillName) ? Skill.SkillName : SkillName;

        public SkillEntryData(string skillName, SkillSO skill, SkillTier tier, int level, bool equipped)
        {
            SkillName = skillName;
            Skill = skill;
            Tier = tier;
            Level = level;
            Equipped = equipped;
        }
    }
    
    public static class SkillCatalog
    {
        public static string GetTierLabel(SkillTier tier)
        {
            switch (tier)
            {
                case SkillTier.Tier3: return "티어 3";
                case SkillTier.Tier2: return "티어 2";
                case SkillTier.Ultimate: return "궁극기";
                default: return string.Empty;
            }
        }

        public static SkillTier GetTierByPoolIndex(int index)
        {
            if (index == 0 || index == 1) return SkillTier.Tier3;
            if (index == 2 || index == 3) return SkillTier.Tier2;
            if (index == 4) return SkillTier.Ultimate;
            return SkillTier.Unknown;
        }

        public static SkillSO FindSkill(RandomSkillGrantA grant, Unit unit, string skillName, out SkillTier tier)
        {
            tier = SkillTier.Unknown;
            if (grant == null || unit == null || string.IsNullOrEmpty(skillName)) return null;

            List<SkillSO> pool = grant.GetAllSkills(unit.UnitClass);
            for (int i = 0; i < pool.Count; i++)
            {
                if (pool[i] != null && pool[i].name == skillName)
                {
                    tier = GetTierByPoolIndex(i);
                    return pool[i];
                }
            }

            return null;
        }

        public static List<SkillEntryData> BuildOwnedEntries(RandomSkillGrantA grant, Unit unit)
        {
            var result = new List<SkillEntryData>();
            if (unit?.OwnedSkills == null) return result;

            List<string> equippedNames = EquipmentService.GetEquippedSkillNames(unit);
            var sortKeys = new List<int>();

            foreach (UnitSkill owned in unit.OwnedSkills)
            {
                if (owned == null || string.IsNullOrEmpty(owned.skillName)) continue;

                SkillSO skill = FindSkill(grant, unit, owned.skillName, out SkillTier tier);
                bool equipped = equippedNames.Contains(owned.skillName);

                int key = (int)tier * 1000 + result.Count;
                int insertAt = sortKeys.FindIndex(k => k > key);
                if (insertAt < 0) insertAt = sortKeys.Count;

                sortKeys.Insert(insertAt, key);
                result.Insert(insertAt, new SkillEntryData(owned.skillName, skill, tier, owned.level, equipped));
            }

            return result;
        }
    }
}
