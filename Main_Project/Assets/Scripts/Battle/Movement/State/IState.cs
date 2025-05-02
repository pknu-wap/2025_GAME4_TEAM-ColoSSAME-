using System.Collections;

namespace Battle.Movement.State
{
    public interface IState
    {
        void EnterState();
        IEnumerator ExecuteState();
        void ExitState();
    }
}