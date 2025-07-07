using UnityEngine;
using System.Collections.Generic;

public class LeagueSettingManager : MonoBehaviour
{
    /// <summary>
    /// 리그 초기화: League 오브젝트 생성, 팀 & 스케줄 생성
    /// </summary>
    public League InitializeLeague()
    {
        League league = new League();

        // Settings
        league.settings = new Settings
        {
            name = "콜로세움 리그 시즌 1",
            totalRounds = 27, // (10-1)*3 = 27라운드
            pointRule = new PointRule { win = 3, draw = 1, lose = 0 },
            playerTeamId = 1, // 플레이어 팀 ID
            playerTeamName = "팀 A"
        };

        // 팀 생성
        league.teams = GenerateInitialTeams();

        // 스케줄 생성 (각 팀별로 3번씩 대전)
        league.schedule = GenerateMultiRoundRobinSchedule(league.teams, 3);

        Debug.Log("✅ 리그 초기화 완료 (LeagueSettingManager)");
        return league;
    }

    /// <summary>
    /// 10팀 생성
    /// </summary>
    private List<Team> GenerateInitialTeams()
    {
        List<Team> teams = new List<Team>();

        teams.Add(new Team { id = 1, name = "카이루스" });
        teams.Add(new Team { id = 2, name = "플로라" });
        teams.Add(new Team { id = 3, name = "이그니스" });
        teams.Add(new Team { id = 4, name = "루멘" });
        teams.Add(new Team { id = 5, name = "녹스" });
        teams.Add(new Team { id = 6, name = "모르스" });
        teams.Add(new Team { id = 7, name = "폴그르" });
        teams.Add(new Team { id = 8, name = "마레" });
        teams.Add(new Team { id = 9, name = "테라" });
        teams.Add(new Team { id = 10, name = "아스트라" });

        return teams;
    }

    /// <summary>
    /// n회 라운드로빈 스케줄 생성
    /// </summary>
    private List<Round> GenerateMultiRoundRobinSchedule(List<Team> teams, int repeatCount)
    {
        List<Round> schedule = new List<Round>();

        for (int r = 0; r < repeatCount; r++)
        {
            List<Round> singleRound = GenerateSingleRoundRobinSchedule(teams, r + 1);
            schedule.AddRange(singleRound);
        }

        return schedule;
    }

    /// <summary>
    /// 1회 라운드로빈 스케줄 생성
    /// </summary>
    private List<Round> GenerateSingleRoundRobinSchedule(List<Team> teams, int repeatIndex)
    {
        List<Round> schedule = new List<Round>();
        int n = teams.Count;
        int rounds = n - 1;
        int half = n / 2;

        List<int> teamIds = new List<int>();
        foreach (var team in teams)
            teamIds.Add(team.id);

        if (n % 2 == 1)
            teamIds.Add(-1); // bye

        for (int round = 0; round < rounds; round++)
        {
            Round r = new Round();
            r.roundNumber = round + 1 + (repeatIndex - 1) * rounds; // 라운드 번호 누적
            r.matches = new List<LeagueMatch>();

            for (int i = 0; i < half; i++)
            {
                int teamA = teamIds[i];
                int teamB = teamIds[teamIds.Count - 1 - i];

                if (teamA != -1 && teamB != -1)
                {
                    LeagueMatch match = new LeagueMatch
                    {
                        matchId = $"{r.roundNumber}-{i + 1}",
                        teamAId = teamA,
                        teamBId = teamB,
                        result = null
                    };
                    r.matches.Add(match);
                }
            }

            // rotate
            List<int> newOrder = new List<int>();
            newOrder.Add(teamIds[0]);
            newOrder.Add(teamIds[teamIds.Count - 1]);
            for (int i = 1; i < teamIds.Count - 1; i++)
                newOrder.Add(teamIds[i]);
            teamIds = newOrder;

            schedule.Add(r);
        }

        return schedule;
    }
}
