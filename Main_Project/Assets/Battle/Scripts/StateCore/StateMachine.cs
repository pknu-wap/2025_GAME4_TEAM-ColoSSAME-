using Battle.Scripts.Ai;
using UnityEngine;

namespace Battle.Scripts.StateCore
{
    public class StateMachine
    {
        public IState currentState { get; private set; }
        public IState previousState { get; private set; }

        public void ChangeState(IState newState)
        {
            previousState = currentState;
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