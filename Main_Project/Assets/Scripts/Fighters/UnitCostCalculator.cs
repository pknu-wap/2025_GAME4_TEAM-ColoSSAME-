public static class UnitCostCalculator
{
    public const float EXP_GAIN= 10f;
    public static int CalculateGoldCost(int level)
    {
        switch (level)
        {
            case 1:  return 10;
            case 2:  return 20;
            case 3:  return 35;
            case 4:  return 50;
            case 5:  return 70;
            case 6:  return 95;
            case 7:  return 125;
            case 8:  return 160;
            case 9:  return 200;
            case 10: return 250;
            default: return 300;
        }
    }
}