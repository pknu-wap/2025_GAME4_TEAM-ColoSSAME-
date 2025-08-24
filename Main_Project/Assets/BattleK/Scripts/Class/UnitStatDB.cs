using System.Collections.Generic;

public static class UnitStatDB
{
    private static readonly Dictionary<UnitClass, UnitStat> statDict = new Dictionary<UnitClass, UnitStat>
    {
        {
            UnitClass.Swordsman, new UnitStat
            {
                unitClass = UnitClass.Swordsman,
                team = TeamUnit.Player,
                defaultState = State.Idle,
                targetStrategy = TargetStrategy.NearestTarget,
                attackDelay = 1f,
                skillCooldown = 5f,
                baseHP = 120,
                baseDamage = 20,
                moveSpeed = 3f,
                attackRange = 1.5f,
                sightRange = 6f
            }
        },
        {
            UnitClass.Archer, new UnitStat
            {
                unitClass = UnitClass.Archer,
                team = TeamUnit.Player,
                defaultState = State.Idle,
                targetStrategy = TargetStrategy.NearestTargetWithClass,
                attackDelay = 1.5f,
                skillCooldown = 7f,
                baseHP = 80,
                baseDamage = 25,
                moveSpeed = 3.5f,
                attackRange = 6f,
                sightRange = 10f
            }
        },
        // 나머지 직업들도 여기에 추가
    };

    public static UnitStat GetStat(UnitClass unitClass)
    {
        return statDict.TryGetValue(unitClass, out var stat) ? stat : null;
    }
}