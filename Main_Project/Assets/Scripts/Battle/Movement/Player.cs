using UnityEngine;

namespace Character.Movement
{
    public class Player : MonoBehaviour
    {
        public int strategyType = 1; // 기본값: 가장 가까운 적 타겟팅
        private TargetingSystem targetingSystem;

        private void Awake()
        {
            targetingSystem = GetComponent<TargetingSystem>();
            
            if (targetingSystem == null)
            {
                Debug.LogError("TargetingSystem을 찾을 수 없음");
            }
        }

        private void Start()
        {
            if (targetingSystem != null)
            {
                targetingSystem.StartTargeting();
            }
        }
    }
}