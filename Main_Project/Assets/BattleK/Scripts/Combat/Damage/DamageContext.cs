namespace BattleK.Scripts.Combat.Damage
{
    public sealed class DamageContext
    {
        public readonly DamageRequest Request;
        public float Amount;
        public int ShieldAbsorbed;
        public int AppliedDamage;
        public int Overkill;
        public bool Evaded;
        public bool Blocked;
        public bool Killed;
        public bool Ignored;
        public bool Cancelled;
        public bool Revived;

        public DamageContext(in DamageRequest request)
        {
            Request = request;
            Amount = request.Amount;
        }

        public void Ignore() { Ignored = true; Cancelled = true; }
        public void MarkEvaded() { Evaded = true; Cancelled = true; }
        public void MarkBlocked() { Blocked = true; Cancelled = true; }
    }
}