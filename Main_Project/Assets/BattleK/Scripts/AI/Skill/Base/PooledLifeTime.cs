using BattleK.Scripts.Manager;
using UnityEngine;

namespace BattleK.Scripts.AI.Skill.Base
{
    public class PooledLifetime : MonoBehaviour
    {
        public void ReturnAfter(float delay)
        {
            CancelInvoke(nameof(Release));
            Invoke(nameof(Release), delay);
        }

        private void Release()
        {
            if (PrefabPoolManager.Instance)
                PrefabPoolManager.Instance.Release(gameObject);
            else
                Destroy(gameObject);
        }
    }
}