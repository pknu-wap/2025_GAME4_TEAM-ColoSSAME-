using System.Collections;
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

    [Header("배경 깃발")]
    [SerializeField] private Image backgroundBanner;
    [SerializeField] private BannerCloth backgroundCloth;
    [SerializeField] private Vector2 swayInterval = new Vector2(6f, 9f);

    // 임시: 값이 바뀌었다는 이벤트가 없는 항목(레벨·등급·가문)은 바뀌었는지만 비교해서 다시 그린다.
    private int shownTeam = -1, shownLevel = -1, shownTier = -1;
    private Coroutine sway;

    private void OnEnable()
    {
        Loc.Changed += Refresh;
        if (UserManager.Instance != null) UserManager.Instance.OnMoneyChanged += HandleMoneyChanged;
        Refresh();
        if (backgroundCloth != null) sway = StartCoroutine(Sway());
    }

    private void OnDisable()
    {
        Loc.Changed -= Refresh;
        if (UserManager.Instance != null) UserManager.Instance.OnMoneyChanged -= HandleMoneyChanged;
        if (sway != null) StopCoroutine(sway);
    }

    private void Update()
    {
        var user = UserManager.Instance != null ? UserManager.Instance.user : null;
        if (LeagueQuery.PlayerTeamId != shownTeam || LeagueQuery.Tier != shownTier || (user != null && user.level != shownLevel))
            Refresh();
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
            if (backgroundBanner != null)
            {
                var banner = LeagueQuery.TeamSprite(team.id);
                if (banner != null) backgroundBanner.sprite = banner;
            }
        }
        tier.text = LeagueQuery.IsReady ? Loc.Get("topbar_tier", shownTier, LeagueQuery.TierName) : "";

        var user = UserManager.Instance != null ? UserManager.Instance.user : null;
        if (user != null)
        {
            shownLevel = user.level;
            level.text = Loc.Get("topbar_level", user.level);
            SetGold(user.money);
        }
    }

    private void SetGold(int money) => gold.text = Loc.Get("topbar_gold", money.ToString("N0"));

    // 배경 깃발: 몇 초마다 한 번씩 천이 흔들린다 (가문 선택 화면과 같은 BannerCloth)
    private IEnumerator Sway()
    {
        float direction = 1f;
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(swayInterval.x, swayInterval.y));
            backgroundCloth.PlayArrival(direction);
            direction = -direction;
        }
    }
}
