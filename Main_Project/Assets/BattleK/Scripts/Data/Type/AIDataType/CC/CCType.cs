using UnityEngine;

namespace BattleK.Scripts.Data.Type.AIDataType.CC
{

    [System.Serializable]
    public class CCData
    {
        [Header("Basic Info")]
        public string ccName;
        public CCType ccType;
        public float duration;
        public GameObject vfxPrefab;
        
        [Header("Hard CC Settings")]
        public bool isHardCC;
        public PlayerState animName;

        [Header("DoT Settings (Optional)")]
        public bool isDoT;
        public float damagePerTick;
        public float tickInterval;

        [Header("Stat Modifier (Optional)")]
        public float speedMultiplier = 1.0f;
    }
    
    public enum CCType
    {
        None,
        Stun,
        Freeze,
        Burn,
        Poison,
        Slow
    }
}
