using System;
using UnityEngine;
namespace BattleK.Scripts.Manager {
    [DisallowMultipleComponent]
    public class StatsReady : MonoBehaviour
    {
        private bool IsReady { get; set; }
        public event Action OnReady;
        public void MarkReady()
        {
            if (IsReady) return;
            IsReady = true;
            OnReady?.Invoke();
            OnReady = null;
        }
    }
}