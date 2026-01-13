using System;
using UnityEngine;

namespace BattleK.Scripts.Data.Type
{
    [Serializable]
    public struct UnitSpawnRequest
    {
        public string logicalKey;       // 데이터 키 (Archer)
        public Vector3 startPos;        // 시작 위치
        public Vector3 endPos;          // 끝 위치
        public bool faceLeft;           // 바라보는 방향
        public float duration;          // 이동 시간
        public bool isPlayer;           // 진영
    }
    
    [Serializable]
    public struct FormationAnimConfig {
        public bool useAnim;
        public Vector3 startCenter;
        public Vector3 endCenter;
        public float travelTime;
    }
}