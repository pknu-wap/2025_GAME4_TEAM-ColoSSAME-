using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Unit
{
    // 유닛의 고유 식별자
    public string unitId; 

    public int rarity;
    public string unitName;
    
    // 레벨
    public int level = 1;

    // 경험치
    public float exp = 0;

    public Unit(string id, int rarity, string unitName)
{
    this.unitId = id;
    this.rarity = rarity;
    this.unitName = unitName;
}
}