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
            return tier switch
            {
                SkillTier.Tier3 => "티어 3",
                SkillTier.Tier2 => "티어 2",
                SkillTier.Ultimate => "궁극기",
                SkillTier.Unknown => string.Empty,
                _ => string.Empty
            };
        }

        private static SkillTier GetTierByPoolIndex(int index)
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
            return FindSkill(pool, skillName, out tier);
        }

        private static SkillSO FindSkill(List<SkillSO> pool, string skillName, out SkillTier tier)
        {
            tier = SkillTier.Unknown;
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
            List<SkillSO> pool = null;
            bool poolLoaded = false;

            foreach (UnitSkill owned in unit.OwnedSkills)
            {
                if (owned == null || string.IsNullOrEmpty(owned.skillName)) continue;

                if (grant != null && !poolLoaded)
                {
                    pool = grant.GetAllSkills(unit.UnitClass);
                    poolLoaded = true;
                }

                SkillTier tier = SkillTier.Unknown;
                SkillSO skill = grant != null ? FindSkill(pool, owned.skillName, out tier) : null;
                bool equipped = equippedNames.Contains(owned.skillName);

                int insertAt = 0;
                while (insertAt < result.Count && result[insertAt].Tier.CompareTo(tier) <= 0)
                    insertAt++;
                result.Insert(insertAt, new SkillEntryData(owned.skillName, skill, tier, owned.level, equipped));
            }

            return result;
        }
    }
}
