using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class EnemyTeam
{
    public int id;
    public string fid;
    public string name;
    
    public int money;
    public int growthStage;
    
    public List<Unit> units = new List<Unit>();
    
    public EnemyTeam()

    {

    }

    public EnemyTeam(int id, string fid, string name)

    {

        this.id = id;

        this.fid = fid;

        this.name = name;

        this.money = 0;

        this.growthStage = 0;

        this.units = new List<Unit>();

    }
}
