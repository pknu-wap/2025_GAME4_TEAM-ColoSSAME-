namespace Battle.Scripts.StateCore
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

        public void Update()
        {
            currentState?.UpdateState();
        }
    }
}