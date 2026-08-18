using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class SeenEnemyData
{
    public string unitId;

    public string teamFid;
    public string teamName;

    public string unitName;
    public int rarity;
    public int level;

    public int maxHP;
    public int attackDamage;
    public int defense;
    public float moveSpeed;
}