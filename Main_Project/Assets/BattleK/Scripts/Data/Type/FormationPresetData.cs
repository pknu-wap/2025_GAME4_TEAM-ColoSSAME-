using System;

namespace BattleK.Scripts.Data.Type
{
    [Serializable]
    public class FormationPresetData
    {
        public string formationId;
        public int saveSlotNumber;
        public string[] unitKeysBySlot;
    }
}