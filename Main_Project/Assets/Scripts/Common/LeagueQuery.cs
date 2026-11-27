using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// 리그 데이터 읽기 전용 도우미. LeagueManager의 값을 계산해서 돌려주기만 하고 바꾸지 않는다.
// 화면(페이지) 스크립트는 이 도우미를 불러 글자·그림만 채운다.
public static class LeagueQuery
{
    public enum Outcome { Lose = -1, Draw = 0, Win = 1 }

    public struct MatchRecord
    {
        public int round;
        public int opponentId;
        public Outcome outcome;
    }

    public struct HeadToHead
    {
        public int win, draw, lose;
    }

    public static League Current
    {
        get
        {
            var lm = LeagueManager.Instance;
            return lm != null && lm.league != null && lm.league.teams != null ? lm.league : null;
        }
    }

    public static bool IsReady => Current != null;

    public static int PlayerTeamId => Current != null ? Current.settings.playerTeamId : -1;

    public static Team FindTeam(int teamId) => Current?.teams.Find(t => t.id == teamId);

    public static Team PlayerTeam => FindTeam(PlayerTeamId);

    // 치른 경기 수 (= 다음 경기 회차 - 1)
    public static int PlayedCount => PlayerTeam != null ? PlayerTeam.played : 0;

    public static int TotalRounds => Current != null ? Current.settings.totalRounds : 0;

    public static int Tier => Current != null ? Current.settings.tier : 0;

    // 리그 등급 이름 (기존 LeagueManager.GetTierName, Localization 적용됨)
    public static string TierName => LeagueManager.Instance != null ? LeagueManager.Instance.GetTierName(Tier) : "";

    // 경기 결과의 점수는 순위용 값(1:0, 2:0)이라 승/무/패 판정에만 쓴다.
    public static Outcome GetOutcome(LeagueMatch match, int teamId)
    {
        if (match == null || match.result == null) return Outcome.Draw;
        int mine = match.teamAId == teamId ? match.result.scoreA : match.result.scoreB;
        int theirs = match.teamAId == teamId ? match.result.scoreB : match.result.scoreA;
        return mine > theirs ? Outcome.Win : mine < theirs ? Outcome.Lose : Outcome.Draw;
    }

    public static int OpponentOf(LeagueMatch match, int teamId) => match.teamAId == teamId ? match.teamBId : match.teamAId;

    public static LeagueMatch FindMatch(int round, int teamId)
    {
        var league = Current;
        if (league == null) return null;
        var r = league.schedule.Find(x => x.roundNumber == round);
        return r?.matches.Find(m => m.teamAId == teamId || m.teamBId == teamId);
    }

    // 내 다음 경기 상대 (리그가 끝났으면 null)
    public static Team NextOpponent()
    {
        int me = PlayerTeamId;
        var match = FindMatch(PlayedCount + 1, me);
        return match == null ? null : FindTeam(OpponentOf(match, me));
    }

    // 내 최근 경기 결과, 최신 순으로 최대 count개
    public static List<MatchRecord> RecentResults(int count)
    {
        var list = new List<MatchRecord>();
        int me = PlayerTeamId;
        for (int round = PlayedCount; round >= 1 && list.Count < count; round--)
        {
            var match = FindMatch(round, me);
            if (match == null) continue;
            list.Add(new MatchRecord { round = round, opponentId = OpponentOf(match, me), outcome = GetOutcome(match, me) });
        }
        return list;
    }

    // 이번 리그에서 내 팀과 상대의 전적
    public static HeadToHead GetHeadToHead(int opponentId)
    {
        var h = new HeadToHead();
        int me = PlayerTeamId;
        for (int round = 1; round <= PlayedCount; round++)
        {
            var match = FindMatch(round, me);
            if (match == null || OpponentOf(match, me) != opponentId) continue;
            switch (GetOutcome(match, me))
            {
                case Outcome.Win: h.win++; break;
                case Outcome.Lose: h.lose++; break;
                default: h.draw++; break;
            }
        }
        return h;
    }

    // 지금 진행할 라운드 번호 (리그가 끝났으면 마지막 라운드)
    public static int CurrentRound => TotalRounds > 0 ? Mathf.Min(PlayedCount + 1, TotalRounds) : PlayedCount + 1;

    // 남은 경기 수
    public static int RemainingMatches => Mathf.Max(0, TotalRounds - PlayedCount);

    // 순위표 순서 (LeagueManager.CalculateRanking이 매긴 rank 순, 같은 순위는 원래 순서 유지)
    public static List<Team> Standings()
    {
        var league = Current;
        return league == null ? new List<Team>() : league.teams.OrderBy(t => t.rank).ToList();
    }

    // 한 라운드의 경기 목록 (내 경기를 맨 앞으로)
    public static List<LeagueMatch> RoundMatches(int round)
    {
        var league = Current;
        var r = league?.schedule.Find(x => x.roundNumber == round);
        if (r == null) return new List<LeagueMatch>();
        int me = PlayerTeamId;
        return r.matches.OrderBy(m => m.teamAId == me || m.teamBId == me ? 0 : 1).ToList();
    }

    public static int WinRate(Team team) => team == null || team.played == 0 ? 0 : Mathf.RoundToInt(team.win * 100f / team.played);

    // 가문 깃발: 기존 LeagueManager.GetTeamSprite를 그대로 쓴다.
    public static Sprite TeamSprite(int teamId) => LeagueManager.Instance != null ? LeagueManager.Instance.GetTeamSprite(teamId) : null;
}
