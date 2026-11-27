using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Book 씬 상단 바 표시 전용 (모든 책갈피 페이지에 고정).
// 가문·리그는 LeagueQuery, 레벨·골드는 UserManager 값을 읽기만 한다.
public class TopBarView : MonoBehaviour
{
    [Header("왼쪽: 가문")]
    [SerializeField] private Image crest;
    [SerializeField] private Sprite[] crestSprites = new Sprite[10]; // 팀 id 1~10 순서
    [SerializeField] private TMP_Text houseName;
    [SerializeField] private TMP_Text level;

    [Header("가운데: 리그 등급")]
    [SerializeField] private TMP_Text tier;

    [Header("오른쪽: 골드")]
    [SerializeField] private TMP_Text gold;

    [SerializeField] private Image strategyKey;

    // 임시: 값이 바뀌었다는 이벤트가 없는 항목(레벨·등급·가문)은 바뀌었는지만 비교해서 다시 그린다.
    private int shownTeam = -1, shownLevel = -1, shownTier = -1, shownMoney = -1;

    private void OnEnable()
    {
        Loc.Changed += Refresh;
        if (UserManager.Instance != null) UserManager.Instance.OnMoneyChanged += HandleMoneyChanged;
        Refresh();
    }

    private void OnDisable()
    {
        Loc.Changed -= Refresh;
        if (UserManager.Instance != null) UserManager.Instance.OnMoneyChanged -= HandleMoneyChanged;
    }

    private void Update()
    {
        var user = UserManager.Instance != null ? UserManager.Instance.user : null;
        if (LeagueQuery.PlayerTeamId != shownTeam || LeagueQuery.Tier != shownTier || (user != null && user.level != shownLevel))
            Refresh();
        // OnEnable 때 UserManager가 아직 없으면 골드 이벤트를 못 받으므로 값 비교로 한 번 더 확인
        else if (user != null && user.money != shownMoney)
            SetGold(user.money);
    }

    private void HandleMoneyChanged(int money) => SetGold(money);

    public void Refresh()
    {
        Team team = LeagueQuery.PlayerTeam;
        shownTeam = LeagueQuery.PlayerTeamId;
        shownTier = LeagueQuery.Tier;
        if (team != null)
        {
            houseName.text = Loc.Get("home_team", Loc.TeamName(team));
            int i = team.id - 1;
            if (i >= 0 && i < crestSprites.Length && crestSprites[i] != null) crest.sprite = crestSprites[i];
        }
        StrategyRoomKey.Display(strategyKey, team?.fid);
        tier.text = LeagueQuery.IsReady ? Loc.Get("topbar_tier", shownTier, LeagueQuery.TierName) : "";

        var user = UserManager.Instance != null ? UserManager.Instance.user : null;
        if (user != null)
        {
            shownLevel = user.level;
            level.text = Loc.Get("topbar_level", user.level);
            SetGold(user.money);
        }
    }

    private void SetGold(int money)
    {
        shownMoney = money;
        gold.text = Loc.Get("topbar_gold", money.ToString("N0"));
    }
}
