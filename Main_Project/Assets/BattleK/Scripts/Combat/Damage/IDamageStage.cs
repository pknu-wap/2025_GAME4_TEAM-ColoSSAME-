namespace BattleK.Scripts.Combat.Damage
{
    public interface IDamageStage
    {
        int Order { get; }
        void Process(DamageContext ctx);
    }
}