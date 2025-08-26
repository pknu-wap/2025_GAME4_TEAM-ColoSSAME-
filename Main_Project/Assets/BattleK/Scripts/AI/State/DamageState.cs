using System.Collections;
using UnityEngine;

public class DamageState : IState
{
    private readonly AICore ai;
    private readonly int damage;

    // 짧은 피격 경직(연출 및 즉시 반격 방지)
    private const float HitStunSeconds = 0.08f;

    public DamageState(AICore ai, int damage)
    {
        this.ai = ai;
        this.damage = damage;
    }

    public void Enter()
    {
        if (ai == null || ai.IsDead) return;

        // 실제 데미지 적용
        TakeDamage(damage);

        // 피격 즉시 모든 액션 정리
        ai.meleeAttack?.CancelAll();
        ai.rangedAttack?.CancelAll();

        // 하드 정지(짧은 시간)
        ai.StopMovementHard(alsoZeroMaxSpeed: false);

        // 피격 연출 필요 시:
        // ai.player.SetStateAnimationIndex(PlayerState.DAMAGED, 0);
        // ai.player.PlayStateAnimation(PlayerState.DAMAGED);
    }

    public IEnumerator Execute()
    {
        if (ai == null) yield break;

        // 피격 경직
        if (HitStunSeconds > 0f)
            yield return new WaitForSeconds(HitStunSeconds);

        if (ai == null) yield break;

        // 사망 체크
        if (ai.hp <= 0)
        {
            ai.Kill(); // DeathState 전환 + 원자적 종료
            yield break;
        }

        // 타겟 유효성
        if (!IsTargetUsable(ai.target))
        {
            ai.StateMachine.ChangeState(new TargetingState(ai));
            yield break;
        }

        // ✅ 핵심: 피격 시에는 attackDelay + stun 동안 반드시 제자리(Idle) 유지
        float hold = Mathf.Max(0f, ai.attackDelay) + Mathf.Max(0f, ai.stun);
        ai.StateMachine.ChangeState(new IdleState(ai, holdSeconds: hold, softStop: true));
        yield break;
    }

    public void Exit() { }

    private void TakeDamage(int amount)
    {
        // 방어력 기반 경감 + 최소 피해 1 보장
        float reduction = 100f / (100f + Mathf.Max(0, ai.def));
        int finalDamage = Mathf.Max(1, Mathf.RoundToInt(amount * reduction));
        ai.hp -= finalDamage;
    }

    private static bool IsTargetUsable(Transform t)
    {
        if (t == null || !t.gameObject.activeInHierarchy) return false;
        var tc = t.GetComponent<AICore>();
        if (tc == null || tc.IsDead || tc.State == State.Death) return false;
        return true;
    }
}
