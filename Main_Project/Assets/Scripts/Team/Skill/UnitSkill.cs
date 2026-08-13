using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BattleK.Scripts.AI.Skill.Base;
using System;

[System.Serializable]
public class UnitSkill
{
    public string skillName;

    public int level;

    public UnitSkill(string skillName, int level = 1)
    {
        this.skillName = skillName;
        this.level = level;
    }

    public void LevelUp(int amount = 1)
    {
        level += amount;
    }
}