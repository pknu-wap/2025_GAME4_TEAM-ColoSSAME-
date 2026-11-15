using System;

namespace BattleK.Scripts.Combat.Damage
{
    public sealed class CombatEvents
    {
        public event Action<DamageResult> Damaged;
        public event Action<DamageResult> Killed;
        public event Action<DamageResult> Revived;
        
        public void RaiseRevived(in DamageResult result) => Revived?.Invoke(result);
        public void RaiseDamaged(in DamageResult result) => Damaged?.Invoke(result);
        public void RaiseKilled(in DamageResult result) => Killed?.Invoke(result);

        public void ClearSubscribers()
        {
            Damaged = null;
            Killed = null;
        }
    }
}