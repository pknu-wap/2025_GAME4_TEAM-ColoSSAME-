using BattleK.UI.Book;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// "일" 페이지(Home) 표시 전용. 데이터 계산은 LeagueQuery(읽기 전용)에 맡기고 글자·그림만 채운다.
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

    [Header("대회 열기 (상대 가문·확인된 용병은 대회 페이지에서 확인)")]
    [SerializeField] private Button newsButton;
    [SerializeField] private BookPageController pageController;

    private static readonly Color Ink = new Color32(0x59, 0x2a, 0x16, 0xff);
    private static readonly Color Win = new Color32(0x2e, 0x5e, 0x22, 0xff);
    private static readonly Color Lose = new Color32(0x7e, 0x20, 0x20, 0xff);

    private void Awake()
    {
        if (newsButton != null) newsButton.onClick.AddListener(OpenLeague);
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

    // 임시: LeagueManager에 리그 갱신 이벤트가 없어서, 치른 경기 수가 바뀌면 다시 그린다.
    // (페이지를 연 채 라운드 결과가 처리되는 경우. 이벤트가 생기면 이 부분을 이벤트 구독으로 바꾼다)
    private int shownPlayed = -1;

    private void Update()
    {
        if (LeagueQuery.IsReady && LeagueQuery.PlayedCount != shownPlayed) Refresh();
    }

    private void OpenLeague()
    {
        if (pageController == null) pageController = FindObjectOfType<BookPageController>();
        if (pageController != null) pageController.OpenPage(BookPageId.League);
    }

    public void Refresh()
    {
        if (!LeagueQuery.IsReady) return;
        shownPlayed = LeagueQuery.PlayedCount;
        Team me = LeagueQuery.PlayerTeam;
        if (me == null) return;

        int played = shownPlayed;
        bool first = played == 0;

        // 내 가문 카드
        SetCrest(myCrest, me.id);
        myName.text = TeamLabel(me, first);
        myDescription.text = FirstLine(Loc.TeamDescription(me));

        // 승·무·패·승률 (값이 0이면 기본 글자색)
        SetStat(0, me.win, me.win > 0 ? Win : Ink);
        SetStat(1, me.draw, Ink);
        SetStat(2, me.lose, me.lose > 0 ? Lose : Ink);
        statValues[3].text = first ? "—" : Mathf.RoundToInt(me.win * 100f / played) + "%";
        statValues[3].color = Ink;

        // 진행도 + 최근 경기
        progressText.text = Loc.Get("home_progress", played, LeagueQuery.TotalRounds);
        var recent = LeagueQuery.RecentResults(recentRows.Length);
        if (firstMatchGuide != null) firstMatchGuide.SetActive(first);
        for (int i = 0; i < recentRows.Length; i++)
        {
            var row = recentRows[i];
            bool has = i < recent.Count;
            row.filled.SetActive(has);
            if (row.emptyLine != null) row.emptyLine.SetActive(!first && !has);
            if (!has) continue;

            var record = recent[i];
            row.round.text = Loc.Get("home_round", record.round);
            row.opponent.text = Loc.TeamName(LeagueQuery.FindTeam(record.opponentId));
            row.result.text = Loc.Get(ResultKey(record.outcome));
            row.result.color = ResultColor(record.outcome);
        }

        // 다음 상대 + 상대전적 (이번 리그 안)
        Team enemy = LeagueQuery.NextOpponent();
        enemyCrest.enabled = enemy != null;
        if (enemy == null)
        {
            enemyName.text = "—";
            headToHead.text = "";
            return;
        }
        SetCrest(enemyCrest, enemy.id);
        enemyName.text = TeamLabel(enemy, first);
        var h = LeagueQuery.GetHeadToHead(enemy.id);
        headToHead.text = h.draw > 0 ? Loc.Get("home_h2h_draw", h.win, h.draw, h.lose) : Loc.Get("home_h2h", h.win, h.lose);
    }

    // 첫 경기 전에는 순위가 의미 없어 숨긴다.
    private static string TeamLabel(Team team, bool first) =>
        first ? Loc.Get("home_team", Loc.TeamName(team)) : Loc.Get("home_team_rank", Loc.TeamName(team), team.rank);

    private static string ResultKey(LeagueQuery.Outcome o) =>
        o == LeagueQuery.Outcome.Win ? "home_result_win" : o == LeagueQuery.Outcome.Lose ? "home_result_lose" : "home_result_draw";

    private static Color ResultColor(LeagueQuery.Outcome o) =>
        o == LeagueQuery.Outcome.Win ? Win : o == LeagueQuery.Outcome.Lose ? Lose : Ink;

    private void SetStat(int i, int value, Color color)
    {
        statValues[i].text = value.ToString();
        statValues[i].color = color;
    }

    private static void SetCrest(Image image, int teamId)
    {
        var sprite = LeagueQuery.TeamSprite(teamId);
        if (sprite != null) image.sprite = sprite;
    }

    private static string FirstLine(string text)
    {
        if (string.IsNullOrEmpty(text)) return "";
        string line = text.Split('\n')[0];
        return line.Trim('“', '”', '"', ' ');
    }
}
