using System.Collections.Generic;

namespace BattleK.Scripts.Data.Stat
{
    public enum InjuryStatus
    {
        Healthy,
        Injury,
        FatalInjury
    }
    
    public static class InjuryStatusLocalization
    {
        private static readonly Dictionary<InjuryStatus, string> displayNames = new()
        {
            { InjuryStatus.Healthy, "injury_healthy" },
            { InjuryStatus.Injury, "injury_injured" },
            { InjuryStatus.FatalInjury, "injury_fatal" }
        };

        public static string GetDisplayName(InjuryStatus status) =>
            displayNames.TryGetValue(status, out var key) ? Loc.Get(key) : status.ToString();
    }

    public static class Heal
    {
        private static readonly Dictionary<InjuryStatus, int> costs = new()
        {
            { InjuryStatus.Healthy, 0 },
            { InjuryStatus.Injury, 1 },      // 임시값, 기획 확정 시 교체
            { InjuryStatus.FatalInjury, 2 }  // 임시값, 기획 확정 시 교체
        };

        public static int GetCost(InjuryStatus status) =>
            costs.TryGetValue(status, out var cost) ? cost : 0;

        public static void Apply(Unit unit)
        {
            if (unit == null) return;
            unit.currentInjury = InjuryStatus.Healthy;
        }
    }
}