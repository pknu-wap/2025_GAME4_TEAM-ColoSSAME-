using System.Collections;
using UnityEngine;

/// <summary>
/// 상태 머신 관리 클래스. 코루틴 기반 상태 전이와 실행을 담당.
/// </summary>
public class StateMachine
{
    private readonly MonoBehaviour coroutineRunner;
    private Coroutine stateCoroutine;
    private IState _currentState;

    public StateMachine(MonoBehaviour runner)
    {
        coroutineRunner = runner;
    }

    /// <summary>
    /// 상태 전환 요청. 같은 상태면 무시, 이전 상태 종료 및 새 상태 진입 처리.
    /// </summary>
    /// <param name="newState">전환할 새 상태</param>
    public void ChangeState(IState newState)
    {
        // 2. 현재 실행 중인 상태 코루틴 종료
        if (stateCoroutine != null)
        {
            coroutineRunner.StopCoroutine(stateCoroutine);
            stateCoroutine = null;
        }

        // 3. 이전 상태 Exit 호출
        _currentState?.Exit();

        // 4. 새 상태로 설정하고 Enter 호출
        _currentState = newState;
        _currentState.Enter();

        // 5. Execute 코루틴 시작
        stateCoroutine = coroutineRunner.StartCoroutine(_currentState.Execute());
    }

    /// <summary>
    /// 현재 상태 반환
    /// </summary>
    public IState GetCurrentState() => _currentState;

    /// <summary>
    /// 현재 상태 강제 중단 (옵션)
    /// </summary>
    public void Stop()
    {
        if (stateCoroutine != null)
        {
            coroutineRunner.StopCoroutine(stateCoroutine);
            stateCoroutine = null;
        }
        _currentState?.Exit();
        _currentState = null;
    }
}