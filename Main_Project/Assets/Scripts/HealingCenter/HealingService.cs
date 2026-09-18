using BattleK.Scripts.Data.Stat;

namespace Colosseum.HealingCenter
{
    public class HealingService
    {
        private static HealingService _instance;
        public static HealingService Instance => _instance ??= new HealingService();

        private HealingService() { }

        public bool IsInjured(Unit unit) =>
            unit != null && unit.currentInjury != InjuryStatus.Healthy;

        public string GetInjuryStatusText(Unit unit) =>
            unit == null ? string.Empty : InjuryStatusLocalization.GetDisplayName(unit.currentInjury);

        public int GetHealingCost(Unit unit) =>
            unit == null ? 0 : Heal.GetCost(unit.currentInjury);

        public bool HasEnoughGold(Unit unit) =>
            UserManager.Instance.user.money >= GetHealingCost(unit);

        public HealingResult TryHeal(Unit unit)
        {
            if (!IsInjured(unit))
            {
                return new HealingResult(HealingResultType.NotInjured, unit.Id);
            }

            int cost = GetHealingCost(unit);

            if (!UserManager.Instance.SpendGold(cost))
            {
                return new HealingResult(HealingResultType.NotEnoughMoney, unit.Id);
            }

            Heal.Apply(unit);
            UserManager.Instance.SaveUser();
            return new HealingResult(HealingResultType.Success, unit.Id, cost);
        }
    }
}
