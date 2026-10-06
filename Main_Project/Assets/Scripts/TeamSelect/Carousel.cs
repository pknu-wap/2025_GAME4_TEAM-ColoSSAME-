using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;

public class Carousel : MonoBehaviour
{
    public List<RectTransform> cards;
    public RectTransform backGround;
    public Vector2[] positions;
    int currentIndex = 0;
    int numCards;

    [Header("원근감 (y좌표로 깊이 계산)")]
    [SerializeField] private float frontScale = 1.5f;                   // 맨 앞 카드 크기
    [SerializeField] private float backScale = 0.75f;                   // 맨 뒤 카드 크기
    [SerializeField, Range(0f, 1f)] private float backBrightness = 0.45f; // 맨 뒤 카드 밝기
    [SerializeField] private float moveDuration = 0.3f;

    // 카드 안 그래픽들의 원래 색 (어둡게 할 때 기준)
    private readonly Dictionary<Graphic, Color> baseColors = new();
    public LeagueManager leagueManager;
    public Image teamImages;
    public TMP_Text teamName;
    public TMP_Text teamText;
    
    public TeamDetailViewer teamDetailViewer;

    [Header("팀 선택 확인")]
    [SerializeField] private ConfirmPopup confirmPopup;   // 비어 있으면 확인 없이 바로 시작
    [SerializeField] private SceneLoader sceneLoader;

    void Start()
    {
        if (leagueManager == null)
            leagueManager = LeagueManager.Instance;
        numCards = cards.Count;
        UpdateCards(instant: true);   // 카드 먼저 (설명 쪽이 실패해도 배치는 되도록), 첫 배치는 즉시
        UpdateExplanation();
    }

    public void Rotate(int dir) // dir: -1 (왼쪽), +1 (오른쪽)
    {
        currentIndex = (currentIndex + dir + numCards) % numCards;
        UpdateCards(instant: false);
        UpdateExplanation();
    }

    int PosIndexOf(int cardIndex) => (cardIndex - currentIndex + numCards) % numCards;

    void UpdateCards(bool instant)
    {
        // y가 작을수록 앞(화면 아래), 클수록 뒤
        float minY = float.MaxValue, maxY = float.MinValue;
        foreach (var p in positions)
        {
            minY = Mathf.Min(minY, p.y);
            maxY = Mathf.Max(maxY, p.y);
        }

        // 1. 그리는 순서: 뒤 카드부터 → 앞 카드가 항상 위에 (선택 카드가 가려지지 않게)
        int baseSibling = int.MaxValue;
        foreach (var c in cards) baseSibling = Mathf.Min(baseSibling, c.GetSiblingIndex());

        var order = new List<int>();
        for (int i = 0; i < numCards; i++) order.Add(i);
        order.Sort((a, b) => positions[PosIndexOf(b)].y.CompareTo(positions[PosIndexOf(a)].y));
        for (int s = 0; s < order.Count; s++) cards[order[s]].SetSiblingIndex(baseSibling + s);

        // 2. 위치 / 크기 / 밝기를 깊이에 맞춰 (첫 배치는 즉시, 회전은 애니메이션)
        for (int i = 0; i < numCards; i++)
        {
            int posIndex = PosIndexOf(i);
            Vector2 pos = positions[posIndex];
            float depth = Mathf.InverseLerp(minY, maxY, pos.y);   // 0 = 맨 앞, 1 = 맨 뒤
            Vector3 scale = Vector3.one * Mathf.Lerp(frontScale, backScale, depth);

            cards[i].DOKill();
            if (instant)
            {
                cards[i].anchoredPosition = pos;
                cards[i].localScale = scale;
            }
            else
            {
                cards[i].DOAnchorPos(pos, moveDuration).SetEase(Ease.OutQuad);
                cards[i].DOScale(scale, moveDuration).SetEase(Ease.OutQuad);
            }
            Tint(cards[i], Mathf.Lerp(1f, backBrightness, depth), instant);

            Image cardImage = cards[i].GetComponent<Image>();
            cardImage.material = posIndex == 0
                ? ShaderController.Instance.bannerOutlineMaterial   // 선택된 카드
                : ShaderController.Instance.normalOutlineMaterial;
        }
    }

    // 카드와 자식 그래픽 전부를 원래 색 × 밝기로
    void Tint(RectTransform card, float brightness, bool instant)
    {
        foreach (var g in card.GetComponentsInChildren<Graphic>(true))
        {
            if (!baseColors.TryGetValue(g, out var baseColor))
                baseColors[g] = baseColor = g.color;

            var target = new Color(baseColor.r * brightness, baseColor.g * brightness, baseColor.b * brightness, baseColor.a);
            g.DOKill();
            if (instant) g.color = target;
            else g.DOColor(target, moveDuration);
        }
    }
    
    private Sprite GetTeamSprite(int teamId)
    {
        string path = $"TeamImages/team_{teamId}";
        Sprite sprite = Resources.Load<Sprite>(path);

        if (sprite == null) 
        {
            Debug.LogWarning($"팀 스프라이트를 찾을 수 없습니다: {path}");
        }

        return sprite;
    }

    void UpdateExplanation()
    {
        Team myTeam = leagueManager.league.teams.Find(t => t.id == currentIndex+1);
        teamImages.sprite = GetTeamSprite(myTeam.id);
        teamName.text = myTeam.name;
        teamText.text =  myTeam.explanation;
    }

    // ✓ 버튼: 되돌릴 수 없는 선택이라 확인부터 받는다
    public void OpenTeamConfirm()
    {
        if (confirmPopup == null)
        {
            ConfirmTeamSelect();
            return;
        }

        Team team = leagueManager.league.teams.Find(t => t.id == currentIndex + 1);
        confirmPopup.Show($"{team.name} 가문으로 시작할까요?\n시작하면 가문을 바꿀 수 없습니다.", ConfirmTeamSelect);
    }

    private void ConfirmTeamSelect()
    {
        TeamSelect();
        sceneLoader.LoadScene();
    }

    public void TeamSelect()
    {
        Team myTeam = leagueManager.league.teams.Find(t => t.id == currentIndex+1);
        leagueManager.league.settings.playerTeamId = myTeam.id;
        leagueManager.league.settings.playerTeamName = myTeam.name;
        leagueManager.saveManager.SaveLeague(leagueManager.league);
        if (UserManager.Instance != null) 
        {
            UserManager.Instance.AddInitialUnitsByFamily(myTeam.fid);
        }
        else 
        {
            Debug.LogError("UserManager 인스턴스를 찾을 수 없습니다.");
        }
        EnemySaveManager.Instance.Clear();
        EnemyTeamService.InitializeFromLeague(leagueManager.league);

        // 백업
        leagueManager.BackupLeagueStart();

        leagueManager.RefreshCurrentMatchInfo();

        Debug.Log($"팀 선택 완료: {myTeam.name}");
    }

    public void OnViewStatusButtonClick()
    {
        Team selectedTeam = leagueManager.league.teams.Find(t => t.id == currentIndex + 1);
        string familyId = selectedTeam.fid; // 가문 ID (예: "Caelus")

        // TeamDetailViewer 스크립트의 함수를 호출하여 가문 ID를 전달
        if (teamDetailViewer != null)
        {
            teamDetailViewer.ShowDetails(familyId);
        }
    }

}