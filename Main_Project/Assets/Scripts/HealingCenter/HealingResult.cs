namespace Colosseum.HealingCenter
{
    public enum HealingResultType
    {
        Success,
        NotInjured,
        NotEnoughMoney
    }
    
    public class HealingResult
    {
        public HealingResultType ResultType { get; }
        public string UnitId { get; }
        public int SpentGold { get; }

        public bool IsSuccess => ResultType == HealingResultType.Success;

        public HealingResult(HealingResultType resultType, string unitId, int spentGold = 0)
        {
            ResultType = resultType;
            UnitId = unitId;
            SpentGold = spentGold;
        }

        public string GetMessage()
        {
            switch (ResultType)
            {
                case HealingResultType.Success:
                    return Loc.Get("heal_done");
                case HealingResultType.NotInjured:
                    return Loc.Get("heal_not_needed");
                case HealingResultType.NotEnoughMoney:
                    return Loc.Get("heal_no_gold");
            }
            return "";
        }
    }
}
