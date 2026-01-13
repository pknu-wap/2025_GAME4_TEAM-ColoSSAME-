using System.Collections;
using BattleK.Scripts.AI;
using UnityEngine;

/// <summary>
/// 코루틴 기반 상태 머신.
/// - ChangeState() 호출 시: 이전 상태 Exit → 코루틴 중지 → 새 상태 Enter → 새 상태 Execute 코루틴 시작
/// - 사망 가드: owner.IsDead이면 DeathState 외의 진입을 차단
/// </summary>
public class StateMachine
{
    public IState CurrentState { get; private set; }

    // 상태 코루틴 실행 주체(= AICore 자신)
    private readonly AICore _owner;
    private readonly MonoBehaviour _runner;
    private Coroutine _stateRoutine;

    public StateMachine(AICore owner)
    {
        _owner  = owner;
        _runner = owner; // AICore는 MonoBehaviour이므로 그대로 실행자 사용
    }

    /// <summary>
    /// 상태 전환(코루틴 안전 관리 포함)
    /// </summary>
    public void ChangeState(IState newState)
    {
        // 사망 가드: 죽은 뒤에는 DeathState 외 진입 금지
        if (_owner != null && _owner.IsDead && !(newState is DeathState))
            return;

        // 이전 상태 종료
        if (CurrentState != null)
        {
            // 이전 상태 코루틴 중지
            SafeStopStateRoutine();
            // Exit 호출
            try { CurrentState.Exit(); } catch { /* 상태 Exit 중 예외 방어 */ }
        }

        // 새 상태 지정
        CurrentState = newState;

        // 새 상태 진입
        if (CurrentState != null)
        {
            try { CurrentState.Enter(); } catch { /* 상태 Enter 중 예외 방어 */ }

            // 새 상태 Execute 코루틴 시작
            if (_runner != null)
            {
                _stateRoutine = _runner.StartCoroutine(RunStateCoroutine(CurrentState));
            }
        }
    }

    /// <summary>
    /// 현재 상태 코루틴을 중지하고 상태를 비운다(필요 시 사용).
    /// </summary>
    public void Stop()
    {
        SafeStopStateRoutine();
        if (CurrentState != null)
        {
            try { CurrentState.Exit(); } catch { }
            CurrentState = null;
        }
    }

    /// <summary>
    /// (호환성 유지용) 구 Tick 기반 호출이 남아 있어도 컴파일 에러 방지.
    /// 코루틴 기반이므로 특별히 할 일은 없다.
    /// </summary>
    public void Update()
    {
        // no-op (이전 Tick 기반 패턴과의 호환을 위해 남겨둠)
        // 상태 로직은 모두 Execute 코루틴에서 처리됨
        if (_owner != null && _owner.IsDead && !(CurrentState is DeathState))
        {
            ChangeState(new DeathState(_owner));
        }
    }

    private IEnumerator RunStateCoroutine(IState state)
    {
        // 현재 상태의 Execute가 완료되면(혹은 내부에서 ChangeState 호출로 중단되면) 자연 종료
        IEnumerator it = null;
        try { it = state.Execute(); }
        catch { yield break; }

        if (it != null)
        {
            while (true)
            {
                // 소유자 사망 시 즉시 Death로 전환
                if (_owner != null && _owner.IsDead && !(state is DeathState))
                {
                    ChangeState(new DeathState(_owner));
                    yield break;
                }

                // 다음 프레임까지 진행
                bool moveNext;
                try { moveNext = it.MoveNext(); }
                catch { yield break; }

                if (!moveNext) break;
                yield return it.Current;
            }
        }

        // 상태 코루틴이 정상 종료된 경우: 다음 상태는 각 상태 내부에서 ChangeState로 옮김
        // 여기서는 아무 것도 하지 않음
        yield break;
    }

    private void SafeStopStateRoutine()
    {
        if (_runner != null && _stateRoutine != null)
        {
            try { _runner.StopCoroutine(_stateRoutine); } catch { }
        }
        _stateRoutine = null;
    }
}
