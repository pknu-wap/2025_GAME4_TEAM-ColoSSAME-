using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BattleSceneManager : MonoBehaviour
{
    public TournamentSaveManager saveManager;
    public TournamentController tournamentController;

    private string myTeamKey = "Team01";
    private string enemyTeamKey;
    private Match currentMatch;

    [Header("우리 팀 (좌측)")]
    public Image myTeamImage;
    public TextMeshProUGUI myTeamText;

    [Header("상대 팀 (우측)")]
    public Image enemyTeamImage;
    public TextMeshProUGUI enemyTeamText;

    [Header("결과창 - 우리 팀")]
    public Image resultMyTeamImage;
    public TextMeshProUGUI resultMyTeamText;
    public TextMeshProUGUI resultMyTeamResult;

    [Header("결과창 - 상대 팀")]
    public Image resultEnemyTeamImage;
    public TextMeshProUGUI resultEnemyTeamText;
    public TextMeshProUGUI resultEnemyTeamResult;

    [Header("결과창 오브젝트")]
    public GameObject resultPanel;

    void Start()
    {
        TournamentData data = saveManager.LoadTournament();
        currentMatch = FindMyCurrentMatch(data);

        if (currentMatch == null)
        {
            Debug.LogError("❌ 현재 매치 없음.");
            return;
        }

        enemyTeamKey = (currentMatch.player1Key == myTeamKey) ? currentMatch.player2Key : currentMatch.player1Key;

        myTeamImage.sprite = LoadTeamSprite(myTeamKey);
        myTeamText.text = GetTeamDisplayName(myTeamKey);

        enemyTeamImage.sprite = LoadTeamSprite(enemyTeamKey);
        enemyTeamText.text = GetTeamDisplayName(enemyTeamKey);

        resultPanel.SetActive(false); // 처음에는 결과창 숨김
    }

    // 버튼으로 호출 (우리팀 승리)
    public void OnWin()
    {
        ApplyResult(myTeamKey);
    }

    // 버튼으로 호출 (우리팀 패배)
    public void OnLose()
    {
        ApplyResult(enemyTeamKey);
    }

    void ApplyResult(string winnerKey)
    {
        Debug.Log($"⚔️ 승자 처리: {winnerKey}");

        if (currentMatch == null)
        {
            Debug.LogError("⛔ currentMatch가 null");
            return;
        }

        // 1. 현재 라운드에 따라 승자 저장
        TournamentData data = saveManager.LoadTournament();

        if (data.finalMatch == currentMatch)
        {
            tournamentController.SetFinalWinner(winnerKey);
        }
        else if (data.semiFinals.Contains(currentMatch))
        {
            int index = data.semiFinals.IndexOf(currentMatch);
            tournamentController.SetSemiFinalWinner(index, winnerKey);
        }
        else if (data.quarterFinals.Contains(currentMatch))
        {
            int index = data.quarterFinals.IndexOf(currentMatch);
            tournamentController.SetQuarterFinalWinner(index, winnerKey);
        }

        // 2. 결과창 UI 업데이트
        resultMyTeamImage.sprite = myTeamImage.sprite;
        resultMyTeamText.text = myTeamText.text;

        resultEnemyTeamImage.sprite = enemyTeamImage.sprite;
        resultEnemyTeamText.text = enemyTeamText.text;

        if (winnerKey == myTeamKey)
        {
            resultMyTeamResult.text = "승";
            resultEnemyTeamResult.text = "패";

            resultEnemyTeamImage.color = new Color(1, 1, 1, 0.3f); // 흐림
            resultMyTeamImage.color = Color.white;

            tournamentController.AutoResolveRemainingMatches();
        }
        else
        {
            resultMyTeamResult.text = "패";
            resultEnemyTeamResult.text = "승";

            resultMyTeamImage.color = new Color(1, 1, 1, 0.3f); // 흐림
            resultEnemyTeamImage.color = Color.white;

            Debug.Log("💀 우리 팀 패배 - 게임 오버");
        }

        tournamentController.SaveTournament();
        resultPanel.SetActive(true); // 결과창 켜기
    }

    Match FindMyCurrentMatch(TournamentData data)
    {
        foreach (var match in data.quarterFinals)
            if ((match.player1Key == myTeamKey || match.player2Key == myTeamKey) && string.IsNullOrEmpty(match.winnerKey))
                return match;

        foreach (var match in data.semiFinals)
            if ((match.player1Key == myTeamKey || match.player2Key == myTeamKey) && string.IsNullOrEmpty(match.winnerKey))
                return match;

        if (data.finalMatch != null &&
            (data.finalMatch.player1Key == myTeamKey || data.finalMatch.player2Key == myTeamKey) &&
            string.IsNullOrEmpty(data.finalMatch.winnerKey))
            return data.finalMatch;

        return null;
    }

    Sprite LoadTeamSprite(string key)
    {
        if (!key.StartsWith("Team"))
            key = $"Team{key.PadLeft(2, '0')}";
        return Resources.Load<Sprite>($"TeamImages/{key}");
    }

    string GetTeamDisplayName(string key)
    {
        if (!key.StartsWith("Team"))
            key = $"Team{key.PadLeft(2, '0')}";
        return $"팀 {key.Substring(4)}";
    }
}
