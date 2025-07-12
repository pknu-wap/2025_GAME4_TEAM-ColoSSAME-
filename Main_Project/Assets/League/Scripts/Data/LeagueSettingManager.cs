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

        teams.Add(new Team { id = 1, name = "카이루스", explanation = "“하늘과 바람을 지배하는 귀족 가문.\n빠르고 우아한 전술을 자랑한다.”" });
        
        teams.Add(new Team { id = 2, name = "플로라", explanation = "“숲과 생명의 숨결을 이어받은 가문.\n치유와 재생의 힘을 지녔다.”" });
        
        teams.Add(new Team { id = 3, name = "이그니스", explanation = "“용의 피가 흐르는 불꽃의 가문.\n강력한 공격과 불굴의 기개로 적을 불태운다.”" });
        
        teams.Add(new Team { id = 4, name = "루멘", explanation = "“빛과 철의 질서를 수호하는 성기사 가문.\n굳건한 방어와 정의의 힘을 믿는다.”" });
        
        teams.Add(new Team { id = 5, name = "녹스", explanation = "“밤과 달의 신비를 따르는 그림자 가문.\n은밀하고 치명적인 일격을 가한다.”" });
        
        teams.Add(new Team { id = 6, name = "모르스", explanation = "“죽음과 저주의 사제를 모시는 가문.\n망령의 힘으로 적을 저주한다.”" });
        
        teams.Add(new Team { id = 7, name = "폴그르", explanation = "“천둥의 신에게 선택받은 속도의 가문.\n번개처럼 빠르고 강렬한 공격을 펼친다.”" });
        
        teams.Add(new Team { id = 8, name = "마레", explanation = "“바다의 심연을 품은 가문.\n흐르는 물처럼 유연하고 냉철하다.”" });
        
        teams.Add(new Team { id = 9, name = "테라", explanation = "“대지와 산의 수호자 가문.\n강인한 방어력과 흔들리지 않는 의지를 가진다.”" });
        
        teams.Add(new Team { id = 10, name = "아스트라",explanation = "“별과 운명을 읽는 예언자 가문.\n신비한 힘으로 전장의 흐름을 꿰뚫는다.”" });
        
        
        

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
