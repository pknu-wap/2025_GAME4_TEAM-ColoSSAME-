using System.Collections.Generic;
using BattleK.Scripts.AI.Skill.Base.Logic.LogicBase;
using BattleK.Scripts.Utils;
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
        public PlayerState animName;

        [Header("Dynamic Logic Actions")]
        [SerializeReference, SelectableReference]
        public List<ICCAction> Actions = new();

        [Header("Stat Modifier (Optional)")]
        public float tickInterval = 0.5f; 
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
