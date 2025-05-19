using System.Collections.Generic;

namespace Battle.Scripts.Value.Data
{
    [System.Serializable]
    public class CharacterData
    {
        public Dictionary<string, CharacterInfo> characters = new();
        public Dictionary<string, PositionData> positions = new();
    }
}