using BattleK.Scripts.Data.ClassInfo;
using UnityEngine;

internal readonly struct BattleUnitStatSnapshot
{
    private readonly int maxHp;
    private readonly int currentHp;
    private readonly int attackDamage;
    private readonly int defense;
    private readonly float attackSpeed;
    private readonly float attackDelay;
    private readonly float evasionRate;
    private readonly float moveSpeed;

    public BattleUnitStatSnapshot(UnitRuntimeStat runtimeStat)
    {
        maxHp = runtimeStat.MaxHP;
        currentHp = runtimeStat.CurrentHP;
        attackDamage = runtimeStat.AttackDamage;
        defense = runtimeStat.Defense;
        attackSpeed = runtimeStat.AttackSpeed;
        attackDelay = runtimeStat.AttackDelay;
        evasionRate = runtimeStat.EvasionRate;
        moveSpeed = runtimeStat.MoveSpeed;
    }

    public void Restore(UnitRuntimeStat runtimeStat)
    {
        runtimeStat.MaxHP = maxHp;
        runtimeStat.CurrentHP = Mathf.Clamp(currentHp, 0, maxHp);
        runtimeStat.AttackDamage = attackDamage;
        runtimeStat.Defense = defense;
        runtimeStat.AttackSpeed = attackSpeed;
        runtimeStat.AttackDelay = attackDelay;
        runtimeStat.EvasionRate = evasionRate;
        runtimeStat.MoveSpeed = moveSpeed;
    }
}
