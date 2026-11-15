using BattleK.Scripts.AI;

namespace BattleK.Scripts.Combat.Revive
{
    public interface IReviveProvider
    {
        int Priority { get; }
        bool IsSpent { get; }
        bool CanRevive(StaticAICore unit);
        ReviveResult Revive(StaticAICore unit);
    }
}