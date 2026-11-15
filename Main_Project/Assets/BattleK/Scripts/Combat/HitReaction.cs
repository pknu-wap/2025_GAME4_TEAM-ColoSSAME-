using BattleK.Scripts.AI;
using BattleK.Scripts.AI.StaticScoreState.StaticVerStates;
using BattleK.Scripts.Combat.Damage;

namespace BattleK.Scripts.Combat
{
    public class HitReaction
    {
        private readonly StaticAICore _owner;

        public HitReaction(StaticAICore owner)
        {
            _owner = owner;
        }

        public void Handle(in DamageResult result)
        {
            if (result.Evaded || result.Blocked || result.Killed || result.Revived) return;
            if (result.Request.HasFlag(DamageFlags.NoHitReaction)) return;
            if (_owner.OverrideMachine.CurrentState != null) return;

            _owner.OverrideMachine.ChangeState(new StaticHitState(_owner));
        }
    }
}