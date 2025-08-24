using System.Collections;

public interface IState
{
    void Enter();
    IEnumerator Execute();
    void Exit();
}