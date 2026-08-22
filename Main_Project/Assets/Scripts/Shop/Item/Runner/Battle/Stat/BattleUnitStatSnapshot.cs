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

    public BattleUnitStatSnapshot(UnitStat stat)
    {
        maxHp = stat.MaxHP;
        currentHp = stat.CurrentHP;
        attackDamage = stat.AttackDamage;
        defense = stat.Defense;
        attackSpeed = stat.AttackSpeed;
        attackDelay = stat.AttackDelay;
        evasionRate = stat.EvasionRate;
        moveSpeed = stat.MoveSpeed;
    }

    public void Restore(UnitStat stat)
    {
        stat.MaxHP = maxHp;
        stat.CurrentHP = Mathf.Clamp(currentHp, 0, maxHp);
        stat.AttackDamage = attackDamage;
        stat.Defense = defense;
        stat.AttackSpeed = attackSpeed;
        stat.AttackDelay = attackDelay;
        stat.EvasionRate = evasionRate;
        stat.MoveSpeed = moveSpeed;
    }
}
