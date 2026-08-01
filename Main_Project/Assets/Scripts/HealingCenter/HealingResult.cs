namespace Colosseum.HealingCenter
{
    public enum HealingResultType
    {
        Success,
        NotInjured,
        NotEnoughMoney,
        NoCharacterSelected
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
                    return "치유가 완료되었습니다.";
                case HealingResultType.NotInjured:
                    return "치유가 필요하지 않습니다.";
                case HealingResultType.NotEnoughMoney:
                    return "골드가 부족합니다.";
                default:
                    return "선택된 캐릭터가 없습니다.";
            }
        }
    }
}
