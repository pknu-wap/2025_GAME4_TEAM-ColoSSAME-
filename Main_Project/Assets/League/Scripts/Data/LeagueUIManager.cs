using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using TMPro;

public class LeagueUIManager : MonoBehaviour
{
    public LeagueManager leagueManager;

    [Header("다음 경기 UI")]
    public Image[] teamAImages;
    public TextMeshProUGUI[] teamAInfoTexts;
    public Image[] teamBImages;
    public TextMeshProUGUI[] teamBInfoTexts;

    [Header("순위표 UI")]
    public TextMeshProUGUI[] teamNameTexts; // 순위 + 팀 이름 통합
    public Image[] teamImages;
    public TextMeshProUGUI[] playedTexts;
    public TextMeshProUGUI[] winTexts;
    public TextMeshProUGUI[] loseTexts;
    public TextMeshProUGUI[] drawTexts;
    public TextMeshProUGUI[] winRateTexts;

    void Start()
    {
        if (leagueManager == null)
            leagueManager = LeagueManager.Instance;

        leagueManager.CalculateRanking();
        UpdateAllUI();
    }

    public void UpdateAllUI()
    {
        UpdateNextMatchUI();
        UpdateRankingUI();
        ShowMyTeamNextMatch();
    }

    private void UpdateNextMatchUI()
    {
        int currentRound = GetCurrentRoundNumber();
        Round round = leagueManager.league.schedule
            .Find(r => r.roundNumber == currentRound);

        if (round == null)
        {
            Debug.LogWarning("❌ 현재 라운드 정보를 찾을 수 없습니다.");
            return;
        }

        for (int i = 0; i < teamAImages.Length; i++)
        {
            if (i < round.matches.Count)
            {
                LeagueMatch match = round.matches[i];
                Team teamA = leagueManager.league.teams.Find(t => t.id == match.teamAId);
                Team teamB = leagueManager.league.teams.Find(t => t.id == match.teamBId);

                teamAImages[i].gameObject.SetActive(true);
                teamBImages[i].gameObject.SetActive(true);

                teamAImages[i].sprite = GetTeamSprite(teamA.id);
                teamAInfoTexts[i].text = $"{teamA.name}\n{teamA.rank}등";

                teamBImages[i].sprite = GetTeamSprite(teamB.id);
                teamBInfoTexts[i].text = $"{teamB.name}\n{teamB.rank}등";
            }
            else
            {
                teamAImages[i].gameObject.SetActive(false);
                teamBImages[i].gameObject.SetActive(false);
            }
        }
    }

    private void UpdateRankingUI()
    {
        var sortedTeams = leagueManager.league.teams
            .OrderBy(t => t.rank)
            .ToList();

        for (int i = 0; i < sortedTeams.Count && i < teamNameTexts.Length; i++)
        {
            Team team = sortedTeams[i];

            // 순위 + 팀 이름 통합 표시
            teamNameTexts[i].text = $"{team.rank}등 {team.name}";
            teamImages[i].sprite = GetTeamSprite(team.id);
            playedTexts[i].text = team.played.ToString();
            winTexts[i].text = team.win.ToString();
            loseTexts[i].text = team.lose.ToString();
            drawTexts[i].text = team.draw.ToString();

            float winRate = team.played > 0 ? (float)team.win / team.played : 0;
            winRateTexts[i].text = $"{(winRate * 100f):0}%";
        }
    }

    private int GetCurrentRoundNumber()
    {
        int myTeamId = leagueManager.league.settings.playerTeamId;
        Team myTeam = leagueManager.league.teams.Find(t => t.id == myTeamId);

        if (myTeam != null)
        {
            return myTeam.played + 1; // played=0이면 1라운드
        }
        else
        {
            Debug.LogWarning("❌ 내 팀 정보를 찾을 수 없습니다.");
            return 1;
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
    
    public Image MyTeamImage;
    public TMP_Text MyTeamText;
    public Image nextMatchMyTeamImage;
    public TMP_Text nextMatchMyTeamText;
    public Image nextMatchOpponentImage;
    public TMP_Text nextMatchOpponentText;

    public void ShowMyTeamNextMatch()
    {
        int currentRound = GetCurrentRoundNumber();
        int playerTeamId = leagueManager.league.settings.playerTeamId;

        Round round = leagueManager.league.schedule
            .Find(r => r.roundNumber == currentRound);

        if (round == null)
        {
            Debug.LogWarning("❌ 현재 라운드 정보를 찾을 수 없습니다.");
            return;
        }

        LeagueMatch myMatch = round.matches
            .Find(m => m.teamAId == playerTeamId || m.teamBId == playerTeamId);

        if (myMatch == null)
        {
            Debug.LogWarning("❌ 이번 라운드에 내 팀 경기가 없습니다.");
            return;
        }

        Team myTeam = leagueManager.league.teams.Find(t => t.id == playerTeamId);
        Team opponentTeam = leagueManager.league.teams.Find(t =>
            t.id == (myMatch.teamAId == playerTeamId ? myMatch.teamBId : myMatch.teamAId));

        // 내 팀
        nextMatchMyTeamImage.sprite = GetTeamSprite(myTeam.id);
        MyTeamImage.sprite = GetTeamSprite(myTeam.id);
        MyTeamText.text = $"{myTeam.name}";
        nextMatchMyTeamText.text = $"{myTeam.name}\n{myTeam.rank}등";

        // 상대 팀
        nextMatchOpponentImage.sprite = GetTeamSprite(opponentTeam.id);
        nextMatchOpponentText.text = $"{opponentTeam.name}\n{opponentTeam.rank}등";
    }

}
