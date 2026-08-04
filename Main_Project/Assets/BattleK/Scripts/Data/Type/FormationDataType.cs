using System;
using BattleK.Scripts.UI;
using UnityEngine;

namespace BattleK.Scripts.Data.Type
{
    [Serializable]
    public struct UnitSpawnRequest
    {
        public string logicalKey;
        public Vector3 startPos;
        public Vector3 endPos;
        public bool faceLeft;
        public float duration;
        public bool isPlayer;
        public Slot sourceSlot;
        public UIDrag sourceOccupant;
    }
    
    [Serializable]
    public struct FormationAnimConfig {
        public bool useAnim;
        public Vector3 startCenter;
        public Vector3 endCenter;
        public float travelTime;
    }
}