using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class LeagueSceneManager : MonoBehaviour
{
    public LeagueManager leagueManager;

    private Round currentRound;
    private LeagueMatch myMatch;
    private Team myTeam;
    private Team opponentTeam;

    [Header("UI")]
    public TMP_Text roundText;
    public TMP_Text resultRoundText;
    public Image myTeamImage;
    public TMP_Text myTeamText;
    public Image enemyTeamImage;
    public TMP_Text enemyTeamText;
    public GameObject resultPanel;
    public Image resultMyTeamImage;
    public TMP_Text resultMyTeamText;
    public TMP_Text resultMyTeamResult;
    public Image resultEnemyTeamImage;
    public TMP_Text resultEnemyTeamText;
    public TMP_Text resultEnemyTeamResult;

    void Start()
    {
        leagueManager = LeagueManager.Instance;
        myTeam = leagueManager.league.teams.Find(t => t.id == leagueManager.league.settings.playerTeamId);
        currentRound = leagueManager.league.schedule.Find(r => r.roundNumber == myTeam.played + 1);
        myMatch = currentRound.matches.Find(m => m.teamAId == myTeam.id || m.teamBId == myTeam.id);

        opponentTeam = leagueManager.league.teams.Find(t =>
            t.id == (myMatch.teamAId == myTeam.id ? myMatch.teamBId : myMatch.teamAId));

        UpdateUI();
    }

    void UpdateUI()
    {
        roundText.text = $"{currentRound.roundNumber}라운드";

        myTeamImage.sprite = leagueManager.GetTeamSprite(myTeam.id);
        myTeamText.text = myTeam.name;

        enemyTeamImage.sprite = leagueManager.GetTeamSprite(opponentTeam.id);
        enemyTeamText.text = opponentTeam.name;

        resultPanel.SetActive(false);
    }

    public void OnClickWin()
    {
        leagueManager.ProcessRoundResult(true);
        resultEnemyTeamImage.color = new Color(1, 1, 1, 0.3f);
        resultMyTeamImage.color = Color.white;
        ShowResultUI(myTeam.id);
    }

    public void OnClickLose()
    {
        leagueManager.ProcessRoundResult(false);
        resultMyTeamImage.color = new Color(1, 1, 1, 0.3f);
        resultEnemyTeamImage.color = Color.white;
        ShowResultUI(opponentTeam.id);
    }
    

    void ShowResultUI(int winnerId)
    {
        resultRoundText.text = $"{currentRound.roundNumber}라운드 결과";

        resultPanel.SetActive(true);
        resultMyTeamImage.sprite = leagueManager.GetTeamSprite(myTeam.id);
        resultMyTeamText.text = myTeam.name;
        resultMyTeamResult.text = winnerId == myTeam.id ? "승" : "패";
        resultEnemyTeamImage.sprite = leagueManager.GetTeamSprite(opponentTeam.id);
        resultEnemyTeamText.text = opponentTeam.name;
        resultEnemyTeamResult.text = winnerId == opponentTeam.id ? "승" : "패";
        
    }
}

