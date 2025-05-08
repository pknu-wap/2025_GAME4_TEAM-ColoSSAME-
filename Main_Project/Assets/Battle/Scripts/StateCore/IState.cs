namespace Battle.Scripts.StateCore
{
    public interface IState
    {
        void EnterState();
        void UpdateState();
        void ExitState();
    }
}