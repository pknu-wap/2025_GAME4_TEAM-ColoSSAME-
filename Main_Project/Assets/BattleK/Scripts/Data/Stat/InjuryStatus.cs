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
            { InjuryStatus.Healthy, "건강함" },
            { InjuryStatus.Injury, "부상" },
            { InjuryStatus.FatalInjury, "치명상" }
        };

        public static string GetDisplayName(InjuryStatus status) =>
            displayNames.TryGetValue(status, out var name) ? name : status.ToString();
    }
}