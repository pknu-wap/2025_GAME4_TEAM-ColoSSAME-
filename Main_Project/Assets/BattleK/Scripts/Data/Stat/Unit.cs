using System.Collections.Generic;
using BattleK.Scripts.Data.ClassInfo;

namespace BattleK.Scripts.Data.Stat
{
    public class Unit
    {
        public string Id;
        public string UnitName;
        public int Tier;
        public readonly UnitClass UnitClass;
        
        public int Level = 1;
        public float EXP = 0;
        public float bonusSuccessRarity = 0f;
        
        public InjuryStatus currentInjury = InjuryStatus.Healthy;

        public List<UnitSkill> EquippedSkills = new();
        public List<UnitSkill> OwnedSkills = new();
        
        public Unit(string id, int tier, string unitName, UnitClass unitClass)
        {
            this.Id = id;
            this.Tier = tier;
            this.UnitName = unitName;
            this.UnitClass = unitClass;
        }
    }
}
