using BattleK.Scripts.Data.ClassInfo;
using UnityEngine;

namespace BattleK.Scripts.Data.Stat
{
    [System.Serializable]
    public class UnitBaseStat
    {
        public string UnitId, UnitName;
        public UnitClass UnitClass;
        public int Level, Rarity;

        public int BaseAtk, BaseDef, BaseHp, BaseAgi;
        public float BaseEvasionRate;

        public InjuryStatus CurrentInjury;
        
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