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
    }

    [Header("무기고 가문 색상")]
    [SerializeField] private Image armoryAccents;
    [SerializeField] private HouseColor[] houseColors;
    private string displayedFamily;

    [Header("가문 선택")]
    [SerializeField] private Vector2[] positions;
    [SerializeField] private Button previousButton;
    [SerializeField] private Button nextButton;
    [SerializeField] private TMP_Text selectionIndex;
    [SerializeField] private float moveDuration = 0.3f;

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

    void OnDestroy() => Loc.Changed -= UpdateExplanation;

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
        previousButton.interactable = currentIndex > 0;
        nextButton.interactable = currentIndex < cards.Count - 1;
        selectionIndex.text = $"{currentIndex + 1} / {cards.Count}";

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
            float targetHeight = distance == 0 ? 310f : distance == 1 ? 200f : 145f;
            float scale = ScaleForHeight(image, targetHeight);
            float brightness = distance == 0 ? 1f : distance == 1 ? 0.85f : 0.74f;
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
                card.DOScale(scale, moveDuration).SetEase(Ease.OutQuad);
                image.DOColor(color, moveDuration);
            }
        }
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
            armoryAccents.enabled = true;
            if (animate) armoryAccents.DOColor(house.color, moveDuration).SetEase(Ease.OutQuad);
            else armoryAccents.color = house.color;
            return;
        }

        armoryAccents.enabled = false;
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