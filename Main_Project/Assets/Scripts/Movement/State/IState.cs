using System.Collections;

public interface IState
{
    void EnterState();
    IEnumerator ExecuteState();
    void ExitState();
}