using System.Linq;
using BattleK.UI.Book;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// "일" 페이지(Home) 표시 전용. LeagueManager·UserManager 값을 읽기만 하고 수정하지 않는다.
public class HomePageView : MonoBehaviour
{
    [System.Serializable]
    public class RecentRow
    {
        public GameObject filled;   // 배경 + 글자 묶음
        public GameObject emptyLine; // 아직 치르지 않은 칸의 밑줄
        public TMP_Text round;
        public TMP_Text opponent;
        public TMP_Text result;
    }

    [Header("내 가문")]
    [SerializeField] private Image myCrest;
    [SerializeField] private TMP_Text myName;
    [SerializeField] private TMP_Text myDescription;
    [SerializeField] private TMP_Text[] statValues = new TMP_Text[4]; // 승, 무, 패, 승률

    [Header("최근 경기")]
    [SerializeField] private TMP_Text progressText;
    [SerializeField] private RecentRow[] recentRows;
    [SerializeField] private GameObject firstMatchGuide;

    [Header("다음 상대")]
    [SerializeField] private Image enemyCrest;
    [SerializeField] private TMP_Text enemyName;
    [SerializeField] private TMP_Text headToHead;

    [Header("소식통 열기")]
    [SerializeField] private Button newsButton;
    [SerializeField] private BookPageController pageController;

    private static readonly Color Ink = new Color32(0x59, 0x2a, 0x16, 0xff);
    private static readonly Color Win = new Color32(0x2e, 0x5e, 0x22, 0xff);
    private static readonly Color Lose = new Color32(0x7e, 0x20, 0x20, 0xff);

    private void Awake()
    {
        if (newsButton != null) newsButton.onClick.AddListener(OpenNews);
    }

    private void OnEnable()
    {
        Loc.Changed += Refresh;
        Refresh();
    }

    private void OnDisable()
    {
        Loc.Changed -= Refresh;
    }

    private void OpenNews()
    {
        if (pageController == null) pageController = FindObjectOfType<BookPageController>();
        if (pageController != null) pageController.OpenPage(BookPageId.News);
    }

    public void Refresh()
    {
        var lm = LeagueManager.Instance;
        if (lm == null || lm.league == null || lm.league.teams == null) return;
        var league = lm.league;
        int myId = league.settings.playerTeamId;
        Team me = league.teams.Find(t => t.id == myId);
        if (me == null) return;

        int played = me.played;
        bool first = played == 0;

        // 내 가문 카드
        SetCrest(myCrest, me.id);
        myName.text = first ? Loc.Get("home_team", Loc.TeamName(me)) : Loc.Get("home_team_rank", Loc.TeamName(me), me.rank);
        myDescription.text = FirstLine(Loc.TeamDescription(me));

        // 승·무·패·승률 (값이 0이면 기본 글자색)
        SetStat(0, me.win, me.win > 0 ? Win : Ink);
        SetStat(1, me.draw, Ink);
        SetStat(2, me.lose, me.lose > 0 ? Lose : Ink);
        statValues[3].text = first ? "—" : Mathf.RoundToInt(me.win * 100f / played) + "%";
        statValues[3].color = Ink;

        // 진행도 + 최근 경기
        progressText.text = Loc.Get("home_progress", played, league.settings.totalRounds);
        var myMatches = league.schedule
            .Where(r => r.roundNumber <= played)
            .OrderByDescending(r => r.roundNumber)
            .Select(r => new { r.roundNumber, m = r.matches.Find(x => x.teamAId == myId || x.teamBId == myId) })
            .Where(x => x.m != null)
            .ToList();

        if (firstMatchGuide != null) firstMatchGuide.SetActive(first);
        for (int i = 0; i < recentRows.Length; i++)
        {
            var row = recentRows[i];
            bool has = !first && i < myMatches.Count;
            row.filled.SetActive(has);
            if (row.emptyLine != null) row.emptyLine.SetActive(!first && !has);
            if (!has) continue;

            var x = myMatches[i];
            int oppId = x.m.teamAId == myId ? x.m.teamBId : x.m.teamAId;
            int outcome = Outcome(x.m, myId);
            row.round.text = Loc.Get("home_round", x.roundNumber);
            row.opponent.text = Loc.TeamName(league.teams.Find(t => t.id == oppId));
            row.result.text = Loc.Get(outcome > 0 ? "home_result_win" : outcome < 0 ? "home_result_lose" : "home_result_draw");
            row.result.color = outcome > 0 ? Win : outcome < 0 ? Lose : Ink;
        }

        // 다음 상대
        var next = league.schedule.Find(r => r.roundNumber == played + 1);
        var nextMatch = next == null ? null : next.matches.Find(m => m.teamAId == myId || m.teamBId == myId);
        Team enemy = nextMatch == null ? null : league.teams.Find(t => t.id == (nextMatch.teamAId == myId ? nextMatch.teamBId : nextMatch.teamAId));
        if (enemy == null)
        {
            enemyCrest.enabled = false;
            enemyName.text = "—";
            headToHead.text = "";
            return;
        }
        enemyCrest.enabled = true;
        SetCrest(enemyCrest, enemy.id);
        enemyName.text = first ? Loc.Get("home_team", Loc.TeamName(enemy)) : Loc.Get("home_team_rank", Loc.TeamName(enemy), enemy.rank);

        // 상대전적 (이번 리그 안)
        int w = 0, d = 0, l = 0;
        foreach (var x in myMatches)
        {
            int oppId = x.m.teamAId == myId ? x.m.teamBId : x.m.teamAId;
            if (oppId != enemy.id) continue;
            int o = Outcome(x.m, myId);
            if (o > 0) w++; else if (o < 0) l++; else d++;
        }
        headToHead.text = d > 0 ? Loc.Get("home_h2h_draw", w, d, l) : Loc.Get("home_h2h", w, l);
    }

    private void SetStat(int i, int value, Color color)
    {
        statValues[i].text = value.ToString();
        statValues[i].color = color;
    }

    // 1 = 승, 0 = 무, -1 = 패. 경기 결과의 점수는 승패 판정용 값만 쓴다.
    private static int Outcome(LeagueMatch m, int myId)
    {
        if (m.result == null) return 0;
        int mine = m.teamAId == myId ? m.result.scoreA : m.result.scoreB;
        int theirs = m.teamAId == myId ? m.result.scoreB : m.result.scoreA;
        return mine > theirs ? 1 : mine < theirs ? -1 : 0;
    }

    private static void SetCrest(Image image, int teamId)
    {
        var sprite = Resources.Load<Sprite>($"TeamImages/team_{teamId}");
        if (sprite != null) image.sprite = sprite;
    }

    private static string FirstLine(string text)
    {
        if (string.IsNullOrEmpty(text)) return "";
        string line = text.Split('\n')[0];
        return line.Trim('“', '”', '"', ' ');
    }
}
