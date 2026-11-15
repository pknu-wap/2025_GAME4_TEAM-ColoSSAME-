namespace BattleK.Scripts.Combat.Damage
{
    public interface IDamageSourceProfile
    {
        string Id { get; }
        string Group { get; }
        DamageDelivery Delivery { get; }
        DamageFlags DefaultFlags { get; }
    }
}