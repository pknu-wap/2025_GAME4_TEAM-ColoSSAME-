using System.Collections.Generic;
using BattleK.Scripts.AI;
using BattleK.Scripts.Combat.Damage;

namespace BattleK.Scripts.Combat
{
    public class UnitBattleRecord
    {
        public string Name;
        public int Layer;
        public long DamageDealt;
        public long DamageTaken;
        public int Kills;
        public long HealDone;
    }
    
    public class BattleRecorder
    {
        private readonly Dictionary<StaticAICore, UnitBattleRecord> _records = new();
        public IReadOnlyDictionary<StaticAICore, UnitBattleRecord> Records => _records;
        public BattleRecorder(CombatEvents events)
        {
            events.Damaged += OnDamaged;
        }

        private void OnDamaged(DamageResult result)
        {
            if (result.Evaded || result.Blocked) return;

            GetOrCreate(result.Target).DamageTaken += result.AppliedDamage;

            if (!result.Attacker) return;
            var attacker = GetOrCreate(result.Attacker);
            attacker.DamageDealt += result.AppliedDamage;
            if (result.Killed) attacker.Kills++;
        }
        
        public void RecordHeal(StaticAICore healer, StaticAICore target, int effective)
        {
            if (healer) GetOrCreate(healer).HealDone += effective;
        }

        public long GetDealt(StaticAICore unit) => _records.TryGetValue(unit, out var r) ? r.DamageDealt : 0;
        public long GetTaken(StaticAICore unit) => _records.TryGetValue(unit, out var r) ? r.DamageTaken : 0;

        public void ResetUnit(StaticAICore unit) => _records.Remove(unit);
        public void Clear() => _records.Clear();

        private UnitBattleRecord GetOrCreate(StaticAICore unit)
        {
            if (_records.TryGetValue(unit, out var record)) return record;

            record = new UnitBattleRecord { Name = unit.name, Layer = unit.gameObject.layer };
            _records[unit] = record;
            return record;
        }
    }
}