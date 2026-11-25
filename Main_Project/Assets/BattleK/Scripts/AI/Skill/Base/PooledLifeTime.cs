using System;
using BattleK.Scripts.Manager;
using UnityEngine;

namespace BattleK.Scripts.AI.Skill.Base
{
    public class PooledLifetime : MonoBehaviour
    {
        private Action _beforeReturn;
        private bool _returnScheduled;

        public void ReturnAfter(float delay)
        {
            ReturnAfter(delay, null);
        }

        internal void ReturnAfter(float delay, Action beforeReturn)
        {
            CancelInvoke(nameof(ReturnNow));
            _beforeReturn = beforeReturn;
            _returnScheduled = true;
            Invoke(nameof(ReturnNow), delay);
        }

        internal void ReturnNow()
        {
            if (!_returnScheduled) return;

            CancelInvoke(nameof(ReturnNow));
            _returnScheduled = false;
            var beforeReturn = _beforeReturn;
            _beforeReturn = null;
            beforeReturn?.Invoke();

            if (PrefabPoolManager.Instance)
                PrefabPoolManager.Instance.Release(gameObject);
            else
                Destroy(gameObject);
        }
    }
}
