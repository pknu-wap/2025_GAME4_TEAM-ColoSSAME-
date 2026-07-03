using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ArenaNewsManager : MonoBehaviour
{
    [Header("UI - 소식 텍스트 5개 연결")]
    public TextMeshProUGUI[] newsTexts = new TextMeshProUGUI[5];

    [Header("연승/연패 기준")]
    public int streakThreshold = 3;

    // ── 내부 데이터 ──────────────────────────────────────────────────────────

    private class ArenaNews
    {
        public string text;
        public int    score;
        public int    teamId;
    }

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

    // ── 진입점 ───────────────────────────────────────────────────────────────

    private void Start() => RefreshNews();

    public void RefreshNews()
    {
        if (LeagueManager.Instance == null || LeagueManager.Instance.league == null)
        {
            if (newsTexts.Length > 0 && newsTexts[0] != null)
                newsTexts[0].text = "리그 정보를 불러오는 중...";
            return;
        }

        var news = GenerateNews();
        for (int i = 0; i < newsTexts.Length; i++)
        {
            if (newsTexts[i] == null) continue;
            newsTexts[i].text = i < news.Count ? news[i] : "";
        }
    }

    // ── 뉴스 생성 ────────────────────────────────────────────────────────────

    private List<string> GenerateNews()
    {
        var league    = LeagueManager.Instance.league;
        int lastRound = FindLastCompletedRound(league);

        // 경기가 아예 없으면 개막 텍스트
        if (lastRound == 0) return GetOpeningLines(5);

        var currentHistory = BuildTeamHistory(league, lastRound);
        var prevHistory    = BuildTeamHistory(league, lastRound - 1);
        var round          = league.schedule.Find(r => r.roundNumber == lastRound);

        var pool = new List<ArenaNews>();

        if (round?.matches != null)
        {
            foreach (var match in round.matches)
            {
                if (match.result == null) continue;
                CollectMatchEvents(pool, league, match, currentHistory, prevHistory);
            }
        }

        // 팀별 중복 제거 → 각 팀에서 점수 가장 높은 뉴스만 유지
        pool = DeduplicateByTeam(pool);

        // 점수 내림차순 정렬
        pool.Sort((a, b) => b.score.CompareTo(a.score));

        var result = new List<string>();
        for (int i = 0; i < Mathf.Min(5, pool.Count); i++)
            result.Add(pool[i].text);

        // 5개 미만이면 개막 텍스트로 채우기
        if (result.Count < 5)
            foreach (var line in GetOpeningLines(5 - result.Count))
            {
                if (result.Count >= 5) break;
                result.Add(line);
            }

        return result;
    }

    // ── 이벤트 수집 ──────────────────────────────────────────────────────────

    private void CollectMatchEvents(
        List<ArenaNews>               pool,
        League                        league,
        LeagueMatch                   match,
        Dictionary<int, List<char>>   cur,
        Dictionary<int, List<char>>   prev)
    {
        int myId   = league.settings.playerTeamId;
        bool isDraw = match.result.winner == 0;

        Team teamA = league.teams.Find(t => t.id == match.teamAId);
        Team teamB = league.teams.Find(t => t.id == match.teamBId);
        if (teamA == null || teamB == null) return;

        // 무승부
        if (isDraw)
        {
            pool.Add(Make(
                Pick($"{teamA.name}{Wa(teamA.name)} {teamB.name}, 무승부로 승점 1점씩 나누다",
                     $"{teamA.name} vs {teamB.name}, 팽팽한 접전 끝에 무승부"),
                30, teamA.id));
            return;
        }

        int  winnerId    = match.result.winner;
        int  loserId     = winnerId == match.teamAId ? match.teamBId : match.teamAId;
        Team winner      = league.teams.Find(t => t.id == winnerId);
        Team loser       = league.teams.Find(t => t.id == loserId);
        if (winner == null || loser == null) return;

        bool isPlayer    = winner.id == myId || loser.id == myId;
        int  pb          = isPlayer ? 20 : 0;   // player bonus

        var winCur  = GetHist(cur,  winner.id);
        var loseCur = GetHist(cur,  loser.id);
        var winPrev = GetHist(prev, winner.id);
        var losePrev= GetHist(prev, loser.id);

        int winStreak      = GetCurrentStreak(winCur,  'W');
        int loseStreak     = GetCurrentStreak(loseCur, 'L');
        int prevLoseStreak = GetCurrentStreak(winPrev, 'L');  // 승자의 직전 연패
        int prevWinStreak  = GetCurrentStreak(losePrev,'W');  // 패자의 직전 연승

        // ── Score 100 ────────────────────────────────────────────────────────

        // 10연승
        if (winStreak >= 10)
            pool.Add(Make(
                Pick($"{winner.name}, 파죽의 {winStreak}연승 — 막을 자가 없다",
                     $"{winner.name}, {winStreak}연승 달성으로 리그를 지배하다"),
                100 + pb, winner.id));

        // 단독 선두 등극
        if (winner.rank == 1)
            pool.Add(Make(
                Pick($"{winner.name}, {loser.name}{Eul(loser.name)} 꺾고 단독 선두 등극",
                     $"{winner.name}, 승리와 함께 리그 정상에 서다"),
                100 + pb, winner.id));

        // 최하위권이 선두권을 꺾는 이변
        if (winner.rank >= league.teams.Count - 1 && loser.rank <= 2)
            pool.Add(Make(
                Pick($"최하위 {winner.name}, 선두 {loser.name}{Eul(loser.name)} 꺾는 이변",
                     $"{winner.name}, {loser.name}{Eul(loser.name)} 상대로 대이변 연출"),
                100, winner.id));

        // ── Score 80 ────────────────────────────────────────────────────────

        // 5연승
        if (winStreak == 5)
            pool.Add(Make(
                Pick($"{winner.name}, 5연승으로 거침없이 질주하다",
                     $"{winner.name}, 파죽의 5연승 달성",
                     $"{winner.name}의 상승세가 멈추지 않는다 — 5연승"),
                80 + pb, winner.id));

        // 5연패
        if (loseStreak == 5)
            pool.Add(Make(
                Pick($"{loser.name}, 5연패 수렁에 빠지다",
                     $"{loser.name}, 5연패 — 반전의 계기가 필요하다",
                     $"{loser.name}의 침체가 깊어지다 — 5연패"),
                80 + pb, loser.id));

        // 연패 탈출
        if (prevLoseStreak >= streakThreshold)
            pool.Add(Make(
                Pick($"{winner.name}, {prevLoseStreak}연패를 끊고 반격에 나서다",
                     $"{winner.name}, 연패 탈출 성공 — 분위기 반전 노린다",
                     $"{winner.name}, 마침내 연패의 사슬을 끊다"),
                80 + pb, winner.id));

        // 연승 종료
        if (prevWinStreak >= streakThreshold)
            pool.Add(Make(
                Pick($"{loser.name}의 {prevWinStreak}연승 행진이 막을 내리다",
                     $"{loser.name}, 연승 마감 — {winner.name}에 발목 잡히다",
                     $"{winner.name}, {loser.name}의 연승을 끊어내다"),
                80 + pb, loser.id));

        // ── Score 60 ────────────────────────────────────────────────────────

        // 3연승 달성
        if (winStreak == streakThreshold)
            pool.Add(Make(
                Pick($"{winner.name}, {winStreak}연승으로 기세를 올리다",
                     $"{winner.name}, 또 한 번 승리하며 {winStreak}연승 달성",
                     $"{winner.name}, 연승 행진을 이어가다"),
                60 + pb, winner.id));

        // 3연패 달성
        if (loseStreak == streakThreshold)
            pool.Add(Make(
                Pick($"{loser.name}, {loseStreak}연패 — 위기에 빠지다",
                     $"{loser.name}, 연패 행진이 멈추지 않는다",
                     $"{loser.name}, {loseStreak}연패 수렁에서 헤어나오지 못하다"),
                60 + pb, loser.id));

        // 시즌 첫 승
        if (winner.win == 1 && winner.draw == 0)
            pool.Add(Make(
                Pick($"{winner.name}, 시즌 첫 승 신고",
                     $"{winner.name}, 첫 승리를 거두며 웃음을 되찾다",
                     $"{winner.name}, 마침내 시즌 첫 승을 따내다"),
                60 + pb, winner.id));

        // 시즌 첫 패
        if (loser.lose == 1)
            pool.Add(Make(
                Pick($"{loser.name}, 시즌 첫 패배를 당하다",
                     $"{loser.name}의 무패 행진, {winner.name}에 의해 막히다",
                     $"{loser.name}, 첫 패 — 무패 기록이 무너지다"),
                60 + pb, loser.id));

        // ── Score 40: 일반 경기 결과 ────────────────────────────────────────

        pool.Add(Make(
            Pick($"{winner.name}, {loser.name}{Eul(loser.name)} 꺾고 승점을 쌓다",
                 $"{winner.name}, {loser.name}{Eul(loser.name)} 상대로 값진 승리"),
            40 + (isPlayer ? 10 : 0), winner.id));
    }

    // ── 유틸 ─────────────────────────────────────────────────────────────────

    private ArenaNews Make(string text, int score, int teamId)
        => new ArenaNews { text = text, score = score, teamId = teamId };

    private string Pick(params string[] options)
        => options[Random.Range(0, options.Length)];

    /// <summary>받침 유무에 따라 을/를 반환</summary>
    private string Eul(string name)
    {
        if (string.IsNullOrEmpty(name)) return "을";
        char last = name[name.Length - 1];
        if (last >= 0xAC00 && last <= 0xD7A3)
            return (last - 0xAC00) % 28 == 0 ? "를" : "을";
        return "를";
    }

    /// <summary>받침 유무에 따라 와/과 반환</summary>
    private string Wa(string name)
    {
        if (string.IsNullOrEmpty(name)) return "과";
        char last = name[name.Length - 1];
        if (last >= 0xAC00 && last <= 0xD7A3)
            return (last - 0xAC00) % 28 == 0 ? "와" : "과";
        return "와";
    }

    private List<ArenaNews> DeduplicateByTeam(List<ArenaNews> pool)
    {
        var best = new Dictionary<int, ArenaNews>();
        foreach (var n in pool)
        {
            if (!best.ContainsKey(n.teamId) || best[n.teamId].score < n.score)
                best[n.teamId] = n;
        }
        return new List<ArenaNews>(best.Values);
    }

    private int FindLastCompletedRound(League league)
    {
        int last = 0;
        foreach (var round in league.schedule)
        {
            if (round.matches == null) continue;
            foreach (var match in round.matches)
            {
                if (match.result != null) { last = Mathf.Max(last, round.roundNumber); break; }
            }
        }
        return last;
    }

    private List<string> GetOpeningLines(int count)
    {
        var list = new List<string>(openingLines);
        Shuffle(list);
        return list.GetRange(0, Mathf.Min(count, list.Count));
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
                if (w == 0)
                {
                    hist[match.teamAId].Add('D');
                    hist[match.teamBId].Add('D');
                }
                else if (w == match.teamAId)
                {
                    hist[match.teamAId].Add('W');
                    hist[match.teamBId].Add('L');
                }
                else
                {
                    hist[match.teamAId].Add('L');
                    hist[match.teamBId].Add('W');
                }
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
            int j = Random.Range(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }
}
