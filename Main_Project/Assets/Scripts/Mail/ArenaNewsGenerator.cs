using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.ParticleSystem;

public class ArenaNewsGenerator
{
    // 연승 연패 기준
    private readonly int streakThreshold;

    private System.Random rng;

    public ArenaNewsGenerator(int streakThreshold = 3)
    {
        this.streakThreshold = streakThreshold;
    }

    private class ArenaNews { 
        public string text; 
        public int score; 
        public int teamId; }

    private static readonly Dictionary<NewsEventType, (int score, string[] templates)> NewsData = new()
    {
        [NewsEventType.Draw] = (30, new[]
        {
            "{winnerWa} {loser}, 무승부로 승점 1점씩 나누다",
            "{winner} vs {loser}, 팽팽한 접전 끝에 무승부",
        }),
        [NewsEventType.WinStreakMax] = (100, new[]
        {
            "{winner}, 파죽의 {streak}연승 — 막을 자가 없다",
            "{winner}, {streak}연승 달성으로 리그를 지배하다",
        }),
        [NewsEventType.TopRank] = (100, new[]
        {
            "{winner}, {loserEul} 꺾고 단독 선두 등극",
            "{winner}, 승리와 함께 리그 정상에 서다",
        }),
        [NewsEventType.Upset] = (100, new[]
        {
            "최하위 {winner}, 선두 {loserEul} 꺾는 이변",
            "{winner}, {loserEul} 상대로 대이변 연출",
        }),
        [NewsEventType.WinStreak5] = (80, new[]
        {
            "{winner}, 5연승으로 거침없이 질주하다",
            "{winner}, 파죽의 5연승 달성",
            "{winner}의 상승세가 멈추지 않는다 — 5연승",
        }),
        [NewsEventType.LoseStreak5] = (80, new[]
        {
            "{loser}, 5연패 수렁에 빠지다",
            "{loser}, 5연패 — 반전의 계기가 필요하다",
            "{loser}의 침체가 깊어지다 — 5연패",
        }),
        [NewsEventType.BreakLoseStreak] = (80, new[]
        {
            "{winner}, {streak}연패를 끊고 반격에 나서다",
            "{winner}, 연패 탈출 성공 — 분위기 반전 노린다",
            "{winner}, 마침내 연패의 사슬을 끊다",
        }),
        [NewsEventType.EndWinStreak] = (80, new[]
        {
            "{loser}의 {streak}연승 행진이 막을 내리다",
            "{loser}, 연승 마감 — {winner}에 발목 잡히다",
            "{winner}, {loser}의 연승을 끊어내다",
        }),
        [NewsEventType.WinStreakN] = (60, new[]
        {
            "{winner}, {streak}연승으로 기세를 올리다",
            "{winner}, 또 한 번 승리하며 {streak}연승 달성",
            "{winner}, 연승 행진을 이어가다",
        }),
        [NewsEventType.LoseStreakN] = (60, new[]
        {
            "{loser}, {streak}연패 — 위기에 빠지다",
            "{loser}, 연패 행진이 멈추지 않는다",
            "{loser}, {streak}연패 수렁에서 헤어나오지 못하다",
        }),
        [NewsEventType.FirstWin] = (60, new[]
        {
            "{winner}, 시즌 첫 승 신고",
            "{winner}, 첫 승리를 거두며 웃음을 되찾다",
            "{winner}, 마침내 시즌 첫 승을 따내다",
        }),
        [NewsEventType.FirstLose] = (60, new[]
        {
            "{loser}, 시즌 첫 패배를 당하다",
            "{loser}의 무패 행진, {winner}에 의해 막히다",
            "{loser}, 첫 패 — 무패 기록이 무너지다",
        }),
        [NewsEventType.PlainWin] = (40, new[]
        {
            "{winner}, {loserEul} 꺾고 승점을 쌓다",
            "{winner}, {loserEul} 상대로 값진 승리",
        }),
    };

    private readonly string[] openingLines =
    {
        "콜로세움 리그, 드디어 막을 올리다",
        "10개 가문이 왕좌를 두고 격돌한다",
        "카이루스 가문, 이번 시즌 우승 후보로 주목받다",
        "이그니스 가문의 새 전략, 이번 시즌 통할 것인가",
        "녹스 가문, 조용한 강자로 떠오르다",
        "플로라 가문, 치유의 전술로 이변을 노린다",
        "루멘 가문의 철벽 수비, 이번 시즌도 건재할까",
        "아스트라 가문, 예언대로라면 우승은 따놓은 당상",
        "테라 가문의 수호자들, 투기장에 입성하다",
        "모르스 가문, 망령의 힘으로 리그를 뒤흔든다"
    };

    public List<string> Generate(League league)
    {
        int lastRound = FindLastCompletedRound(league);
        rng = new System.Random(lastRound);

        if (lastRound == 0) return GetOpeningLines(5);

        var cur = BuildTeamHistory(league, lastRound);
        var prev = BuildTeamHistory(league, lastRound - 1);
        var round = league.schedule.Find(r => r.roundNumber == lastRound);

        var pool = new List<ArenaNews>();
        if (round?.matches != null)
            foreach (var match in round.matches)
                if (match.result != null)
                    CollectMatchEvents(pool, league, match, cur, prev);

        pool = DeduplicateByTeam(pool);
        pool.Sort((a, b) => b.score.CompareTo(a.score));

        var result = new List<string>();
        for (int i = 0; i < Mathf.Min(5, pool.Count); i++) result.Add(pool[i].text);
        foreach (var line in GetOpeningLines(5 - result.Count))
        {
            if (result.Count >= 5) break;
            result.Add(line);
        }
        return result;
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
        if (!NewsData.TryGetValue(type, out var data) || data.templates.Length == 0) return;
        string tpl = data.templates[rng.Next(0, data.templates.Length)];
        pool.Add(new ArenaNews
        {
            text = Fill(tpl, winner, loser, streak),
            score = data.score + bonus,
            teamId = subjectId
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

    private List<string> GetOpeningLines(int count)
    {
        var list = new List<string>(openingLines);
        Shuffle(list);
        return list.GetRange(0, Mathf.Min(count, list.Count));
    }

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

    private void Shuffle(List<string> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = rng.Next(i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }
}