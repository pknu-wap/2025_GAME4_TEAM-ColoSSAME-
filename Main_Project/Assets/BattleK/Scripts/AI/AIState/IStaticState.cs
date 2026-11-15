using System.Collections;

namespace BattleK.Scripts.AI.AIState
{
    public interface IStaticScoreState
    {
        void Enter();
        IEnumerator Execute();
        void Exit();
    }

    public interface IStaticActionState : IStaticScoreState
    {
        int Priority { get; }
        bool CanExecute();
    }
}
