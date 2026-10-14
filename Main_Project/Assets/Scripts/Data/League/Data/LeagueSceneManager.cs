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
        Loc.Changed += HandleLanguageChanged;
    }

    void OnDestroy() => Loc.Changed -= HandleLanguageChanged;

    private int shownWinnerId;

    private void HandleLanguageChanged()
    {
        if (myTeam == null || opponentTeam == null) return;
        roundText.text = Loc.Get("match_round", currentRound.roundNumber);
        myTeamText.text = Loc.TeamName(myTeam);
        enemyTeamText.text = Loc.TeamName(opponentTeam);
        if (resultPanel.activeSelf) ShowResultUI(shownWinnerId);
    }

    void UpdateUI()
    {
        roundText.text = Loc.Get("match_round", currentRound.roundNumber);

        myTeamImage.sprite = leagueManager.GetTeamSprite(myTeam.id);
        myTeamText.text = Loc.TeamName(myTeam);

        enemyTeamImage.sprite = leagueManager.GetTeamSprite(opponentTeam.id);
        enemyTeamText.text = Loc.TeamName(opponentTeam);

        resultPanel.SetActive(false);
    }

    private enum PendingResult { None, Win, Lose, Draw }
    private PendingResult pending = PendingResult.None;

    public void OnClickWin()
    {
        pending = PendingResult.Win;             
        resultEnemyTeamImage.color = new Color(1, 1, 1, 0.3f);
        resultMyTeamImage.color = Color.white;
        ShowResultUI(myTeam.id);              
    }

    public void OnClickLose()
    {
        pending = PendingResult.Lose;
        resultMyTeamImage.color = new Color(1, 1, 1, 0.3f);
        resultEnemyTeamImage.color = Color.white;
        ShowResultUI(opponentTeam.id);
    }

    public void OnClickDraw()
    {
        pending = PendingResult.Draw;
        resultMyTeamImage.color = Color.white;
        resultEnemyTeamImage.color = Color.white;
        ShowResultUI(0);
    }

    public void OnClickReturn()
    {
        switch (pending)
        {
            case PendingResult.Win: leagueManager.ProcessRoundResult(true); break;
            case PendingResult.Lose: leagueManager.ProcessRoundResult(false); break;
            case PendingResult.Draw: leagueManager.ProcessRoundResult(true, isDraw: true); break;
        }
        pending = PendingResult.None;
        SceneTransition.Instance.Load("Book");
    }


    void ShowResultUI(int winnerId)
    {
        shownWinnerId = winnerId;
        resultRoundText.text = Loc.Get("match_round_result", currentRound.roundNumber);

        resultPanel.SetActive(true);
        resultMyTeamImage.sprite = leagueManager.GetTeamSprite(myTeam.id);
        resultMyTeamText.text = Loc.TeamName(myTeam);
        resultMyTeamResult.text = winnerId == 0 ? Loc.Get("result_draw") : (winnerId == myTeam.id ? Loc.Get("result_win") : Loc.Get("result_lose"));
        resultEnemyTeamImage.sprite = leagueManager.GetTeamSprite(opponentTeam.id);
        resultEnemyTeamText.text = Loc.TeamName(opponentTeam);
        resultEnemyTeamResult.text = winnerId == 0 ? Loc.Get("result_draw") : (winnerId == opponentTeam.id ? Loc.Get("result_win") : Loc.Get("result_lose"));

    }
}

