using System.Collections;
using System.Collections.Generic;
using BattleK.Scripts.Data.ClassInfo;
using UnityEngine;
using UnityEngine.Serialization;

[System.Serializable]
public class CharacterData
{
    public string Family_ID;
    public string Family_Name;
    public string Unit_ID;
    public string Unit_Name;
    public int Tier;
    public UnitClass Class;
    public string Description;
    public string Story;
    public Stat_Distribution Stat_Distribution;
    public Visuals Visuals;
}