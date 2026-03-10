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

    public UnitSkill(string id, int level = 1)
    {
        skillId = id;
        this.level = level;
    }
}
