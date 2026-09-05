using System.Collections.Generic;

namespace BattleK.Scripts.AI.CCState
{
    public readonly struct StatBreakdown
    {
        public readonly float BaseValue;
        public readonly List<(string Label, float Delta)> Contributions;
        public readonly float FinalValue;

        public StatBreakdown(float baseValue, List<(string, float)> contributions, float finalValue)
        {
            BaseValue = baseValue;
            Contributions = contributions;
            FinalValue = finalValue;
        }
    }
}