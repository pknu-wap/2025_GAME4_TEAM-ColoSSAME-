using UnityEngine;
using System.Collections.Generic;


    [System.Serializable]
    public class League
    {
        public Settings settings;
        public List<Team> teams;
        public List<Round> schedule;
    }

    [System.Serializable]
    public class Settings
    {
        public string name;
        public int totalRounds;
        public PointRule pointRule;
        public int playerTeamId; // 내 팀 id
        public string playerTeamName; // 내 팀 이름
    }

    [System.Serializable]
    public class PointRule
    {
        public int win;
        public int draw;
        public int lose;
    }

    [System.Serializable]
    public class Team
    {
        public int id;
        public string fid;
        public string name;
        public string explanation;
        public int played;
        public int win;
        public int draw;
        public int lose;
        public int goalsFor;
        public int goalsAgainst;
        public int points;
        public int rank; // 팀 순위

    }

    [System.Serializable]
    public class Round
    {
        public int roundNumber;
        public List<LeagueMatch> matches;
    }

    [System.Serializable]
    public class LeagueMatch
    {
        public string matchId;
        public int teamAId;
        public int teamBId;
        public Result result;
    }

    [System.Serializable]
    public class Result
    {
        public int winner;
        public int scoreA;
        public int scoreB;
    }
