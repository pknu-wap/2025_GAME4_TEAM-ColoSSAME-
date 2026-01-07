using System.Collections.Generic;

namespace BattleK.Scripts.Data
{
    public class CharacterStatsRow
    {
        public string Unit_ID;
        public string Unit_Name;
        public int ATK, DEF, HP, AGI;
        public float APS;
        public int Rarity;
        public int Level;
    }
    public class FamilyJson
    {
        public string Family_Name;
        public string Family_Description;
        public int id;
        public List<FamilyCharacter> Characters;
    }
    public class FamilyCharacter
    {
        public string Unit_ID;
        public string Unit_Name;
        public int Rarity;
        public int Level;
        public string Class;
        public string Description;
        public string Story;
        public StatDistribution Stat_Distribution;
        public FamilyVisuals Visuals;
        public int LV;
        public int EXP;
    }
    public class StatDistribution
    {
        public int ATK;
        public int DEF;
        public int HP;
        public int AGI;
    }
    public class FamilyVisuals
    {
        public string Portrait;
        public string Prefab;
    }
}