using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BattleK.Scripts.AI.Skill.Base;
using System;

[System.Serializable]
public class UnitSkill
{
    public string skillId;

    public int level;

    public UnitSkill(string skillId, int level = 1)
    {
        this.skillId = skillId;
        this.level = level;
    }

    public void LevelUp(int amount = 1)
    {
        level += amount;
    }
}