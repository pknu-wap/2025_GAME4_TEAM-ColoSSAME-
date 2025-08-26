using System.Collections;

/// <summary>
/// 코루틴 기반 상태 인터페이스.
/// 계약(Contract):
/// 1) Enter(): 상태 진입 시 정확히 한 번 호출된다.
/// 2) Execute(): 상태의 메인 루프를 코루틴으로 수행한다.
///    - 상태 전환이 필요해지면 StateMachine.ChangeState(...) 호출 후 즉시 'yield break' 하라.
///    - 주기적으로 '소유자(AICore)의 생존 여부'와 '타겟 유효성'을 검사해 무한 대기를 피하라.
/// 3) Exit(): 다른 상태로 전환될 때 정확히 한 번 호출된다.
/// </summary>
public interface IState
{
    /// <summary>상태 진입 시 1회 호출.</summary>
    void Enter();

    /// <summary>
    /// 상태 메인 루프(여러 프레임). 상태 전환 시점에 'yield break' 권장.
    /// </summary>
    IEnumerator Execute();

    /// <summary>상태 종료(다음 상태로 전환 직전) 시 1회 호출.</summary>
    void Exit();
}