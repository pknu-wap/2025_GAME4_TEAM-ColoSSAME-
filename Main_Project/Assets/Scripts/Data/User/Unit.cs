using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BattleK.Scripts.AI.Skill.Base;
using BattleK.Scripts.Data.ClassInfo;

[System.Serializable]
public class Unit
{
    // 유닛의 고유 식별자
    public string unitId; 
    public int rarity;
    public int bonusSuccessRarity;
    public string unitName;
    public string unitClass;
    
    public int level = 1;
    public float exp = 0;
    
    public InjuryStatus currentInjury = InjuryStatus.Healthy;
    //public string equippedItemId;
    
    public List<UnitSkill> skills = new();
    public List<string> selectedSkills = new();

    public Unit(string id, int rarity, string unitName, string unitClass)
    {
        this.unitId = id;
        this.rarity = rarity;
        this.unitName = unitName;
        this.unitClass = unitClass;
    }
}