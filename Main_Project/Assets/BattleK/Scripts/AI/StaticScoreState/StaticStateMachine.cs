using UnityEngine;

namespace BattleK.Scripts.AI.StaticScoreState
{
    public class StaticStateMachine
    {
        public IStaticScoreState CurrentState { get; private set; }
        
        private readonly MonoBehaviour _runner;
        private Coroutine _runningRoutine;

        public StaticStateMachine(MonoBehaviour runner)
        {
            _runner = runner;
        }

        public void ChangeState(IStaticScoreState newState)
        {
            if (CurrentState == newState) return;
            
            StopCurrentState();
            CurrentState = newState;
            StartCurrentState();
        }
        
        public void StopAndClear()
        {
            StopCurrentState();
            CurrentState = null;
        }

        private void StartCurrentState()
        {
            if (CurrentState == null) return;
            CurrentState.Enter();
            _runningRoutine = _runner.StartCoroutine(CurrentState.Execute());
        }

        private void StopCurrentState()
        {
            if (CurrentState == null) return;
            if(_runningRoutine != null) _runner.StopCoroutine(_runningRoutine);
            _runningRoutine = null;
            
            CurrentState.Exit();
            CurrentState = null;
        }
    }
}
