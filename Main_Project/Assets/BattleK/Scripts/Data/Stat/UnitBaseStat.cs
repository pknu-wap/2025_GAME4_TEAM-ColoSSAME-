using BattleK.Scripts.Data.ClassInfo;
using UnityEngine;

namespace BattleK.Scripts.Data.Stat
{
    public class UnitBaseStat
    {
        public string UnitId, UnitName;
        public UnitClass UnitClass;
        public int Level, Rarity;

        public int BaseAtk, BaseDef, BaseHp, BaseAgi;
        public float BaseEvasionRate;
        public float BaseAttackSpeed;

        public int BaseSkillPoint;
        public float BaseMoveSpeed;
        public float BaseAttackDelay;

        public InjuryStatus CurrentInjury;
        public static UnitBaseStat FromUnitStat(UnitRuntimeStat runtimeStat, int level, int rarity, int baseAgi, string unitId = null)
        {
            return new UnitBaseStat
            {
                UnitId = unitId ?? runtimeStat.Name,
                UnitName = runtimeStat.Name,
                UnitClass = runtimeStat.UnitClass,
                Level = level,
                Rarity = rarity,
                BaseAtk = runtimeStat.AttackDamage,
                BaseDef = runtimeStat.Defense,
                BaseHp = runtimeStat.MaxHP,
                BaseAgi = baseAgi,
                BaseEvasionRate = runtimeStat.EvasionRate,
                BaseAttackSpeed = runtimeStat.AttackSpeed,
                BaseSkillPoint = runtimeStat.SkillPoint,
                BaseMoveSpeed = runtimeStat.MoveSpeed,
                BaseAttackDelay = runtimeStat.AttackDelay,
                CurrentInjury = runtimeStat.InjuryLevel
            };
        }
        public static UnitBaseStat FromFamilyAndSave(CharacterData family, Unit savedUnit)
        {
            var level = savedUnit != null && savedUnit.Level > 0 ? savedUnit.Level : 1;
            var rarity = savedUnit != null && savedUnit.Tier > 0 ? savedUnit.Tier : Mathf.Max(1, family.Tier);
            var unitClass = savedUnit?.UnitClass ?? family.Class;

            return new UnitBaseStat
            {
                UnitId = family.Unit_ID,
                UnitName = family.Unit_Name,
                UnitClass = unitClass,
                Level = level,
                Rarity = rarity,
                BaseAtk = family.Stat_Distribution?.ATK ?? 0,
                BaseDef = family.Stat_Distribution?.DEF ?? 0,
                BaseHp  = family.Stat_Distribution?.HP  ?? 0,
                BaseAgi = family.Stat_Distribution?.AGI ?? 0,
                CurrentInjury = savedUnit?.currentInjury ?? InjuryStatus.Healthy
            };
        }
    }
}
