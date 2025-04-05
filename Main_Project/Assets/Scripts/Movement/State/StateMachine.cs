using System.Collections;

namespace Movement.State
{
    public class StateMachine
    {
        private IState currentState;

        public void ChangeState(IState newState)
        {
            currentState?.ExitState();
            currentState = newState;
            currentState.EnterState();
        }

        public IEnumerator ExecuteState()
        {
            if (currentState != null)
            {
                yield return currentState.ExecuteState();
            }
        }
    }
}