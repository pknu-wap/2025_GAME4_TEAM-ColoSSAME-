using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;

public class Carousel : MonoBehaviour
{
    public List<RectTransform> cards;
    public RectTransform backGround;

    [System.Serializable]
    private struct HouseColor
    {
        public string familyId;
        public Color color;
        [Range(0, 9)] public int crestSlot;
    }

    [Header("무기고 가문 색상")]
    [SerializeField] private Image armoryAccents;
    [SerializeField] private HouseColor[] houseColors;
    private string displayedFamily;
    private Material sharedAccentMaterial;
    private Material accentMaterial;
    private static readonly int FromAge = Shader.PropertyToID("_FromAge");
    private static readonly int FromOpacity = Shader.PropertyToID("_FromOpacity");
    private static readonly int FromCrest = Shader.PropertyToID("_FromCrest");
    private static readonly int ToCrest = Shader.PropertyToID("_ToCrest");
    private static readonly int Elapsed = Shader.PropertyToID("_Elapsed");
    // 방패 문장 바뀜·천 흔들림 전체 길이. ArmoryAccents.shader의 0.76·0.75(문장 붓질, CrestMotion 24프레임), 0.70+0.06(천 흔들림)과 함께 바꿀 것.
    private const float CrestSwapDuration = 0.76f;

    [Header("가문 전환 효과")]
    [Tooltip("방패·천이 흰색 쪽으로 빛나는 정도")]
    [SerializeField, Range(0f, 1f)] private float flashAmount = 0.4f;
    [SerializeField] private float flashIn = 0.1f;
    [SerializeField] private float flashOut = 0.3f;
    [SerializeField] private float punchScale = 1.1f;
    [SerializeField] private float punchDuration = 0.25f;
    [SerializeField] private Image houseHalo;
    [SerializeField, Range(0f, 1f)] private float haloAlpha = 0.45f;
    [SerializeField] private Image lampGlow;
    [SerializeField, Range(0f, 1f)] private float lampAlpha = 0.5f;
    [SerializeField] private Image floorGlow;
    [SerializeField, Range(0f, 1f)] private float floorAlpha = 0.5f;
    [SerializeField] private float lightDuration = 0.2f;

    [Header("가문 선택")]
    [SerializeField] private Vector2[] positions;
    [SerializeField] private Button previousButton;
    [SerializeField] private Button nextButton;
    [SerializeField] private TMP_Text selectionIndex;
    [SerializeField] private float moveDuration = 0.3f;
    [Tooltip("가운데, ±1, ±2 깃발 높이")]
    [SerializeField] private float[] bannerHeights = { 310f, 200f, 145f };
    [Tooltip("가운데, ±1, ±2 깃발 밝기")]
    [SerializeField] private float[] bannerBrightness = { 1f, 0.85f, 0.74f };

    private int currentIndex;
    public LeagueManager leagueManager;
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
        UpdateCards(instant: true);   // 카드 먼저 (설명 쪽이 실패해도 배치는 되도록), 첫 배치는 즉시
        UpdateExplanation();
        Loc.Changed += UpdateExplanation;
    }

    void OnDestroy()
    {
        Loc.Changed -= UpdateExplanation;
        if (accentMaterial == null) return;
        accentMaterial.DOKill();
        if (armoryAccents != null) armoryAccents.material = sharedAccentMaterial;
        Destroy(accentMaterial);
    }

    public void Rotate(int dir) // dir: -1 (왼쪽), +1 (오른쪽)
    {
        int index = Mathf.Clamp(currentIndex + dir, 0, cards.Count - 1);
        if (index == currentIndex) return;
        currentIndex = index;
        UpdateCards(instant: false);
        UpdateExplanation();
    }

    private void UpdateCards(bool instant)
    {
        int center = positions.Length / 2;
        if (previousButton != null) previousButton.interactable = currentIndex > 0;
        if (nextButton != null) nextButton.interactable = currentIndex < cards.Count - 1;
        if (selectionIndex != null) selectionIndex.text = $"{currentIndex + 1} / {cards.Count}";

        for (int i = 0; i < cards.Count; i++)
        {
            var card = cards[i];
            var image = card.GetComponent<Image>();
            int distance = Mathf.Abs(i - currentIndex);
            int slot = i - currentIndex + center;
            card.DOKill();
            image.DOKill();
            if (slot < 0 || slot >= positions.Length)
            {
                card.gameObject.SetActive(false);
                continue;
            }

            bool snap = instant || !card.gameObject.activeSelf;
            card.gameObject.SetActive(true);
            float targetHeight = bannerHeights[Mathf.Min(distance, bannerHeights.Length - 1)];
            float scale = ScaleForHeight(image, targetHeight);
            float brightness = bannerBrightness[Mathf.Min(distance, bannerBrightness.Length - 1)];
            var color = new Color(brightness, brightness, brightness, 1f);
            if (snap)
            {
                card.anchoredPosition = positions[slot];
                card.localScale = Vector3.one * scale;
                image.color = color;
            }
            else
            {
                card.DOAnchorPos(positions[slot], moveDuration).SetEase(Ease.OutQuad);
                var grow = DOTween.Sequence().SetTarget(card)
                    .Append(card.DOScale(scale, moveDuration).SetEase(Ease.OutQuad));
                if (distance == 0 && punchScale > 1f)   // 새 가운데 깃발: 도착 후 살짝 튀어오름
                    grow.Append(card.DOScale(scale * punchScale, punchDuration * 0.35f).SetEase(Ease.OutQuad))
                        .Append(card.DOScale(scale, punchDuration * 0.65f).SetEase(Ease.OutQuad));
                image.DOColor(color, moveDuration);
            }
        }

        if (houseHalo != null) houseHalo.rectTransform.anchoredPosition = positions[center];
    }

    private static float ScaleForHeight(Image image, float targetHeight)
    {
        var box = image.rectTransform.rect;
        var sprite = image.overrideSprite;
        float fittedHeight = box.height;
        if (image.preserveAspect && sprite != null && sprite.rect.width > 0f)
        {
            float heightAtBoxWidth = box.width * sprite.rect.height / sprite.rect.width;
            fittedHeight = Mathf.Min(box.height, heightAtBoxWidth);
        }
        return fittedHeight > 0f ? targetHeight / fittedHeight : 1f;
    }

    public void ReturnToMainMenu()
    {
        SceneTransition.Instance.Load("MainMenu");
    }

    void UpdateExplanation()
    {
        Team myTeam = leagueManager.league.teams.Find(t => t.id == currentIndex+1);
        UpdateArmoryColor(myTeam.fid);
        teamName.text = Loc.TeamName(myTeam);
        teamText.text = Loc.TeamDescription(myTeam);
    }

    private void UpdateArmoryColor(string familyId)
    {
        if (armoryAccents == null || houseColors == null || displayedFamily == familyId) return;

        bool animate = armoryAccents.enabled;
        armoryAccents.DOKill();
        displayedFamily = familyId;
        foreach (var house in houseColors)
        {
            if (house.familyId != familyId) continue;
            ShowShieldCrest(house.crestSlot, animate);
            armoryAccents.enabled = true;
            if (animate)
            {
                var peak = Color.Lerp(house.color, Color.white, flashAmount);
                DOTween.Sequence().SetTarget(armoryAccents)
                    .Append(armoryAccents.DOColor(peak, flashIn).SetEase(Ease.OutQuad))
                    .Append(armoryAccents.DOColor(house.color, flashOut).SetEase(Ease.OutQuad));
            }
            else armoryAccents.color = house.color;
            Glow(houseHalo, house.color, haloAlpha, animate ? moveDuration : 0f);
            Glow(lampGlow, house.color, lampAlpha, animate ? lightDuration : 0f);
            Glow(floorGlow, house.color, floorAlpha, animate ? lightDuration : 0f);
            return;
        }

        armoryAccents.enabled = false;
        Glow(houseHalo, Color.clear, 0f, 0f);
        Glow(lampGlow, Color.clear, 0f, 0f);
        Glow(floorGlow, Color.clear, 0f, 0f);
    }

    private void ShowShieldCrest(int slot, bool animate)
    {
        if (accentMaterial == null)
        {
            sharedAccentMaterial = armoryAccents.material;
            if (!sharedAccentMaterial.HasProperty(Elapsed)) return;
            accentMaterial = new Material(sharedAccentMaterial);
            armoryAccents.material = accentMaterial;
            animate = false;
        }

        // Keep only the painted portion when a second arrow press interrupts a stroke.
        float age = accentMaterial.GetFloat(Elapsed);
        int selected = Mathf.RoundToInt(accentMaterial.GetFloat(ToCrest));
        bool useOutgoing = animate && age < 0.14f;
        float previous = useOutgoing ? accentMaterial.GetFloat(FromCrest) : selected;
        float previousAge = useOutgoing ? accentMaterial.GetFloat(FromAge) : age;
        float opacity = useOutgoing
            ? accentMaterial.GetFloat(FromOpacity) * (1f - Mathf.SmoothStep(0f, 1f, age / 0.15f))
            : 1f;
        accentMaterial.DOKill();
        accentMaterial.SetFloat(FromCrest, animate ? previous : slot);
        accentMaterial.SetFloat(FromAge, animate ? previousAge : CrestSwapDuration);
        accentMaterial.SetFloat(FromOpacity, opacity);
        accentMaterial.SetFloat(ToCrest, slot);
        accentMaterial.SetFloat(Elapsed, animate ? 0f : CrestSwapDuration);
        if (animate)
            accentMaterial.DOFloat(CrestSwapDuration, Elapsed, CrestSwapDuration).SetEase(Ease.Linear);
    }

    private static void Glow(Image glow, Color color, float alpha, float duration)
    {
        if (glow == null) return;
        glow.DOKill();
        color.a = alpha;
        if (duration > 0f) glow.DOColor(color, duration).SetEase(Ease.OutQuad);
        else glow.color = color;
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
        confirmPopup.Show(Loc.Get("teamselect_confirm", Loc.TeamName(team)), ConfirmTeamSelect);
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