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
    public float[] scales;
    int currentIndex = 0;
    int numCards;
    public LeagueManager leagueManager;
    public Image teamImages;
    public TMP_Text teamName;
    public TMP_Text teamText;
    
    public TeamDetailViewer teamDetailViewer;

    void Start()
    {
        if (leagueManager == null)
            leagueManager = LeagueManager.Instance;
        numCards = cards.Count;
        UpdateExplanation();
        UpdateCards();
    }

    public void Rotate(int dir) // dir: -1 (왼쪽), +1 (오른쪽)
    {
        currentIndex = (currentIndex + dir + numCards) % numCards;
        UpdateCards();
        UpdateExplanation();
    }

    void UpdateCards()
    {
        List<RectTransform> frontCards = new List<RectTransform>();
        List<RectTransform> backCards = new List<RectTransform>();

        // ➡️ 1. 카드 그룹 분리
        for (int i = 0; i < numCards; i++)
        {
            int posIndex = (i - currentIndex + numCards) % numCards;
            float yValue = positions[posIndex].y;

            if (yValue <= 15)
                frontCards.Add(cards[i]);
            else
                backCards.Add(cards[i]);
        }

        // ➡️ 2. sibling index 먼저 재설정
        for (int i = 0; i < backCards.Count; i++)
        {
            backCards[i].SetSiblingIndex(i);
        }

        foreach (var card in frontCards)
        {
            card.SetAsLastSibling();
        }

        // ➡️ 3. 그 후 DOTween 애니메이션 실행
        for (int i = 0; i < numCards; i++)
        {
            int posIndex = (i - currentIndex + numCards) % numCards;

            cards[i].DOAnchorPos(positions[posIndex], 0.3f).SetEase(Ease.OutQuad);
            cards[i].DOScale(Vector3.one * scales[posIndex], 0.3f).SetEase(Ease.OutQuad);
            Image cardImage = cards[i].GetComponent<Image>();
            if (positions[posIndex].x == 0 && positions[posIndex].y < -30)
            {
                cardImage.material = ShaderController.Instance.bannerOutlineMaterial;
                Debug.Log("적용함");
            }
            else
            {
                cardImage.material = ShaderController.Instance.normalOutlineMaterial;
            }
        }
    }
    
    private Sprite GetTeamSprite(int teamId)
    {
        string path = $"TeamImages/team_{teamId}";
        Sprite sprite = Resources.Load<Sprite>(path);

        if (sprite == null)
        {
            Debug.LogWarning($"❌ 팀 스프라이트를 찾을 수 없습니다: {path}");
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

    public void TeamSelect()
    {
        Team myTeam = leagueManager.league.teams.Find(t => t.id == currentIndex+1);
        leagueManager.league.settings.playerTeamId = myTeam.id;
        leagueManager.league.settings.playerTeamName = myTeam.name;
        leagueManager.saveManager.SaveLeague(leagueManager.league);
        if (UserManager.Instance != null)
        {
            UserManager.Instance.AddInitialUnitsByFamily(myTeam.name);
        }
        else
        {
            Debug.LogError("❌ UserManager 인스턴스를 찾을 수 없습니다.");
        }
    }

    public void OnViewStatusButtonClick()
    {
        Team selectedTeam = leagueManager.league.teams.Find(t => t.id == currentIndex + 1);
        string familyId = selectedTeam.fid; // 가문 ID (예: "Caelus")

        // ➡️ TeamDetailViewer 스크립트의 함수를 호출하여 가문 ID를 전달
        if (teamDetailViewer != null)
        {
            teamDetailViewer.ShowDetails(familyId);
        }
    }

}