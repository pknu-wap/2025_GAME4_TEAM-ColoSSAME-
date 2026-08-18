using System.Collections.Generic;

[System.Serializable]
public class EnemyTeamList
{
    public List<EnemyTeam> teams = new List<EnemyTeam>();
    public List<string> seenEnemyIds = new List<string>();
}