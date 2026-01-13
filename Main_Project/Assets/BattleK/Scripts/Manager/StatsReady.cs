using System;
using UnityEngine;
namespace BattleK.Scripts.Manager {
    [DisallowMultipleComponent]
    public class StatsReady : MonoBehaviour
    {
        /// <summary>StatAdaptManager가 스탯 적용을 끝내면 true</summary>
        public bool IsReady { get; private set; }

        /// <summary>처음 Ready가 될 때 1회만 호출</summary>
        public event Action OnReady;

        /// <summary>스탯 적용이 끝났음을 알림(중복 호출 안전)</summary>
        public void MarkReady()
        {
            if (IsReady) return;
            IsReady = true;
            OnReady?.Invoke();
            OnReady = null; // 1회성 이벤트
        }
    }
}