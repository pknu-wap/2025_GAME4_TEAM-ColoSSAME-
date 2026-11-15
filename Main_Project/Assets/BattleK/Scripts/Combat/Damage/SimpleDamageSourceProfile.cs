namespace BattleK.Scripts.Combat.Damage
{
    public class SimpleDamageSourceProfile : IDamageSourceProfile
    {
        public string Id { get; }
        public string Group { get; }
        public DamageDelivery Delivery { get; }
        public DamageFlags DefaultFlags { get; }

        public SimpleDamageSourceProfile(string id, string group = "", DamageDelivery delivery = DamageDelivery.Direct, DamageFlags defaultFlags = DamageFlags.None)
        {
            Id = id;
            Group = group;
            Delivery = delivery;
            DefaultFlags = defaultFlags;
        }

        public static readonly SimpleDamageSourceProfile Unknown = new SimpleDamageSourceProfile("Unknown");
    }
}