using System;
using System.Collections.Generic;

[Serializable]
public class TournamentData
{
    public List<Match> quarterFinals = new();
    public List<Match> semiFinals = new();
    public Match finalMatch;
}

[Serializable]
public class Match
{
    public string player1Key;
    public string player2Key;
    public string winnerKey;
}
