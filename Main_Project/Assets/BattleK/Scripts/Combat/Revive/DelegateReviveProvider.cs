using System;
using BattleK.Scripts.AI;

namespace BattleK.Scripts.Combat.Revive
{
    public class DelegateReviveProvider : IReviveProvider
    {
        private readonly Func<StaticAICore, bool> _canRevive;
        private readonly Func<StaticAICore, ReviveResult> _revive;
        private readonly Func<bool> _isSpent;

        public int Priority { get; }
        public bool IsSpent => _isSpent != null && _isSpent();

        public DelegateReviveProvider(int priority, Func<StaticAICore, bool> canRevive, Func<StaticAICore, ReviveResult> revive, Func<bool> isSpent = null)
        {
            Priority = priority;
            _canRevive = canRevive;
            _revive = revive;
            _isSpent = isSpent;
        }

        public bool CanRevive(StaticAICore unit) => _canRevive(unit);
        public ReviveResult Revive(StaticAICore unit) => _revive(unit);
    }
}