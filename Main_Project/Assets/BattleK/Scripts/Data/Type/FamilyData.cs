using System.Collections.Generic;

namespace BattleK.Scripts.Data.Type
{
    [System.Serializable]
    public class CharacterStatsRow
    {
        public string Unit_ID;
        public string Unit_Name;
        public int ATK, DEF, HP, AGI;
        public int Rarity;
        public int Level;
    }
    public class FamilyJson
    {
        public List<FamilyCharacter> Characters;
    }
    public class FamilyCharacter
    {
        public string Unit_ID;
        public string Unit_Name;
        public int Rarity;
        public int Level;
        public StatDistribution Stat_Distribution;
    }
    public class StatDistribution
    {
        public int ATK;
        public int DEF;
        public int HP;
        public int AGI;
    }

    public enum FamilyName
    {
        Astra,
        
    }
}