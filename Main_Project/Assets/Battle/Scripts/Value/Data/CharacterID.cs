using Battle.Scripts.Ai;
using UnityEngine;

namespace Battle.Scripts.Value.Data
{
    public class CharacterID : MonoBehaviour
    {
        public string characterTeamKey;
        public string characterKey; 
        public string GetFullKey() => $"{characterTeamKey}_{characterKey}";
    }
}