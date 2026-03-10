using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BattleK.Scripts.AI.Skill.Base;

[System.Serializable]
public class Unit
{
    // 유닛의 고유 식별자
    public string unitId; 

    public int rarity;
    public string unitName;
    public string unitClass;
    
    // 레벨
    public int level = 1;

    // 경험치
    public float exp = 0;

    //스킬 리스트
    public List<UnitSkill> skills = new List<UnitSkill>();

    public Unit(string id, int rarity, string unitName, string unitClass)
{
    this.unitId = id;
    this.rarity = rarity;
    this.unitName = unitName;
    this.unitClass = unitClass;

}
}