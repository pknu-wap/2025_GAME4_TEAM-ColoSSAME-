using System.Collections.Generic;
using BattleK.Scripts.Combat.Status;

namespace BattleK.Scripts.Combat.Damage
{
    public class CombatContext
    {
        public CombatEvents Events { get; }
        public DamageService Damage { get; }
        
        public StatusService Status { get; } = new StatusService();
        public BattleRecorder Recorder { get; }

        public CombatContext(IEnumerable<IDamageStage> damageStages = null)
        {
            Events = new CombatEvents();
            Damage = new DamageService(damageStages, Events);
            Recorder = new BattleRecorder(Events);
        }
    }
}