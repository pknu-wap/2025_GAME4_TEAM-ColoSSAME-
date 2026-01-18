using UnityEngine;
using UnityEngine.UI;

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
    public string Name;
    public Sprite Icon;
    public bool IsPlayer;
    
    [Header("클래스")]
    public UnitClass UnitClass;
    public bool IsRanged;

    [Header("시야 범위")]
    public float SightRange;
    
    [Header("능력치")]
    public int MaxHP;
    public int CurrentHP;
    public int AttackDamage;
    public float AttackSpeed;
    public float AttackRange;
    public float AttackDelay;
    public int Defense;
    public float MoveSpeed;
    public float EvasionRate;
}
