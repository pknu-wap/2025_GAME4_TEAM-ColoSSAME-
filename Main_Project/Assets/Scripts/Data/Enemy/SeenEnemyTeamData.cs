using System;
using System.Collections.Generic;

[Serializable]
public class SeenEnemyTeamData
{
    public string teamFid;
    public List<SeenEnemyData> enemies = new List<SeenEnemyData>();
}