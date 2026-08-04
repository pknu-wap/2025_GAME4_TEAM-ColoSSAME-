using UnityEngine;

namespace Colosseum.HealingCenter
{
    public class HealingService
    {
        private static HealingService _instance;
        public static HealingService Instance => _instance ??= new HealingService();

        private HealingService() { }
        
        // TODO: 실제 부상 판정 매니저로 교체 필요. 예상 형태:
        public bool IsInjured(string unitId)
        {
            Debug.LogWarning($"[HealingService] TODO 미연동: IsInjured({unitId})");
            return false;
        }

        /// TODO: 실제 부상 enum → 표시명 매핑으로 교체 필요.
        public string GetInjuryStatusText(string unitId)
        {
            return IsInjured(unitId) ? "부상" : "건강함";
        }

        /// TODO: 부상 정도에 따라 비용이 달라진다면 그 값을 참조하는 로직으로 교체.
        public int GetHealingCost(string unitId)
        {
            return 100; // TODO: 임시 고정값. 실제 기획 값으로 교체 필요.
        }

        // TODO: 실제 부상 회복 API로 교체 필요.
        private void ApplyHeal(string unitId)
        {
            Debug.LogWarning($"[HealingService] TODO 미연동: ApplyHeal({unitId})");
        }

        // TODO: 실제 저장 매니저로 교체 필요.
        private void SaveData()
        {
            Debug.LogWarning("[HealingService] TODO 미연동: SaveData()");
        }
        
        public HealingResult TryHeal(string unitId)
        {
            if (string.IsNullOrEmpty(unitId))
            {
                return new HealingResult(HealingResultType.NoCharacterSelected, unitId);
            }

            if (!IsInjured(unitId))
            {
                return new HealingResult(HealingResultType.NotInjured, unitId);
            }

            int cost = GetHealingCost(unitId);
            if (UserManager.Instance.user.money < cost)
            {
                return new HealingResult(HealingResultType.NotEnoughMoney, unitId);
            }

            UserManager.Instance.SpendGold(cost);
            ApplyHeal(unitId);
            SaveData();

            return new HealingResult(HealingResultType.Success, unitId, cost);
        }
    }
}
