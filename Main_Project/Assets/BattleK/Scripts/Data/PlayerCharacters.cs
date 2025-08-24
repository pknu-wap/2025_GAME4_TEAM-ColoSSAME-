using System;
using System.Collections.Generic;

// 전체 저장 데이터(여러 캐릭터 포함)
[Serializable]
public class PlayerCharacters
{
    public List<CharacterRecord> characters = new List<CharacterRecord>();
}

// 개별 캐릭터의 데이터 구조
[Serializable]
public class CharacterRecord
{
    public string characterKey;

    public int hp;
    public int def;
    public int moveSpeed;
    public int attackDamage;
    public float attackSpeed;
    public float attackRange;
    public float sightRange;
    public float evasionRate;
    public float skillRange;
    public float skillDelay;
    public string unitClass;

    public List<UnitClass> targetClasses = new List<UnitClass>();
    public string target1; // 예: "검사"
    public string target2; // 예: "도끼병"
}