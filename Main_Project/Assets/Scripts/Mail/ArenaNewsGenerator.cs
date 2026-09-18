using System.Collections.Generic;
using UnityEngine;

// 소식통 3칸: 리그 소식 / 경기장 소문 / 우리 팀 여론
public class NewsBundle
{
    public string leagueNews;   // 이번 라운드 실제 사건 중 점수 최고
    public string rumor;        // 다른 팀 유닛 소식 (레벨/스킬/영입/부상)
    public string opinion;      // 우리 팀에 대한 팬 여론
}

// 선택 로직
public class ArenaNewsGenerator
{
    private readonly ArenaNewsTemplateSO templates;
    private readonly int streakThreshold;   // 연승 연패 기준

    private System.Random rng;

    public ArenaNewsGenerator(ArenaNewsTemplateSO templates, int streakThreshold = 3)
    {
        this.templates = templates;
        this.streakThreshold = streakThreshold;
    }

    private class ArenaNews
    {
        public string text;
        public int score;
        public int teamId;
        public NewsEventType type;
    }


    public NewsBundle Generate(League league)
    {
        if (templates == null || league == null) return new NewsBundle { leagueNews = "", rumor = "", opinion = "" };

        int lastRound = FindLastCompletedRound(league);
        rng = new System.Random(lastRound);   // 같은 라운드엔 같은 뉴스

        return new NewsBundle
        {
            leagueNews = BuildLeagueNews(league, lastRound),
            rumor = BuildRumor(league, lastRound),
            opinion = BuildOpinion(league, lastRound),
        };
    }

    // 리그 소식
    private string BuildLeagueNews(League league, int lastRound)
    {
        if (lastRound == 0) return Pick(templates.openingLines);

        var pool = CollectRoundPool(league, lastRound);

        if (lastRound > 1)
        {
            var prevPool = CollectRoundPool(league, lastRound - 1);
            if (prevPool.Count > 0)
            {
                var prevType = prevPool[0].type;
                foreach (var n in pool)
                    if (n.type == prevType) n.score -= templates.repeatPenalty;
                pool.Sort((a, b) => b.score.CompareTo(a.score));
            }
        }

        return pool.Count > 0 ? pool[0].text : Pick(templates.openingLines);
    }

    // 2경기장 소문
    private string BuildRumor(League league, int lastRound)
    {
        // 리그 시작 직후엔 승급/영입 이벤트
        var events = league.unitEvents?.FindAll(e => e.round == lastRound);
        if (events == null || events.Count == 0) return Pick(templates.noRumorLines);

        UnitNewsEvent best = null;
        List<string> bestTemplates = null;
        int bestScore = int.MinValue;

        foreach (var e in events)
        {
            if (!templates.TryGetRumor(e.type, out int baseScore, out var tpls)) continue;

            int score = baseScore
                      + (e.type == UnitNewsType.LevelJump ? e.amount * 10 : 0)
                      + (e.teamId == league.currentEnemyTeamId ? templates.nextOpponentBonus : 0)
                      + rng.Next(0, 10);   // 동점 흔들기

            if (score > bestScore) { bestScore = score; best = e; bestTemplates = tpls; }
        }
        if (best == null) return Pick(templates.noRumorLines);

        return FillRumor(Pick(bestTemplates), best);
    }

    // 우리 팀 여론(직전 결과×순위×연승/연패)
    private string BuildOpinion(League league, int lastRound)
    {
        var me = league.teams.Find(t => t.id == league.settings.playerTeamId);
        if (me == null || lastRound == 0)
            return FillOpinion(Pick(templates.GetOpinion(OpinionMood.Preseason)), me, 0);

        var hist = GetHist(BuildTeamHistory(league, lastRound), me.id);
        char last = hist.Count > 0 ? hist[hist.Count - 1] : 'N';
        int winStreak = GetCurrentStreak(hist, 'W');
        int loseStreak = GetCurrentStreak(hist, 'L');
        bool isLast = me.rank >= league.teams.Count;

        OpinionMood mood =
            winStreak >= streakThreshold ? OpinionMood.Hype :
            loseStreak >= streakThreshold ? OpinionMood.Crisis :
            last == 'W' && me.rank == 1 ? OpinionMood.Top :
            last == 'W' ? OpinionMood.Win :
            last == 'L' && isLast ? OpinionMood.Bottom :
            last == 'L' ? OpinionMood.Lose :
            last == 'D' ? OpinionMood.Draw : OpinionMood.Preseason;

        int streak = mood == OpinionMood.Hype ? winStreak : loseStreak;
        return FillOpinion(Pick(templates.GetOpinion(mood)), me, streak);
    }

    // 리그 소식 수집

    private List<ArenaNews> CollectRoundPool(League league, int roundNo)
    {
        var cur = BuildTeamHistory(league, roundNo);
        var prev = BuildTeamHistory(league, roundNo - 1);
        var round = league.schedule.Find(r => r.roundNumber == roundNo);

        var pool = new List<ArenaNews>();
        if (round?.matches != null)
            foreach (var match in round.matches)
                if (match.result != null)
                    CollectMatchEvents(pool, league, match, cur, prev);

        pool = DeduplicateByTeam(pool);
        pool.Sort((a, b) => b.score.CompareTo(a.score));
        return pool;
    }

    private void CollectMatchEvents(
    List<ArenaNews> pool, League league, LeagueMatch match,
    Dictionary<int, List<char>> cur, Dictionary<int, List<char>> prev)
    {
        Team teamA = league.teams.Find(t => t.id == match.teamAId);
        Team teamB = league.teams.Find(t => t.id == match.teamBId);
        if (teamA == null || teamB == null) return;

        // 무승부
        if (match.result.winner == 0)
        {
            Add(pool, NewsEventType.Draw, teamA, teamB, 0, teamA.id, 0);
            return;
        }

        int winnerId = match.result.winner;
        int loserId = winnerId == match.teamAId ? match.teamBId : match.teamAId;
        Team winner = league.teams.Find(t => t.id == winnerId);
        Team loser = league.teams.Find(t => t.id == loserId);
        if (winner == null || loser == null) return;

        bool isPlayer = winner.id == league.settings.playerTeamId
                     || loser.id == league.settings.playerTeamId;
        int bonus = isPlayer ? 20 : 0;

        int winStreak = GetCurrentStreak(GetHist(cur, winner.id), 'W');
        int loseStreak = GetCurrentStreak(GetHist(cur, loser.id), 'L');
        int prevLoseStreak = GetCurrentStreak(GetHist(prev, winner.id), 'L');
        int prevWinStreak = GetCurrentStreak(GetHist(prev, loser.id), 'W');

        // 종류별 이벤트 검사
        CheckStreakEvents(pool, winner, loser, winStreak, loseStreak, prevWinStreak, prevLoseStreak, bonus);
        CheckRankEvents(pool, league, winner, loser, bonus);
        CheckFirstResultEvents(pool, winner, loser, bonus);

        // 일반 승리
        Add(pool, NewsEventType.PlainWin, winner, loser, 0, winner.id, isPlayer ? 10 : 0);
    }

    private void CheckStreakEvents(
    List<ArenaNews> pool, Team winner, Team loser,
    int winStreak, int loseStreak, int prevWinStreak, int prevLoseStreak, int bonus)
    {
        if (winStreak >= 10)
            Add(pool, NewsEventType.WinStreakMax, winner, loser, winStreak, winner.id, bonus);

        if (winStreak == 5)
            Add(pool, NewsEventType.WinStreak5, winner, loser, winStreak, winner.id, bonus);

        if (loseStreak == 5)
            Add(pool, NewsEventType.LoseStreak5, winner, loser, loseStreak, loser.id, bonus);

        if (prevLoseStreak >= streakThreshold)
            Add(pool, NewsEventType.BreakLoseStreak, winner, loser, prevLoseStreak, winner.id, bonus);

        if (prevWinStreak >= streakThreshold)
            Add(pool, NewsEventType.EndWinStreak, winner, loser, prevWinStreak, loser.id, bonus);

        if (winStreak == streakThreshold)
            Add(pool, NewsEventType.WinStreakN, winner, loser, winStreak, winner.id, bonus);

        if (loseStreak == streakThreshold)
            Add(pool, NewsEventType.LoseStreakN, winner, loser, loseStreak, loser.id, bonus);
    }

    private void CheckRankEvents(
    List<ArenaNews> pool, League league, Team winner, Team loser, int bonus)
    {
        if (winner.rank == 1)
            Add(pool, NewsEventType.TopRank, winner, loser, 0, winner.id, bonus);

        if (winner.rank >= league.teams.Count - 1 && loser.rank <= 2)
            Add(pool, NewsEventType.Upset, winner, loser, 0, winner.id, 0);
    }

    private void CheckFirstResultEvents(
    List<ArenaNews> pool, Team winner, Team loser, int bonus)
    {
        if (winner.win == 1 && winner.draw == 0)
            Add(pool, NewsEventType.FirstWin, winner, loser, 0, winner.id, bonus);

        if (loser.lose == 1)
            Add(pool, NewsEventType.FirstLose, winner, loser, 0, loser.id, bonus);
    }

    private void Add(List<ArenaNews> pool, NewsEventType type,
                     Team winner, Team loser, int streak, int subjectId, int bonus)
    {
        if (!templates.TryGetLeague(type, out int score, out var tpls)) return;
        pool.Add(new ArenaNews
        {
            text = Fill(Pick(tpls), winner, loser, streak),
            score = score + bonus,
            teamId = subjectId,
            type = type,
        });
    }

    private string Fill(string t, Team winner, Team loser, int streak)
    {
        string wn = winner?.name ?? "", ln = loser?.name ?? "";
        return t
            .Replace("{winnerEul}", wn == "" ? "" : wn + KoreanParticle.Get(wn, Particle.EulReul))
            .Replace("{loserEul}", ln == "" ? "" : ln + KoreanParticle.Get(ln, Particle.EulReul))
            .Replace("{winnerWa}", wn == "" ? "" : wn + KoreanParticle.Get(wn, Particle.GwaWa))
            .Replace("{loserWa}", ln == "" ? "" : ln + KoreanParticle.Get(ln, Particle.GwaWa))
            .Replace("{winner}", wn)
            .Replace("{loser}", ln)
            .Replace("{streak}", streak.ToString());
    }

    private string FillRumor(string t, UnitNewsEvent e)
    {
        string un = e.unitName ?? "";
        return t
            .Replace("{unitEul}", un == "" ? "" : un + KoreanParticle.Get(un, Particle.EulReul))
            .Replace("{unit}", un)
            .Replace("{team}", e.teamName ?? "")
            .Replace("{detail}", e.detail ?? "");
    }

    private string FillOpinion(string t, Team me, int streak)
    {
        return t
            .Replace("{team}", me?.name ?? "우리 팀")
            .Replace("{rank}", (me?.rank ?? 0).ToString())
            .Replace("{streak}", streak.ToString());
    }

    private string Pick(IReadOnlyList<string> lines)
        => lines == null || lines.Count == 0 ? "" : lines[rng.Next(lines.Count)];


    private List<ArenaNews> DeduplicateByTeam(List<ArenaNews> pool)
    {
        var best = new Dictionary<int, ArenaNews>();
        foreach (var n in pool)
            if (!best.ContainsKey(n.teamId) || best[n.teamId].score < n.score)
                best[n.teamId] = n;
        return new List<ArenaNews>(best.Values);
    }

    private int FindLastCompletedRound(League league)
    {
        int last = 0;
        foreach (var round in league.schedule)
        {
            if (round.matches == null) continue;
            foreach (var match in round.matches)
                if (match.result != null) { last = Mathf.Max(last, round.roundNumber); break; }
        }
        return last;
    }

    private List<char> GetHist(Dictionary<int, List<char>> dict, int id)
        => dict.ContainsKey(id) ? dict[id] : new List<char>();

    private Dictionary<int, List<char>> BuildTeamHistory(League league, int upToRound)
    {
        var hist = new Dictionary<int, List<char>>();
        foreach (var t in league.teams) hist[t.id] = new List<char>();

        var sorted = new List<Round>(league.schedule);
        sorted.Sort((a, b) => a.roundNumber.CompareTo(b.roundNumber));

        foreach (var round in sorted)
        {
            if (round.roundNumber > upToRound) break;
            if (round.matches == null) continue;
            foreach (var match in round.matches)
            {
                if (match.result == null) continue;
                if (!hist.ContainsKey(match.teamAId) || !hist.ContainsKey(match.teamBId)) continue;

                int w = match.result.winner;
                if (w == 0) { hist[match.teamAId].Add('D'); hist[match.teamBId].Add('D'); }
                else if (w == match.teamAId) { hist[match.teamAId].Add('W'); hist[match.teamBId].Add('L'); }
                else { hist[match.teamAId].Add('L'); hist[match.teamBId].Add('W'); }
            }
        }
        return hist;
    }

    private int GetCurrentStreak(List<char> history, char target)
    {
        int count = 0;
        for (int i = history.Count - 1; i >= 0; i--)
        {
            if (history[i] == target) count++;
            else break;
        }
        return count;
    }
}
