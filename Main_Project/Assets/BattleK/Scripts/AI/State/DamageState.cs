using System.Collections;
using UnityEngine;

public class DamageState : IState
{
    private readonly AICore ai;
    private readonly int damage;

    public DamageState(AICore ai, int damage)
    {
        this.ai = ai;
        this.damage = damage;
    }

    public void Enter()
    {
        TakeDamage(damage);
    }

    public IEnumerator Execute()
    {
        if (ai.hp <= 0)
        {
            ai.StateMachine.ChangeState(new DeathState(ai));
        }
        else
        {
            ai.StateMachine.ChangeState(new AttackState(ai));
        }
        yield return null;
    }

    public void Exit() { }

    public void TakeDamage(int amount)
    {
        // 기존: 100 / (100 + ai.def) * amount  (정수 나눗셈으로 0되기 쉬움)
        // 개선: 부동소수점으로 정확히 계산, 최소 1피해 보장(선택)
        float reduction = 100f / (100f + Mathf.Max(0, ai.def));
        int finalDamage = Mathf.Max(1, Mathf.RoundToInt(amount * reduction));
        ai.hp -= finalDamage;
    }
}