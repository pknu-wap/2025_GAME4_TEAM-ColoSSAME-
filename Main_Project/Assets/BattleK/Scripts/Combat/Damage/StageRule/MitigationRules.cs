namespace BattleK.Scripts.Combat.Damage.StageRule
{
    public interface IMitigationRule
    {
        float Mitigate(float amount, float armor);
    }

    public class DiminishingMitigationRule : IMitigationRule
    {
        public float Mitigate(float amount, float armor)
            => amount * (float)(100.0 / (100.0 + armor));
    }

    public class NoMitigationRule : IMitigationRule
    {
        public float Mitigate(float amount, float armor) => amount;
    }
}