using System.Collections;
using System.Collections.Generic;
using BattleK.Scripts.Data;
using BattleK.Scripts.Data.Stat;
using BattleK.Scripts.Manager;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// "대회" 페이지(League) 표시 전용. 계산은 LeagueQuery(읽기 전용)에 맡기고 글자·그림만 채운다.
// 탭 2개: 리그 순위(책 양면 순위표) / 경기 대진(이번 라운드 5경기 → 가문 선택 → 용병)
public class LeagueView : MonoBehaviour
{
    [System.Serializable]
    public class RankRow
    {
        public Button button;      // 줄 전체 (누르면 경기 대진으로)
        public Image leftBg, rightBg;
        public TMP_Text rank, house, tag;
        public Image crest;
        public TMP_Text[] values = new TMP_Text[5]; // 승, 무, 패, 승점, 승률
    }

    [System.Serializable]
    public class MatchRow
    {
        public Button button;
        public Image bg;
        public Outline selected;
        public Image crestA, crestB;
        public TMP_Text nameA, nameB, rankA, rankB, ourTag, vs;
    }

    [System.Serializable]
    public class SideTab
    {
        public Button button;
        public Image bg;
        public Outline selected;
        public Image crest;
        public TMP_Text house, usTag;
    }

    [System.Serializable]
    public class UnitCell
    {
        public Image bg;
        public GameObject known;
        public Image portrait;
        public TMP_Text label;
        public GameObject unknown;
        public TMP_Text unknownLabel;
    }

    [Header("탭")]
    [SerializeField] private Button rankTab;
    [SerializeField] private Button matchTab;
    [SerializeField] private TMP_Text rankTabLabel, matchTabLabel;
    [SerializeField] private GameObject rankPanel, matchPanel;
    [SerializeField] private Sprite tabOn, tabOff;

    [Header("진행 회차")]
    [SerializeField] private TMP_Text progressText;
    [SerializeField] private RectTransform progressFill;
    [SerializeField] private float progressWidth = 298.2f;

    [Header("리그 순위")]
    [SerializeField] private TMP_Text[] rankHeaders = new TMP_Text[7]; // 순위, 가문, 승, 무, 패, 승점, 승률
    [SerializeField] private RankRow[] rankRows;
    [SerializeField] private TMP_Text ruleText, gapText;

    [Header("경기 대진")]
    [SerializeField] private TMP_Text roundTitle, resultNote;
    [SerializeField] private MatchRow[] matchRows;
    [SerializeField] private SideTab[] sideTabs = new SideTab[2];
    [SerializeField] private Sprite rowOn, rowOff;
    [SerializeField] private TMP_Text unitsTitle, unitsNote;
    [SerializeField] private UnitCell[] cells;
    [SerializeField] private GameObject pager;
    [SerializeField] private Button prevPage, nextPage;
    [SerializeField] private TMP_Text pageText;
    [SerializeField] private Sprite iconOn, iconOff;

    private static readonly Color Ink = new Color32(0x59, 0x2a, 0x16, 0xff);
    private static readonly Color Ink2 = new Color32(0x75, 0x4b, 0x22, 0xff);
    private static readonly Color Cream = new Color32(0xfc, 0xd5, 0xa9, 0xff);
    private static readonly Color Gold = new Color32(0xa0, 0x62, 0x0f, 0xff);
    private static readonly Color Stripe = new Color32(0x75, 0x4b, 0x22, 0x14);
    private static readonly Color MineFill = new Color32(0xff, 0xf3, 0xdc, 0xd9);
    private static readonly Color Clear = new Color(0, 0, 0, 0);

    private const int CellsPerPage = 9;

    private bool showMatches;
    private int selectedMatch;
    private int selectedSide;
    private int unitPage;
    private int shownPlayed = -1;
    private List<LeagueMatch> matches = new List<LeagueMatch>();

    private readonly AddressableAssetLoader<Sprite> portraitLoader = new AddressableAssetLoader<Sprite>();
    private Coroutine portraitRoutine;

    private void Awake()
    {
        rankTab.onClick.AddListener(() => ShowTab(false));
        matchTab.onClick.AddListener(() => ShowTab(true));
        for (int i = 0; i < rankRows.Length; i++)
        {
            int index = i;
            rankRows[i].button.onClick.AddListener(() => OpenTeamFromRanking(index));
        }
        for (int i = 0; i < matchRows.Length; i++)
        {
            int index = i;
            matchRows[i].button.onClick.AddListener(() => SelectMatch(index, DefaultSide(index)));
        }
        for (int i = 0; i < sideTabs.Length; i++)
        {
            int index = i;
            sideTabs[i].button.onClick.AddListener(() => { selectedSide = index; unitPage = 0; RefreshMatches(); });
        }
        prevPage.onClick.AddListener(() => { unitPage--; RefreshUnits(); });
        nextPage.onClick.AddListener(() => { unitPage++; RefreshUnits(); });
    }

    private void OnEnable()
    {
        Loc.Changed += Refresh;
        // 페이지를 열 때마다 이번 라운드 내 경기를 기본으로 보여 준다
        selectedMatch = 0;
        selectedSide = DefaultSide(0);
        unitPage = 0;
        Refresh();
    }

    private void OnDisable()
    {
        Loc.Changed -= Refresh;
        if (portraitRoutine != null) StopCoroutine(portraitRoutine);
        portraitRoutine = null;
    }

    private void OnDestroy()
    {
        portraitLoader.ReleaseAll();
    }

    // 임시: LeagueManager에 리그 갱신 이벤트가 없어서, 치른 경기 수가 바뀌면 다시 그린다. (HomePageView와 같은 방식)
    private void Update()
    {
        if (LeagueQuery.IsReady && LeagueQuery.PlayedCount != shownPlayed) Refresh();
    }

    private void ShowTab(bool matchesTab)
    {
        showMatches = matchesTab;
        Refresh();
    }

    // 순위표에서 가문을 누르면 그 가문의 이번 라운드 경기를 경기 대진에서 연다
    private void OpenTeamFromRanking(int rowIndex)
    {
        var standings = LeagueQuery.Standings();
        if (rowIndex >= standings.Count) return;
        int teamId = standings[rowIndex].id;
        int m = matches.FindIndex(x => x.teamAId == teamId || x.teamBId == teamId);
        if (m < 0) return;
        showMatches = true;
        SelectMatch(m, matches[m].teamAId == teamId ? 0 : 1);
    }

    private void SelectMatch(int index, int side)
    {
        selectedMatch = index;
        selectedSide = side;
        unitPage = 0;
        Refresh();
    }

    // 내 경기는 상대 가문을, 다른 경기는 왼쪽 가문을 먼저 보여 준다
    private int DefaultSide(int matchIndex)
    {
        if (matchIndex >= matches.Count) return 0;
        var m = matches[matchIndex];
        int me = LeagueQuery.PlayerTeamId;
        if (m.teamAId == me) return 1;
        return 0;
    }

    public void Refresh()
    {
        if (!LeagueQuery.IsReady) return;
        bool roundChanged = shownPlayed != LeagueQuery.PlayedCount;
        shownPlayed = LeagueQuery.PlayedCount;
        matches = LeagueQuery.RoundMatches(LeagueQuery.CurrentRound);
        if (roundChanged || selectedMatch >= matches.Count)
        {
            selectedMatch = 0;
            selectedSide = DefaultSide(0);
            unitPage = 0;
        }

        rankTabLabel.text = Loc.Get("league_tab_rank");
        matchTabLabel.text = Loc.Get("league_tab_match");
        SetTab(rankTab, rankTabLabel, !showMatches);
        SetTab(matchTab, matchTabLabel, showMatches);
        rankPanel.SetActive(!showMatches);
        matchPanel.SetActive(showMatches);

        int round = LeagueQuery.CurrentRound, total = LeagueQuery.TotalRounds;
        progressText.text = Loc.Get("league_progress", round, total);
        if (progressFill != null)
            progressFill.sizeDelta = new Vector2(total > 0 ? progressWidth * LeagueQuery.PlayedCount / total : 0f, progressFill.sizeDelta.y);

        if (showMatches) RefreshMatches();
        else RefreshRanking();
    }

    private void SetTab(Button tab, TMP_Text label, bool on)
    {
        tab.image.sprite = on ? tabOn : tabOff;
        tab.interactable = !on;
        label.color = on ? Cream : Ink2;
    }

    // ===== 리그 순위 =====
    private void RefreshRanking()
    {
        string[] headerKeys = { "league_col_rank", "league_col_house", "league_col_w", "league_col_d", "league_col_l", "league_col_pts", "league_col_rate" };
        for (int i = 0; i < rankHeaders.Length && i < headerKeys.Length; i++) rankHeaders[i].text = Loc.Get(headerKeys[i]);

        var standings = LeagueQuery.Standings();
        int me = LeagueQuery.PlayerTeamId;
        for (int i = 0; i < rankRows.Length; i++)
        {
            var row = rankRows[i];
            bool has = i < standings.Count;
            row.button.gameObject.SetActive(has);
            if (!has) continue;

            Team t = standings[i];
            bool mine = t.id == me, played = t.played > 0, leader = played && t.rank == 1;
            Color bg = mine ? MineFill : (i % 2 == 0 ? Stripe : Clear);
            row.leftBg.color = bg;
            row.rightBg.color = bg;
            SetOutline(row.leftBg, mine);
            SetOutline(row.rightBg, mine);

            row.rank.text = played ? t.rank.ToString() : "—";
            row.rank.color = leader ? Gold : Ink2;
            row.crest.sprite = LeagueQuery.TeamSprite(t.id);
            row.house.text = Loc.TeamName(t);
            row.tag.text = mine ? Loc.Get("league_tag_us") : leader ? Loc.Get("league_tag_leader") : "";
            row.tag.color = mine ? Ink2 : Gold;

            row.values[0].text = t.win.ToString();
            row.values[1].text = t.draw.ToString();
            row.values[2].text = t.lose.ToString();
            row.values[3].text = t.points.ToString();
            row.values[4].text = played ? LeagueQuery.WinRate(t) + "%" : "—";
        }

        ruleText.text = Loc.Get("league_rule");
        Team myTeam = LeagueQuery.PlayerTeam;
        int left = LeagueQuery.RemainingMatches;
        if (myTeam == null || LeagueQuery.PlayedCount == 0)
        {
            var rule = LeagueQuery.Current.settings.pointRule;
            gapText.text = Loc.Get("league_gap_start", left, rule.win, rule.draw);
        }
        else if (myTeam.rank == 1)
            gapText.text = Loc.Get("league_gap_leading", left);
        else
            gapText.text = Loc.Get("league_gap_behind", standings[0].points - myTeam.points, left);
    }

    private static void SetOutline(Graphic g, bool on)
    {
        var o = g.GetComponent<Outline>();
        if (o != null) o.enabled = on;
    }

    // ===== 경기 대진 =====
    private void RefreshMatches()
    {
        roundTitle.text = Loc.Get("league_this_round");
        resultNote.text = Loc.Get("league_result_note");
        int me = LeagueQuery.PlayerTeamId;

        for (int i = 0; i < matchRows.Length; i++)
        {
            var row = matchRows[i];
            bool has = i < matches.Count;
            row.button.gameObject.SetActive(has);
            if (!has) continue;

            var m = matches[i];
            bool cur = i == selectedMatch, ours = m.teamAId == me || m.teamBId == me;
            row.bg.sprite = cur ? rowOn : rowOff;
            row.selected.enabled = cur;
            row.button.interactable = !cur;
            FillMatchSide(m.teamAId, row.crestA, row.nameA, row.rankA);
            FillMatchSide(m.teamBId, row.crestB, row.nameB, row.rankB);
            row.ourTag.gameObject.SetActive(ours);
            row.ourTag.text = Loc.Get("league_our_match");
            row.vs.color = ours ? Ink : Ink2;
            // 우리 경기는 "우리 경기" 아래로 VS를 내린다
            var vsRt = row.vs.rectTransform;
            vsRt.anchoredPosition = new Vector2(vsRt.anchoredPosition.x, ours ? -65.1f : -51.45f);
        }

        if (matches.Count == 0) return;
        var sel = matches[selectedMatch];
        int[] ids = { sel.teamAId, sel.teamBId };
        for (int i = 0; i < 2; i++)
        {
            var tab = sideTabs[i];
            bool on = i == selectedSide;
            Team t = LeagueQuery.FindTeam(ids[i]);
            tab.bg.sprite = on ? rowOn : rowOff;
            tab.bg.color = on ? Color.white : new Color(1, 1, 1, 0.8f);
            tab.selected.enabled = on;
            tab.button.interactable = !on;
            tab.crest.sprite = LeagueQuery.TeamSprite(ids[i]);
            tab.house.text = Loc.TeamName(t);
            tab.house.color = on ? Ink : Ink2;
            tab.usTag.text = ids[i] == me ? Loc.Get("league_us") : "";
        }

        RefreshUnits();
    }

    private void FillMatchSide(int teamId, Image crest, TMP_Text house, TMP_Text rank)
    {
        Team t = LeagueQuery.FindTeam(teamId);
        crest.sprite = LeagueQuery.TeamSprite(teamId);
        house.text = Loc.TeamName(t);
        rank.text = t != null && t.played > 0 ? Loc.Get("league_rank_short", t.rank) : "—";
    }

    // 용병 칸: 우리 가문은 보유 용병 전체(9명 넘으면 쪽 넘김), 다른 가문은 경기에서 만난 용병만
    private void RefreshUnits()
    {
        if (matches.Count == 0) return;
        var sel = matches[selectedMatch];
        int teamId = selectedSide == 0 ? sel.teamAId : sel.teamBId;
        Team team = LeagueQuery.FindTeam(teamId);
        bool mine = teamId == LeagueQuery.PlayerTeamId;

        var entries = new List<(string id, string name, int level)>();
        if (mine)
        {
            var units = UserManager.Instance != null && UserManager.Instance.user != null ? UserManager.Instance.user.myUnits : null;
            if (units != null) foreach (var u in units) entries.Add((u.Id, u.UnitName, u.Level));
        }
        else if (team != null && EnemySaveManager.Instance != null)
        {
            var seen = EnemySaveManager.Instance.GetSeenEnemiesByTeam(team.fid);
            if (seen != null) foreach (var e in seen) entries.Add((e.unitId, e.unitName, e.level));
        }

        int pages = Mathf.Max(1, Mathf.CeilToInt(entries.Count / (float)CellsPerPage));
        unitPage = Mathf.Clamp(unitPage, 0, pages - 1);

        unitsTitle.text = Loc.Get(mine ? "league_my_units" : "league_seen_units", entries.Count);
        unitsNote.gameObject.SetActive(!mine);
        unitsNote.text = Loc.Get("league_seen_note");
        pager.SetActive(mine && pages > 1);
        pageText.text = $"{unitPage + 1}/{pages}";
        prevPage.interactable = unitPage > 0;
        nextPage.interactable = unitPage < pages - 1;
        prevPage.image.sprite = unitPage > 0 ? iconOn : iconOff;
        nextPage.image.sprite = unitPage < pages - 1 ? iconOn : iconOff;

        var toLoad = new List<(Image image, string id)>();
        for (int k = 0; k < cells.Length; k++)
        {
            var cell = cells[k];
            int idx = unitPage * CellsPerPage + k;
            bool has = idx < entries.Count;
            cell.known.SetActive(has);
            // 다른 가문의 빈 칸은 "미확인", 우리 가문의 빈 칸은 비워 둔다
            cell.unknown.SetActive(!has && !mine);
            cell.bg.color = has ? Color.white : new Color(1, 1, 1, mine ? 0.35f : 0.7f);
            if (!has)
            {
                cell.unknownLabel.text = Loc.Get("league_unknown");
                continue;
            }
            var e = entries[idx];
            cell.label.text = $"<color=#754B22>Lv.{e.level}</color> {Loc.UnitName(e.id, e.name)}";
            cell.portrait.sprite = null;
            cell.portrait.enabled = false;
            toLoad.Add((cell.portrait, e.id));
        }

        if (portraitRoutine != null) StopCoroutine(portraitRoutine);
        portraitRoutine = isActiveAndEnabled ? StartCoroutine(LoadPortraits(toLoad)) : null;
    }

    private IEnumerator LoadPortraits(List<(Image image, string id)> list)
    {
        foreach (var item in list)
        {
            var image = item.image;
            yield return portraitLoader.LoadAsync(AddressableAssetType.Character, item.id, sprite =>
            {
                if (image == null) return;
                image.sprite = sprite;
                image.enabled = sprite != null;
            });
        }
        portraitRoutine = null;
    }
}
