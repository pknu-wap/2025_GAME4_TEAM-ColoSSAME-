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
                Debug.LogError("❌ TargetingSystem을 찾을 수 없습니다! 타겟팅이 비활성화될 수 있습니다.");
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