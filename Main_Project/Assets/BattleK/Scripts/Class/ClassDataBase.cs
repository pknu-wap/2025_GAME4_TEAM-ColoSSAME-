using UnityEngine;

// [✓] 유닛 종류
public enum UnitClass
{
    Swordsman,
    Archer,
    Mage,
    Axeman,
    Spearman,
    Thief,
    Priest,
    Shieldman
}

// [✓] 직업별 공격 딜레이 (ms 기준)
public enum UnitAttackDelay
{
    Swordsman = 25,
    Archer = 50,
    Mage = 25,
    Axeman = 43,
    Spearman = 30,
    Thief = 25
}

// [✓] 팀 구분
public enum TeamUnit
{
    Player = 10,
    Enemy
}

// [✓] 타겟 전략
public enum TargetStrategy
{
    NearestTarget,
    NearestTargetWithClass
}

// [✓] 상태 정의
public enum State
{
    None,
    Idle,
    Targeting,
    MOVE,
    Attack,
    Death,
    Skill // ← 필요시 추가
}

public enum ActionType
{
    Attack,
    Skill
}

[System.Serializable]
public class UnitStat
{
    public UnitClass unitClass;
    public TeamUnit team;
    public State defaultState;
    public TargetStrategy targetStrategy;

    [Header("전투 관련")]
    public float attackDelay;
    public float skillCooldown;

    [Header("기본 능력치")]
    public int baseHP;
    public int baseDamage;
    public float moveSpeed;
    public float attackRange;
    public float sightRange;
}
