using System.Collections;

namespace Movement.State
{
    public interface IState
    {
        void EnterState();
        IEnumerator ExecuteState();
        void ExitState();
    }
}